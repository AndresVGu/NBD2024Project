using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.Utilities;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Sales,Designer,Guest")]
    public class WorkOrdersController : ElephantController
    {
        private readonly NBDContext _context;

        public WorkOrdersController(NBDContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? projectID, WorkOrderStatus? status, int? page, int? pageSizeID)
        {
            var workOrders = _context.WorkOrders
                .Include(w => w.Project)
                .ThenInclude(p => p.Client)
                .AsNoTracking()
                .AsQueryable();

            if (IsGuestMode())
            {
                string owner = CurrentOwnerName();
                workOrders = workOrders.Where(w => w.CreatedBy == owner);
            }

            if (projectID.HasValue)
            {
                workOrders = workOrders.Where(w => w.ProjectID == projectID.Value);
            }

            if (status.HasValue)
            {
                workOrders = workOrders.Where(w => w.Status == status.Value);
            }

            workOrders = workOrders.OrderBy(w => w.ScheduledDate).ThenBy(w => w.Title);

            PopulateProjectSelectList(projectID);
            ViewData["Status"] = new SelectList(Enum.GetValues(typeof(WorkOrderStatus)).Cast<WorkOrderStatus>(), status);

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<WorkOrder>.CreateAsync(workOrders, page ?? 1, pageSize);
            return View(pagedData);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrder = await _context.WorkOrders
                .Include(w => w.Project)
                .ThenInclude(p => p.City)
                .Include(w => w.Project)
                .ThenInclude(p => p.Client)
                .Include(w => w.CrewAssignments)
                .ThenInclude(a => a.Employee)
                .Include(w => w.MaterialConsumptions)
                .ThenInclude(c => c.Material)
                .Include(w => w.MaterialConsumptions)
                .ThenInclude(c => c.StockLocation)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ID == id);

            if (workOrder == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrder.CreatedBy))
            {
                return Forbid();
            }

            var employees = await _context.Employees
                .Include(e => e.EmployeeSkills)
                .Where(e => e.Active)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new
                {
                    e.ID,
                    Display = e.FirstName + " " + e.LastName + " - " + string.Join(", ", e.EmployeeSkills.Select(s => s.Skill.ToString()))
                })
                .ToListAsync();

            ViewData["EmployeeID"] = new SelectList(employees, "ID", "Display");
            ViewData["Skill"] = new SelectList(Enum.GetValues(typeof(EmployeeSkillType)).Cast<EmployeeSkillType>());
            ViewData["MaterialID"] = new SelectList(await _context.Materials.OrderBy(m => m.Name).ToListAsync(), "ID", "Name");
            ViewData["StockLocationID"] = new SelectList(await _context.StockLocations.Where(l => l.IsActive).OrderBy(l => l.Name).ToListAsync(), "ID", "Name");

            return View(workOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordConsumption(int id, int materialID, int stockLocationID,
            DateTime consumedOn, double quantityUsed, string notes)
        {
            var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == id);
            if (workOrder == null)
            {
                return NotFound();
            }

            if (!(User.IsInRole("Admin") || User.IsInRole("Supervisor") || User.IsInRole("Root")))
            {
                TempData["AlertMessage"] = "Only Admin/Supervisor/Root can record consumption.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (quantityUsed <= 0)
            {
                TempData["AlertMessage"] = "Quantity used must be greater than 0.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var stock = await _context.MaterialStocks
                .FirstOrDefaultAsync(s => s.MaterialID == materialID && s.StockLocationID == stockLocationID);

            if (stock == null)
            {
                TempData["AlertMessage"] = "No stock record exists for that material/location.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (stock.QuantityOnHand < quantityUsed)
            {
                TempData["AlertMessage"] = "Insufficient inventory for this consumption.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var before = stock.QuantityOnHand;
            stock.QuantityOnHand -= quantityUsed;

            _context.WorkOrderMaterialConsumptions.Add(new WorkOrderMaterialConsumption
            {
                WorkOrderID = id,
                MaterialID = materialID,
                StockLocationID = stockLocationID,
                ConsumedOn = consumedOn.Date,
                QuantityUsed = quantityUsed,
                UnitCostAtUse = stock.LastUnitCost,
                Notes = notes
            });

            _context.InventoryMovements.Add(new InventoryMovement
            {
                MovementDate = consumedOn.Date,
                MovementType = InventoryMovementType.Consumption,
                MaterialID = materialID,
                StockLocationID = stockLocationID,
                QuantityDelta = -quantityUsed,
                QuantityBefore = before,
                QuantityAfter = stock.QuantityOnHand,
                UnitCost = stock.LastUnitCost,
                ReferenceCode = $"WO-{id}",
                Notes = notes
            });

            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Consumption recorded and inventory updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCrewMember(int id, int employeeID, EmployeeSkillType assignedSkill,
            double estimatedHours, double actualHours)
        {
            var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == id);
            if (workOrder == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrder.CreatedBy))
            {
                return Forbid();
            }

            if (estimatedHours < 0 || estimatedHours > 24 || actualHours < 0 || actualHours > 24)
            {
                TempData["AlertMessage"] = "Hours must be between 0 and 24.";
                return RedirectToAction(nameof(Details), new { id });
            }

            bool hasSkill = await _context.EmployeeSkills
                .AnyAsync(s => s.EmployeeID == employeeID && s.Skill == assignedSkill);

            if (!hasSkill)
            {
                TempData["AlertMessage"] = "Selected employee does not have the required skill.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var assignment = await _context.WorkOrderCrewAssignments
                .FirstOrDefaultAsync(a => a.WorkOrderID == id && a.EmployeeID == employeeID);

            if (assignment == null)
            {
                assignment = new WorkOrderCrewAssignment
                {
                    WorkOrderID = id,
                    EmployeeID = employeeID,
                    AssignedSkill = assignedSkill,
                    EstimatedHours = estimatedHours,
                    ActualHours = actualHours
                };
                _context.WorkOrderCrewAssignments.Add(assignment);
            }
            else
            {
                assignment.AssignedSkill = assignedSkill;
                assignment.EstimatedHours = estimatedHours;
                assignment.ActualHours = actualHours;
            }

            await _context.SaveChangesAsync();
            await UpdateAssignedCrewSummaryAsync(id);

            TempData["AlertMessage"] = "Crew assignment saved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCrewMember(int id, int employeeID)
        {
            var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == id);
            if (workOrder == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrder.CreatedBy))
            {
                return Forbid();
            }

            var assignment = await _context.WorkOrderCrewAssignments
                .FirstOrDefaultAsync(a => a.WorkOrderID == id && a.EmployeeID == employeeID);

            if (assignment != null)
            {
                _context.WorkOrderCrewAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
                await UpdateAssignedCrewSummaryAsync(id);
            }

            TempData["AlertMessage"] = "Crew assignment removed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> RoutePlan(DateTime? date)
        {
            var selectedDate = date?.Date ?? DateTime.Today;

            var query = _context.WorkOrders
                .Include(w => w.Project)
                .ThenInclude(p => p.City)
                .ThenInclude(c => c.Province)
                .Include(w => w.Project)
                .ThenInclude(p => p.Client)
                .Where(w => w.ScheduledDate.Date == selectedDate
                    && w.Status != WorkOrderStatus.Cancelled)
                .AsNoTracking();

            if (IsGuestMode())
            {
                string owner = CurrentOwnerName();
                query = query.Where(w => w.CreatedBy == owner);
            }

            var workOrders = await query.ToListAsync();
            var model = BuildRoutePlan(selectedDate, workOrders);
            return View(model);
        }

        public IActionResult Create()
        {
            PopulateProjectSelectList();
            PopulateStatusSelectList();
            return View(new WorkOrder());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,ScheduledDate,Status,AssignedCrew,CompletedOn,Notes,ProjectID")] WorkOrder workOrder)
        {
            if (IsGuestMode())
            {
                bool ownsProject = await _context.Projects
                    .AnyAsync(p => p.ID == workOrder.ProjectID && p.CreatedBy == CurrentOwnerName());
                if (!ownsProject)
                {
                    ModelState.AddModelError("ProjectID", "Guest Mode can only create work orders for your own projects.");
                }
            }

            bool duplicateExists = await _context.WorkOrders
                .AsNoTracking()
                .AnyAsync(w => w.ProjectID == workOrder.ProjectID
                    && w.ScheduledDate.Date == workOrder.ScheduledDate.Date
                    && w.Status != WorkOrderStatus.Cancelled);

            if (duplicateExists)
            {
                ModelState.AddModelError("ScheduledDate", "A work order for this project already exists on that date.");
            }

            NormalizeCompletionDate(workOrder);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(workOrder);
                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Work Order created successfully.";
                    return RedirectToAction(nameof(Details), new { id = workOrder.ID });
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("ScheduledDate", "A work order for this project already exists on that date.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }

            PopulateProjectSelectList(workOrder.ProjectID);
            PopulateStatusSelectList(workOrder.Status);
            return View(workOrder);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == id);
            if (workOrder == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrder.CreatedBy))
            {
                return Forbid();
            }

            PopulateProjectSelectList(workOrder.ProjectID);
            PopulateStatusSelectList(workOrder.Status);
            return View(workOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, byte[] rowVersion)
        {
            var workOrderToUpdate = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == id);
            if (workOrderToUpdate == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrderToUpdate.CreatedBy))
            {
                return Forbid();
            }

            _context.Entry(workOrderToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync(workOrderToUpdate, "",
                w => w.Title, w => w.ScheduledDate, w => w.Status,
                w => w.AssignedCrew, w => w.CompletedOn, w => w.Notes, w => w.ProjectID))
            {
                if (IsGuestMode())
                {
                    bool ownsProject = await _context.Projects
                        .AnyAsync(p => p.ID == workOrderToUpdate.ProjectID && p.CreatedBy == CurrentOwnerName());
                    if (!ownsProject)
                    {
                        ModelState.AddModelError("ProjectID", "Guest Mode can only assign your own projects.");
                        PopulateProjectSelectList(workOrderToUpdate.ProjectID);
                        PopulateStatusSelectList(workOrderToUpdate.Status);
                        return View(workOrderToUpdate);
                    }
                }

                NormalizeCompletionDate(workOrderToUpdate);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Work Order updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = workOrderToUpdate.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkOrderExists(workOrderToUpdate.ID))
                    {
                        return NotFound();
                    }

                    ModelState.AddModelError(string.Empty,
                        "The record you attempted to edit was modified by another user. Please refresh and try again.");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            PopulateProjectSelectList(workOrderToUpdate.ProjectID);
            PopulateStatusSelectList(workOrderToUpdate.Status);
            return View(workOrderToUpdate);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrder = await _context.WorkOrders
                .Include(w => w.Project)
                .ThenInclude(p => p.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.ID == id);

            if (workOrder == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrder.CreatedBy))
            {
                return Forbid();
            }

            return View(workOrder);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == id);
            if (workOrder == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(workOrder.CreatedBy))
            {
                return Forbid();
            }

            try
            {
                _context.WorkOrders.Remove(workOrder);
                await _context.SaveChangesAsync();
                TempData["AlertMessageDelete"] = "Work Order deleted successfully.";
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to delete record. Try again, and if the problem persists see your system administrator.");
                return View(workOrder);
            }
        }

        private void PopulateProjectSelectList(int? selectedId = null)
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .AsNoTracking()
                .AsQueryable();

            if (IsGuestMode())
            {
                string owner = CurrentOwnerName();
                query = query.Where(p => p.CreatedBy == owner);
            }

            var items = query
                .OrderBy(p => p.ProjectName)
                .Select(p => new
                {
                    p.ID,
                    Display = p.ProjectName + " - " + p.Client.CompanyName
                })
                .ToList();

            ViewData["ProjectID"] = new SelectList(items, "ID", "Display", selectedId);
        }

        private void PopulateStatusSelectList(WorkOrderStatus? selectedStatus = null)
        {
            var statuses = Enum.GetValues(typeof(WorkOrderStatus)).Cast<WorkOrderStatus>();
            ViewData["Status"] = new SelectList(statuses, selectedStatus);
        }

        private static void NormalizeCompletionDate(WorkOrder workOrder)
        {
            if (workOrder.Status == WorkOrderStatus.Completed)
            {
                workOrder.CompletedOn ??= DateTime.Today;
            }
            else
            {
                workOrder.CompletedOn = null;
            }
        }

        private bool WorkOrderExists(int id)
        {
            return _context.WorkOrders.Any(e => e.ID == id);
        }

        private async Task UpdateAssignedCrewSummaryAsync(int workOrderId)
        {
            var workOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ID == workOrderId);
            if (workOrder == null)
            {
                return;
            }

            var names = await _context.WorkOrderCrewAssignments
                .Where(a => a.WorkOrderID == workOrderId)
                .Include(a => a.Employee)
                .Select(a => a.Employee.FirstName + " " + a.Employee.LastName)
                .Distinct()
                .ToListAsync();

            workOrder.AssignedCrew = names.Any() ? string.Join(", ", names) : string.Empty;
            await _context.SaveChangesAsync();
        }

        private static RoutePlanViewModel BuildRoutePlan(DateTime date, List<WorkOrder> workOrders)
        {
            var cityCoordinates = new Dictionary<string, (double Lat, double Lon)>(StringComparer.OrdinalIgnoreCase)
            {
                ["Toronto"] = (43.6532, -79.3832),
                ["Halifax"] = (44.6488, -63.5752),
                ["Calgary"] = (51.0447, -114.0719),
                ["Niagara Falls"] = (43.0896, -79.0849),
                ["St. Catharines"] = (43.1594, -79.2469)
            };

            var remaining = new List<WorkOrder>(workOrders);
            var route = new List<RouteStopViewModel>();
            (double Lat, double Lon) current = (43.0896, -79.0849);
            double cumulative = 0;
            int seq = 1;

            while (remaining.Any())
            {
                WorkOrder next = remaining
                    .OrderBy(w => Distance(current, GetCoordinate(w.Project?.City?.Name, cityCoordinates)))
                    .First();

                var nextCoord = GetCoordinate(next.Project?.City?.Name, cityCoordinates);
                double leg = Distance(current, nextCoord);
                cumulative += leg;

                route.Add(new RouteStopViewModel
                {
                    Sequence = seq++,
                    WorkOrderID = next.ID,
                    WorkOrderTitle = next.Title,
                    ProjectName = next.Project?.ProjectName ?? "-",
                    ClientName = next.Project?.Client?.CompanyName ?? "-",
                    CityName = next.Project?.City?.Name ?? "Unknown",
                    ProjectSite = next.Project?.ProjectSite ?? "-",
                    DistanceFromPreviousKm = Math.Round(leg, 2),
                    CumulativeDistanceKm = Math.Round(cumulative, 2),
                    ScheduledDate = next.ScheduledDate
                });

                current = nextCoord;
                remaining.Remove(next);
            }

            return new RoutePlanViewModel
            {
                Date = date,
                Stops = route
            };
        }

        private static (double Lat, double Lon) GetCoordinate(string cityName,
            IReadOnlyDictionary<string, (double Lat, double Lon)> known)
        {
            if (!string.IsNullOrWhiteSpace(cityName) && known.TryGetValue(cityName, out var coord))
            {
                return coord;
            }

            return (43.0896, -79.0849);
        }

        private static double Distance((double Lat, double Lon) a, (double Lat, double Lon) b)
        {
            const double earthKm = 6371;
            double dLat = DegreesToRadians(b.Lat - a.Lat);
            double dLon = DegreesToRadians(b.Lon - a.Lon);
            double lat1 = DegreesToRadians(a.Lat);
            double lat2 = DegreesToRadians(b.Lat);

            double sinDLat = Math.Sin(dLat / 2);
            double sinDLon = Math.Sin(dLon / 2);
            double h = sinDLat * sinDLat + Math.Cos(lat1) * Math.Cos(lat2) * sinDLon * sinDLon;
            double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
            return earthKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
