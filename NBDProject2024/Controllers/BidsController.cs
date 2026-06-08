using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.Utilities;

namespace NBDProject2024.Controllers
{
    
    public class BidsController : ElephantController
    {
        private readonly NBDContext _context;

        public BidsController(NBDContext context)
        {
            _context = context;
        }

        // GET: Bids
        [Authorize(Roles = "Admin,Supervisor,Designer,Sales,Guest")]
        
        public async Task<IActionResult> Index(int? page, int? pageSizeID, string SearchString, string actionButton, int? MaterialID,
            int? LabourID, string sortDirection = "desc", string sortField = "Bid Date")
        {
            //Supply SelectList for Materials and Labours
            ViewData["MaterialID"] = new SelectList(_context
                .Materials
                .OrderBy(m => m.Name), "ID", "Name", "Price");

            ViewData["LabourID"] = new SelectList(_context
                .Labours
                .OrderBy(m => m.Name), "ID", "Name", "Price");

            //Count the number of filters filters 
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            //List of sort Options
            string[] sortOptions = new[] { "Bid Date","Project",
            "Materials", "Material Total", "Labour","Labour Total",
            "Total"};

            // Populate();

            var bids = _context.Bids
                .Include(b => b.BidMaterials).ThenInclude(b => b.Materials)
                .Include(b => b.BidLabours).ThenInclude(b => b.Labours)
                 .Include(b => b.Project)
                .AsSplitQuery()
                .AsNoTracking();

            if (IsGuestMode())
            {
                var owner = CurrentOwnerName();
                bids = bids.Where(b => b.CreatedBy == owner);
            }

            //Filters:

            if (!String.IsNullOrEmpty(SearchString))
            {
                bids = bids.Where(b => //b.Labour.Name.ToUpper().Contains(SearchString.ToUpper())
                 b.BidDate.ToString().Contains(SearchString)
                //|| b.Material.Name.ToUpper().Contains(SearchString.ToUpper())
                || b.Project.ProjectName.ToUpper().Contains(SearchString.ToUpper())
                || b.Project.City.Name.ToUpper().Contains(SearchString.ToUpper())
                || b.Project.City.Province.Name.ToUpper().Contains(SearchString.ToUpper())
                );
                numberFilters++;

            }

            if (numberFilters != 0)
            {
                ViewData["Filtering"] = "btn-danger";
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                ViewData["ShowFilter"] = " show";
            }


            #region Sorting:
            if (!String.IsNullOrEmpty(actionButton))//form submitted
            {
                page = 1; //reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) // Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton; // sort by the button clicked
                }
            }

            if (sortField == "Bid Date")
            {
                if (sortDirection == "asc")
                {
                    bids = bids
                        .OrderBy(c => c.BidDate);
                }
                else
                {
                    bids = bids
                        .OrderByDescending(c => c.BidDate);
                }
            }
            else if (sortField == "Project")
            {
                if (sortDirection == "asc")
                {
                    bids = bids
                        .OrderBy(c => c.Project.ProjectName);

                }
                else
                {
                    bids = bids
                        .OrderByDescending(c => c.Project.ProjectName);

                }

            }


            //set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            #endregion


            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["PageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Bid>.CreateAsync(bids, page ?? 1, pageSize);
            return View(pagedData);
        }

        // GET: Bids/Details/5
        [Authorize(Roles = "Admin,Supervisor,Designer,Sales,Guest")]
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bids == null)
            {
                return NotFound();
            }

            var bid = await _context.Bids

                .Include(b => b.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bid == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(bid.CreatedBy))
            {
                return Forbid();
            }

