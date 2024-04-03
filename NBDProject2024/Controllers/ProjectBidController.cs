using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.Utilities;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class ProjectBidController : ElephantController
    {
        private readonly NBDContext _context;

        public ProjectBidController(NBDContext context)
        {
            _context = context;
        }

        //GET: BID
       
        
        public async Task<IActionResult> Index(int? ProjectID, int? page, DateTime BidDate,
            int? pageSizeID, int? BidID, string actionButton,
            string SearchString, string sortDirection = "desc",
            string sortField = "Bid")
        {
            /* if(BidDate ==DateTime.MinValue)
             {
                 BidDate = _context.Bids.Min(b => b.BidDate).Date;
                 ViewData["BidDate"] = BidDate.ToString("yyyy/MM/dd");

             }
             ViewData["Filtering"] = "btn-outline-secondary";
             int numberFilters = 0;*/
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Projects");

            PopulateDropDownLists();

            string[] sortOptions = new[] { "" };

            var pbid = from a in _context.Bids
                       .Include(a => a.BidMaterials).ThenInclude(a => a.Materials)
                       .Include(a => a.BidLabours).ThenInclude(a => a.Labours)
                       .Include(a => a.Project)

                       where a.ProjectID == ProjectID.GetValueOrDefault()
                       select a;

            Project project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.City)
                .Where(p => p.ID == ProjectID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Project = project;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Bid>.CreateAsync(pbid.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        private SelectList LabourSelectList(int? id)
        {
            var query = from d in _context.Labours
                        orderby d.Name
                        select d;
            return new SelectList(query, "ID", "Name", id);
        }


        private SelectList MaterialSelectList(int? id)
        {
            var query = from d in _context.Materials
                        orderby d.Name
                        select d;
            return new SelectList(query, "ID", "Name", id);
        }
        private void PopulateDropDownLists(Bid bid = null)
        {
            ViewData["MaterialID"] = MaterialSelectList(bid?.BidMaterials.FirstOrDefault().MaterialID);
            ViewData["MaterialID"] = LabourSelectList(bid?.BidLabours.FirstOrDefault().LabourID);
        }

        private bool ProjectBidExists(int id)
        {
            return _context.Bids.Any(b => b.ID == id);
        }
    }
}
