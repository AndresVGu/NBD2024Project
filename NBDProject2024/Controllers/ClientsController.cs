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

    public class ClientsController : ElephantController
    {
        private readonly NBDContext _context;

        public ClientsController(NBDContext context)
        {
            _context = context;
        }

        // GET: Clients
        [Authorize(Roles = "Admin,Supervisor,Designer,Sales")]

        public async Task<IActionResult> Index(string SearchString, int? CityID,
           int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Client")
        {

            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;

            //List of sort options
            //make sure this array has matchin values to the colum headings
            string[] sortOptions = new[] { "Client", "City", "Company", "Phone" };
            PopulateDropDownLists();

            var clients = _context.Clients
                .Include(c => c.Projects)
                .Include(c => c.City)
                .AsNoTracking();



            #region Filters


            //filters:
            if (CityID.HasValue)
            {
                clients = clients.Where(c => c.CityID == CityID);

                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                clients = clients.Where(c => c.City.Name.ToUpper().Contains(SearchString.ToUpper())
                                        || c.PostalCode.ToUpper().Contains(SearchString.ToUpper())
                                        || c.City.Province.Name.ToUpper().Contains(SearchString.ToUpper())
                                        || c.AddressCountry.ToUpper().Contains(SearchString.ToUpper())
                                        || c.AddressStreet.ToUpper().Contains(SearchString.ToUpper())
                                        || c.CompanyName.ToUpper().Contains(SearchString.ToUpper())
                                        || c.FirstName.ToUpper().Contains(SearchString.ToUpper())
                                        || c.LastName.ToUpper().Contains(SearchString.ToUpper())
                                        || c.MiddleName.ToUpper().Contains(SearchString.ToUpper())
                                        || c.Phone.Contains(SearchString)
                                        || c.FirstName.ToUpper() + " " + c.LastName.ToUpper() == SearchString.ToUpper()
                );

                numberFilters++;
            }
            //Feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending in if we are filtering
                ViewData["Filtering"] = "btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                @ViewData["ShowFilter"] = " show";
            }


            #endregion

            #region Sorting:
            //Before we sort, see if we have called for a change of filtering or sorting
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
            //sort itself
            if (sortField == "Phone")
            {
                if (sortDirection == "asc")
                {
                    clients = clients
                        .OrderBy(c => c.Phone);
                }
                else
                {
                    clients = clients
                        .OrderByDescending(c => c.Phone);
                }
            }
            else if (sortField == "Client")
            {
                if (sortDirection == "asc")
                {
                    clients = clients
                        .OrderBy(c => c.LastName)
                        .ThenBy(c => c.FirstName);
                }
                else
                {
                    clients = clients
                        .OrderByDescending(c => c.LastName)
                        .ThenByDescending(c => c.FirstName);
                }

            }
            else if (sortField == "Company")
            {
                if (sortDirection == "asc")
                {
                    clients = clients
                        .OrderBy(c => c.CompanyName);
                }
                else
                {
                    clients = clients
                        .OrderByDescending(c => c.CompanyName);
                }
            }
            else if (sortField == "City")
            {
                if (sortDirection == "asc")
                {
                    clients = clients
                        .OrderBy(c => c.City.Name);
                }
                else
                {
                    clients = clients
                        .OrderByDescending(c => c.City.Name);
                }
            }

            //set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            #endregion

            //Handle Paging
            //int pageSize = 10;
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Client>.CreateAsync(clients.AsNoTracking(), page ?? 1, pageSize);
            PopulateFilter();
            return View(pagedData);
        }

        // GET: Clients/Details/5
        [Authorize(Roles = "Admin,Supervisor,Designer , Sales")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Projects)
                .Include(c => c.City)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        [Authorize(Roles = "Admin,Supervisor,Sales")]

        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor,Sales")]

        public async Task<IActionResult> Create([Bind("ID,FirstName, MiddleName,LastName, CompanyName," +
            "Phone,Email,AddressCountry,AddressStreet,CityID,PostalCode,")] Client client, string[] selectedOptions)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    _context.Add(client);
                    await _context.SaveChangesAsync();
                    //We can add any name here
                    TempData["AlertMessage"] = "Client Created Sucessfully...!";
                    return RedirectToAction("Details", new { client.ID });

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
            //ViewData["ContactID"] = new SelectList(_context.Contacts, "ID", "ContactFirstName", client.ContactID);
            PopulateDropDownLists(client);
            return View(client);
        }

        // GET: Clients/Edit/5
        [Authorize(Roles = "Admin,Supervisor,Sales")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.City)
                .FirstOrDefaultAsync(c => c.ID == id);


            if (client == null)
            {
                return NotFound();
            }


            PopulateDropDownLists(client);
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor,Sales")]

        public async Task<IActionResult> Edit(int id, Byte[] RowVersion)
        {
            var clientToUpdate = await _context.Clients
                .Include(c => c.City)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (clientToUpdate == null)
            {
                return NotFound();
            }
            _context.Entry(clientToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<Client>(clientToUpdate, "",
                c => c.FirstName, c => c.MiddleName, c => c.LastName,
                c => c.CompanyName, c => c.Phone,
                c => c.Email,
                c => c.AddressCountry, c => c.AddressStreet,
                c => c.CityID, c => c.PostalCode))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Client Updated Successfully...!";
                    return RedirectToAction("Details", new { clientToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(clientToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to edit record. Try again, and if the problem persists see your administrator.");
                }

            }

            PopulateDropDownLists(clientToUpdate);

            return View(clientToUpdate);
        }

        // GET: Clients/Delete/5
        [Authorize(Roles = "Admin,Supervisor,Sales")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Clients == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.City)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (client == null)
            {
                return NotFound();
            }
            if (User.IsInRole("Sales"))
            {
                if (client.CreatedBy != User.Identity.Name)
                {
                    ModelState.AddModelError("", "As a Sales Advisor," +
                        "You cannot delete this client because" +
                        " you did not enter them into the system. ");
                    ViewData["NoSubmit"] = "disabled=disabled";
                }
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor, Sales")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Clients == null)
            {
                return Problem("No Clients To Delete");
            }
            var client = await _context.Clients
                .Include(c => c.City)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (User.IsInRole("Sales"))
            {
                if (client.CreatedBy != User.Identity.Name)
                {
                    ModelState.AddModelError("", "As a Sales Advisor," +
                        "you cannot delete this client because you did not" +
                        "enter them into the system.");
                    ViewData["NoSubmit"] = "disable=disable";

                    return View(client); // this line prevents the attemp to delete
                }
            }
            try
            {
                if (client != null)
                {
                    _context.Clients.Remove(client);
                }

                await _context.SaveChangesAsync();
                // return RedirectToAction(nameof(Index));
                TempData["AlertMessageDelete"] = "Client Deleted Sucessfully...!";
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Client. you cannot delete a Client with Projects assigned.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your administrator.");
                }
            }

            return View(client);
        }
        #region Client Methods:
        //PartialViews:
        public PartialViewResult ListOfProjectsDetails(int id)
        {
            var query = from p in _context.Projects
                        where p.ClientID == id
                        orderby p.StartTime descending
                        select p;

            return PartialView("_ListOfProjects", query.ToList());
        }

        //Province Select List
        private SelectList ProvinceSelectList(string selectedID)
        {
            return new SelectList(_context.Provinces
                .OrderBy(c => c.Name), "ID", "Name", selectedID);
        }
        //City Select List:
        private SelectList CitySelectList(string ProvinceID, int? selectedID)
        {
            var query = from c in _context.Cities
                        where c.ProvinceID == ProvinceID
                        select c;
            return new SelectList(query.OrderBy(c => c.Name), "ID", "Summary", selectedID);
        }

        private SelectList CitiesSelectList(int? selectedId)
        {
            return new SelectList(_context.Cities
                .OrderBy(c => c.Name)
                , "ID", "Name", selectedId);
        }
        private void PopulateFilter(Client client = null)
        {
            ViewData["CityID"] = CitiesSelectList(client?.CityID);
        }
        //Populate DropDownList: Check
        private void PopulateDropDownLists(Client client = null)
        {


            if ((client?.CityID).HasValue)
            {
                //Careful: CityID might have a value but the city object is missing
                if (client.City == null)
                {
                    client.City = _context.Cities.Find(client.CityID);
                }
                ViewData["ProvinceID"] = ProvinceSelectList(client.City.ProvinceID);
                ViewData["CityID"] = CitySelectList(client.City.ProvinceID, client.CityID);
            }
            else
            {
                ViewData["ProvinceID"] = ProvinceSelectList(null);
                ViewData["CityID"] = CitySelectList(null, null);

            }

        }
        [HttpGet]
        public JsonResult GetCities(string ProvinceID)
        {
            return Json(CitySelectList(ProvinceID, null));
        }

        public JsonResult GetClients(string term)
        {
            var result = from c in _context.Clients
                         where c.LastName.ToUpper().Contains(term.ToUpper())
                         || c.FirstName.ToUpper().Contains(term.ToUpper())
                         orderby c.FirstName, c.LastName
                         select new { value = c.FirstName + " " + c.LastName };

            return Json(result);
        }

        #endregion

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ID == id);
        }
    }
}