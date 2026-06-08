using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly NBDContext _context;

        public DashboardController(NBDContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool? myScope)
        {
            bool isAdmin = User.IsInRole("Admin");
            bool isSupervisor = User.IsInRole("Supervisor");
            bool isDesigner = User.IsInRole("Designer");
            bool isSales = User.IsInRole("Sales");
            bool isGuestMode = User.HasClaim("GuestMode", "true") || User.IsInRole("Guest");
            string owner = User.Identity?.Name ?? string.Empty;

            bool privileged = isAdmin || isSupervisor;
            bool defaultMyScope = !privileged;
            bool useMyScope = isGuestMode || (myScope ?? defaultMyScope);

            var clients = _context.Clients.AsNoTracking().AsQueryable();
            var projects = _context.Projects.AsNoTracking().AsQueryable();
            var bids = _context.Bids.AsNoTracking().AsQueryable();
            var workOrders = _context.WorkOrders
                .AsNoTracking()
                .Include(w => w.Project)
                .ThenInclude(p => p.Client)
                .Include(w => w.CrewAssignments)
                .AsQueryable();

            if (useMyScope)
            {
                clients = clients.Where(c => c.CreatedBy == owner);
                projects = projects.Where(p => p.CreatedBy == owner);
                bids = bids.Where(b => b.CreatedBy == owner);
                workOrders = workOrders.Where(w => w.CreatedBy == owner);
            }

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var model = new DashboardVM
            {
                IsAdmin = isAdmin,
                IsSupervisor = isSupervisor,
                IsDesigner = isDesigner,
                IsSales = isSales,
                CanToggleScope = !isGuestMode,
                UseMyScope = useMyScope,

                TotalClients = await _context.Clients.CountAsync(),
                TotalProjects = await _context.Projects.CountAsync(),
                TotalBids = await _context.Bids.CountAsync(),
                TotalEmployees = await _context.Employees.CountAsync(),
                ActiveEmployees = await _context.Employees.CountAsync(e => e.Active),
                InactiveEmployees = await _context.Employees.CountAsync(e => !e.Active),

                ScopedClients = await clients.CountAsync(),
                ScopedProjects = await projects.CountAsync(),
                MyCreatedClients = await _context.Clients.CountAsync(c => c.CreatedBy == owner),
                MyCreatedProjects = await _context.Projects.CountAsync(p => p.CreatedBy == owner),
                MyCreatedBids = await _context.Bids.CountAsync(b => b.CreatedBy == owner),

                ProjectsWithoutBids = await projects.CountAsync(p => !p.Bids.Any()),
                BidsThisMonth = await bids.CountAsync(b => b.BidDate >= monthStart),

                TotalWorkOrders = await workOrders.CountAsync(),
                PendingWorkOrders = await workOrders.CountAsync(w => w.Status == WorkOrderStatus.Pending || w.Status == WorkOrderStatus.Scheduled),
                InProgressWorkOrders = await workOrders.CountAsync(w => w.Status == WorkOrderStatus.InProgress),
                CompletedWorkOrders = await workOrders.CountAsync(w => w.Status == WorkOrderStatus.Completed),

                TodayRouteStops = await workOrders.CountAsync(w => w.ScheduledDate.Date == today && w.Status != WorkOrderStatus.Cancelled),
                TodayEstimatedHours = await workOrders.Where(w => w.ScheduledDate.Date == today).SelectMany(w => w.CrewAssignments).SumAsync(a => (double?)a.EstimatedHours) ?? 0,
                TodayActualHours = await workOrders.Where(w => w.ScheduledDate.Date == today).SelectMany(w => w.CrewAssignments).SumAsync(a => (double?)a.ActualHours) ?? 0,

                EmployeesWithSkills = await _context.Employees.CountAsync(e => e.EmployeeSkills.Any() && (!useMyScope || e.CreatedBy == owner)),
                TotalSkillAssignments = await _context.WorkOrderCrewAssignments.CountAsync(a => !useMyScope || a.WorkOrder.CreatedBy == owner)
            };

            model.TodayHoursVariance = model.TodayActualHours - model.TodayEstimatedHours;

            model.RecentClients = await clients
                .OrderByDescending(c => c.CreatedOn)
                .Take(5)
                .Select(c => new ActivityItemVM
                {
                    Id = c.ID,
                    Title = c.CompanyName,
                    SubTitle = c.FullName,
                    CreatedOn = c.CreatedOn,
                    CreatedBy = c.CreatedBy
                })
                .ToListAsync();

            model.RecentProjects = await projects
                .Include(p => p.Client)
                .OrderByDescending(p => p.CreatedOn)
                .Take(5)
                .Select(p => new ActivityItemVM
                {
                    Id = p.ID,
                    Title = p.ProjectName,
                    SubTitle = p.Client.CompanyName,
                    CreatedOn = p.CreatedOn,
                    CreatedBy = p.CreatedBy
                })
                .ToListAsync();

            model.RecentBids = await bids
                .Include(b => b.Project)
                .OrderByDescending(b => b.BidDate)
                .Take(5)
                .Select(b => new ActivityItemVM
                {
                    Id = b.ID,
                    Title = b.Project.ProjectName,
                    SubTitle = "Bid Date: " + b.BidDate.ToString("yyyy-MMM-dd"),
                    CreatedOn = b.CreatedOn,
                    CreatedBy = b.CreatedBy
                })
                .ToListAsync();

            model.RecentWorkOrders = await workOrders
                .OrderByDescending(w => w.ScheduledDate)
                .ThenByDescending(w => w.CreatedOn)
                .Take(6)
                .Select(w => new ActivityItemVM
                {
                    Id = w.ID,
                    Title = w.Title,
                    SubTitle = w.Project.ProjectName + " - " + w.Status,
                    CreatedOn = w.CreatedOn,
                    CreatedBy = w.CreatedBy
                })
                .ToListAsync();

            model.SkillUsage = await _context.WorkOrderCrewAssignments
                .AsNoTracking()
                .Where(a => !useMyScope || a.WorkOrder.CreatedBy == owner)
                .GroupBy(a => a.AssignedSkill)
                .Select(g => new SkillUsageItemVM
                {
                    Skill = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var statusCounts = await workOrders
                .GroupBy(w => w.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            model.WorkOrderStatusChart = Enum.GetValues(typeof(WorkOrderStatus))
                .Cast<WorkOrderStatus>()
                .Select(status => new StatusChartItemVM
                {
                    Label = status.ToString(),
                    Count = statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0
                })
                .ToList();

            var weekStart = today.AddDays(-6);
            var weeklyHours = await _context.WorkOrderCrewAssignments
                .AsNoTracking()
                .Where(a => a.WorkOrder.ScheduledDate.Date >= weekStart && a.WorkOrder.ScheduledDate.Date <= today)
                .Where(a => !useMyScope || a.WorkOrder.CreatedBy == owner)
                .GroupBy(a => a.WorkOrder.ScheduledDate.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    Estimated = g.Sum(x => x.EstimatedHours),
                    Actual = g.Sum(x => x.ActualHours)
                })
                .ToListAsync();

            model.WeeklyHoursTrend = Enumerable.Range(0, 7)
                .Select(offset => weekStart.AddDays(offset))
                .Select(day =>
                {
                    var point = weeklyHours.FirstOrDefault(x => x.Day == day);
                    return new HoursTrendPointVM
                    {
                        Label = day.ToString("MMM dd"),
                        EstimatedHours = point?.Estimated ?? 0,
                        ActualHours = point?.Actual ?? 0
                    };
                })
                .ToList();

            var stockQuery = _context.MaterialStocks
                .AsNoTracking()
                .Include(s => s.Material)
                .Include(s => s.StockLocation)
                .Where(s => s.QuantityOnHand <= s.MinQuantity);

            if (useMyScope)
            {
                stockQuery = stockQuery.Where(s => s.CreatedBy == owner);
            }

            model.ReorderAlerts = await stockQuery.CountAsync();
            model.ReorderAlertItems = await stockQuery
                .OrderBy(s => s.QuantityOnHand - s.MinQuantity)
                .Take(5)
                .Select(s => new ReorderAlertVM
                {
                    Material = s.Material.Name,
                    Location = s.StockLocation.Name,
                    OnHand = s.QuantityOnHand,
                    Minimum = s.MinQuantity
                })
                .ToListAsync();

            return View(model);
        }

        public IActionResult Schedule()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ScheduleEvents(DateTime? start, DateTime? end)
        {
            var query = _context.Projects
                .AsNoTracking()
                .Include(p => p.Client)
                .AsQueryable();

            bool isGuestMode = User.HasClaim("GuestMode", "true") || User.IsInRole("Guest");
            if (isGuestMode)
            {
                string owner = User.Identity?.Name ?? string.Empty;
                query = query.Where(p => p.CreatedBy == owner);
            }

            if (start.HasValue)
            {
                query = query.Where(p => p.StartTime >= start.Value.Date);
            }

            if (end.HasValue)
            {
                query = query.Where(p => p.StartTime <= end.Value.Date.AddDays(1));
            }

            var events = await query
                .OrderBy(p => p.StartTime)
                .Select(p => new
                {
                    id = p.ID,
                    title = p.ProjectName + " - " + p.Client.CompanyName,
                    start = p.StartTime,
                    end = p.EndTime.HasValue ? p.EndTime.Value.AddDays(1) : p.StartTime.AddDays(1),
                    allDay = true,
                    color = p.EndTime.HasValue && p.EndTime.Value.Date < DateTime.Today ? "#2f9e44" : "#2b6cb0",
                    url = Url.Action("Details", "Projects", new { id = p.ID })
                })
                .ToListAsync();

            return Json(events);
        }
    }
}