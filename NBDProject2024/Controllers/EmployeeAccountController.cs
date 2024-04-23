using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.Utilities;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize]
    public class EmployeeAccountController : CognizantController
    {
        //Database context
        private readonly NBDContext _context;

        public EmployeeAccountController(NBDContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Details));
        }

        //GET EmployeeAccount/Details/5
        public async Task<IActionResult> Details()
        {
            var employee = await _context.Employees
                .Include(e => e.Subscriptions)
                .Where(e => e.Email == User.Identity.Name)
                .Select(e => new EmployeeVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    MiddleName = e.MiddleName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    NumberOfPushSubscriptions = e.Subscriptions.Count()
                })
                .FirstOrDefaultAsync();

            if(employee == null)
            {
                return NotFound();
            }
            TempData["AlertMessage"] = "Employee Updated Successfully...!";
            return View(employee);
        }


        //GET: EmployeeAcount/Edit/5
        public async Task<IActionResult> Edit()
        {
            var employee = await _context.Employees
                //We dont need a parameter because we are using Identity.Name
                .Where(e => e.Email == User.Identity.Name)
                .Select(e => new EmployeeVM
                {
                    ID = e.ID,
                    FirstName = e.FirstName,
                    MiddleName = e.MiddleName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    NumberOfPushSubscriptions = e.Subscriptions.Count()
                })
                .FirstOrDefaultAsync();

            if(employee == null)
            {
                return NotFound();
            }
            TempData["AlertMessage"] = "Employee Updated Successfully...!";
            return View(employee);
        }

        //POST: EmployeeAccount/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(e => e.ID == id);

            if(await TryUpdateModelAsync<Employee>(employeeToUpdate, "",
                c => c.FirstName, c=> c.MiddleName, c => c.LastName, c => c.Phone))
            {
                try
                {
                    _context.Update(employeeToUpdate);
                    await _context.SaveChangesAsync();
                    UpdateUserNameCookie(employeeToUpdate.FullName);
                    TempData["AlertMessage"] = "Employee Updated Successfully...!";
                    return RedirectToAction(nameof(Details));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if(!EmployeeExists(employeeToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Something went wrong in the database.");
                }
            }
            return View(employeeToUpdate);
        }

        //METHODS:
        private void UpdateUserNameCookie(string userName)
        {
            CookieHelper.CookieSet(HttpContext, "userName", userName, 960);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}