            return View(bid);
        }

        // GET: Bids/Create
        [Authorize(Roles = "Admin,Supervisor,Designer,Guest")]
       
        public IActionResult Create()
        {
            var projectQuery = _context.Projects.AsQueryable();
            if (IsGuestMode())
            {
                string owner = CurrentOwnerName();
                projectQuery = projectQuery.Where(p => p.CreatedBy == owner);
            }

            ViewData["ProjectID"] = new SelectList(projectQuery, "ID", "ProjectName");
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");
            return View();
        }

        // POST: Bids/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor,Designer,Guest")]
       
        public async Task<IActionResult> Create([Bind("ID,BidDate,ProjectID")] Bid bid,
            int[] materialIds, int[] materialQuantities, int[] labourIds, double[] labourHours)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (IsGuestMode())
                    {
                        bool ownsProject = await _context.Projects
                            .AnyAsync(p => p.ID == bid.ProjectID && p.CreatedBy == CurrentOwnerName());
                        if (!ownsProject)
                        {
                            ModelState.AddModelError("ProjectID", "Guest Mode can only assign your own projects.");
                            var guestProjects = _context.Projects.Where(p => p.CreatedBy == CurrentOwnerName());
                            ViewData["ProjectID"] = new SelectList(guestProjects, "ID", "ProjectName", bid.ProjectID);
                            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
                            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");
                            return View(bid);
                        }
                    }

                    _context.Add(bid);
                    await _context.SaveChangesAsync();

                    // Consolidate repeated selections into a single line per material.
                    var materialMap = new Dictionary<int, int>();
                    int materialLength = Math.Min(materialIds?.Length ?? 0, materialQuantities?.Length ?? 0);
                    for (int i = 0; i < materialLength; i++)
                    {
                        int materialId = materialIds[i];
                        int quantity = materialQuantities[i];

                        if (materialId > 0 && quantity > 0)
                        {
                            if (!materialMap.ContainsKey(materialId))
                            {
                                materialMap[materialId] = 0;
                            }
                            materialMap[materialId] += quantity;
                        }
                    }

                    foreach (var item in materialMap)
                    {
                        bid.BidMaterials.Add(new BidMaterial
                        {
                            BidID = bid.ID,
                            MaterialID = item.Key,
                            MaterialQuantity = item.Value
                        });
                    }

                    // Consolidate repeated selections into a single line per labour type.
                    var labourMap = new Dictionary<int, double>();
                    int labourLength = Math.Min(labourIds?.Length ?? 0, labourHours?.Length ?? 0);
                    for (int i = 0; i < labourLength; i++)
                    {
                        int labourId = labourIds[i];
                        double hours = labourHours[i];

                        if (labourId > 0 && hours > 0)
                        {
                            if (!labourMap.ContainsKey(labourId))
                            {
                                labourMap[labourId] = 0;
                            }
                            labourMap[labourId] += hours;
                        }
                    }

                    foreach (var item in labourMap)
                    {
                        bid.BidLabours.Add(new BidLabour
                        {
                            BidID = bid.ID,
                            LabourID = item.Key,
                            HoursQuantity = item.Value
                        });
                    }

                    if (materialMap.Count > 0 || labourMap.Count > 0)
                    {
                        await _context.SaveChangesAsync();
                    }

                    TempData["AlertMessage"] = "Bid Created Successfully...!";
                    return RedirectToAction("Details", new { bid.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            var projects = _context.Projects.AsQueryable();
            if (IsGuestMode())
            {
                projects = projects.Where(p => p.CreatedBy == CurrentOwnerName());
            }

            ViewData["ProjectID"] = new SelectList(projects, "ID", "ProjectName", bid.ProjectID);
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");
            return View(bid);
        }

        // GET: Bids/Edit/5
        [Authorize(Roles = "Admin,Supervisor,Designer,Guest")]
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bids == null)
            {
                return NotFound();
            }

            var bid = await _context.Bids
                .Include(b => b.BidMaterials).ThenInclude(b => b.Materials)
                .Include(b => b.BidLabours).ThenInclude(b => b.Labours)
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.ID == id);


            if (bid == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(bid.CreatedBy))
            {
                return Forbid();
            }

            var projectQuery = _context.Projects.AsQueryable();
            if (IsGuestMode())
            {
                projectQuery = projectQuery.Where(p => p.CreatedBy == CurrentOwnerName());
            }

            ViewData["ProjectID"] = new SelectList(projectQuery, "ID", "ProjectName", bid.ProjectID);
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");
            return View(bid);
        }

        // POST: Bids/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
         [Authorize(Roles = "Admin,Supervisor,Designer,Guest")]
       
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion,
            int[] materialIds, int[] materialQuantities, int[] labourIds, double[] labourHours)
        {
            var bidToUpdate = await _context.Bids
                .Include(b => b.BidMaterials).ThenInclude(b => b.Materials)
                .Include(b => b.BidLabours).ThenInclude(b => b.Labours)
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (bidToUpdate == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(bidToUpdate.CreatedBy))
            {
                return Forbid();
            }
            _context.Entry(bidToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<Bid>(bidToUpdate, "",
                b => b.BidDate, b => b.ProjectID))
            {
                if (IsGuestMode())
                {
                    bool ownsProject = await _context.Projects
                        .AnyAsync(p => p.ID == bidToUpdate.ProjectID && p.CreatedBy == CurrentOwnerName());
                    if (!ownsProject)
                    {
                        ModelState.AddModelError("ProjectID", "Guest Mode can only assign your own projects.");
                        var guestProjects = _context.Projects.Where(p => p.CreatedBy == CurrentOwnerName());
                        ViewData["ProjectID"] = new SelectList(guestProjects, "ID", "ProjectName", bidToUpdate.ProjectID);
                        ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
                        ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");
                        return View(bidToUpdate);
                    }
                }

                try
                {
                    _context.BidMaterials.RemoveRange(bidToUpdate.BidMaterials);
                    _context.BidLabours.RemoveRange(bidToUpdate.BidLabours);

                    var materialMap = new Dictionary<int, int>();
                    int materialLength = Math.Min(materialIds?.Length ?? 0, materialQuantities?.Length ?? 0);
                    for (int i = 0; i < materialLength; i++)
                    {
                        int materialId = materialIds[i];
                        int quantity = materialQuantities[i];

                        if (materialId > 0 && quantity > 0)
                        {
                            if (!materialMap.ContainsKey(materialId))
                            {
                                materialMap[materialId] = 0;
                            }
                            materialMap[materialId] += quantity;
                        }
                    }

                    foreach (var item in materialMap)
                    {
                        bidToUpdate.BidMaterials.Add(new BidMaterial
                        {
                            BidID = bidToUpdate.ID,
                            MaterialID = item.Key,
                            MaterialQuantity = item.Value
                        });
                    }

                    var labourMap = new Dictionary<int, double>();
                    int labourLength = Math.Min(labourIds?.Length ?? 0, labourHours?.Length ?? 0);
                    for (int i = 0; i < labourLength; i++)
                    {
                        int labourId = labourIds[i];
                        double hours = labourHours[i];

                        if (labourId > 0 && hours > 0)
                        {
                            if (!labourMap.ContainsKey(labourId))
                            {
                                labourMap[labourId] = 0;
                            }
                            labourMap[labourId] += hours;
                        }
                    }

                    foreach (var item in labourMap)
                    {
                        bidToUpdate.BidLabours.Add(new BidLabour
                        {
                            BidID = bidToUpdate.ID,
                            LabourID = item.Key,
                            HoursQuantity = item.Value
                        });
                    }

                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Bid Updated Successfully...!";
                    return RedirectToAction("Details", new { bidToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BidExists(bidToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit"
                           + "was modified by another user. PLease go back and refresh.");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }

            var projects = _context.Projects.AsQueryable();
            if (IsGuestMode())
            {
                projects = projects.Where(p => p.CreatedBy == CurrentOwnerName());
            }

            ViewData["ProjectID"] = new SelectList(projects, "ID", "ProjectName", bidToUpdate.ProjectID);
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");

            return View(bidToUpdate);
        }

        // GET: Bids/Delete/5
         [Authorize(Roles = "Admin,Supervisor,Designer,Guest")]
      
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bids == null)
            {
                return NotFound();
            }

            var bid = await _context.Bids
              .Include(b => b.BidMaterials).ThenInclude(b => b.Materials)
              .Include(b => b.BidLabours).ThenInclude(b => b.Labours)
                .Include(b => b.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bid == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(bid.CreatedBy))
            {
                return Forbid();
            }

            if (User.IsInRole("Designer"))
            {
                if (bid.CreatedBy != User.Identity.Name)
                {
                    ModelState.AddModelError("", "As a Designer," +
                        "You cannot delete this client because" +
                        " you did not enter them into the system. ");
                    ViewData["NoSubmit"] = "disabled=disabled";
                }
            }

            return View(bid);
        }

        // POST: Bids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Supervisor,Designer,Guest")]
       
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bids == null)
            {
                return Problem("No bids to delete");
            }
            var bid = await _context.Bids
                .Include(b => b.BidMaterials).ThenInclude(b => b.Materials)
              .Include(b => b.BidLabours).ThenInclude(b => b.Labours)
                .Include(b => b.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (bid == null)
            {
                return NotFound();
            }

            if (IsGuestMode() && !IsOwnedByCurrentUser(bid.CreatedBy))
            {
                return Forbid();
            }

            if (User.IsInRole("Designer"))
            {
                if (bid.CreatedBy != User.Identity.Name)
                {
                    ModelState.AddModelError("", "As a Designer," +
                        "you cannot delete this client because you did not" +
                        "enter them into the system.");
                    ViewData["NoSubmit"] = "disable=disable";

                    return View(bid); // this line prevents the attemp to delete
                }
            }
            try
            {
                if (bid != null)
                {
                    _context.Bids.Remove(bid);
                }
                await _context.SaveChangesAsync();
                TempData["AlertMessageDelete"] = "Bid Deleted Sucessfully...!";
                return Redirect(ViewData["returnURL"].ToString());

            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to delete Bid. Remove related records and try again.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(bid);
        }

        public PartialViewResult ListOfMaterialsDetails(int id)
        {
            if (IsGuestMode())
            {
                bool ownsBid = _context.Bids.Any(b => b.ID == id && b.CreatedBy == CurrentOwnerName());
                if (!ownsBid)
                {
                    return PartialView("_ListOfMaterials", new List<BidMaterial>());
                }
            }

            var query = from m in _context.BidMaterials
                        .Include(m => m.Materials)
                        where m.BidID == id
                        orderby m.Materials.Name
                        select m;

            return PartialView("_ListOfMaterials", query.ToList());
        }

        public PartialViewResult ListOfLaboursDetails(int id)
        {
            if (IsGuestMode())
            {
                bool ownsBid = _context.Bids.Any(b => b.ID == id && b.CreatedBy == CurrentOwnerName());
                if (!ownsBid)
                {
                    return PartialView("_ListOfLabours", new List<BidLabour>());
                }
            }

            var query = from b in _context.BidLabours
                       .Include(b => b.Labours)
                        where b.BidID == id
                        orderby b.Labours.Name
                        select b;

            return PartialView("_ListOfLabours", query.ToList());
        }
        private bool BidExists(int id)
        {
            return _context.Bids.Any(e => e.ID == id);
        }
    }
}


