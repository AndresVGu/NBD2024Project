using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.Utilities;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles ="Admin")]
    public class EmployeeController : CognizantController
    {
        private readonly NBDContext _context;
        private readonly ApplicationDbContext _identityContext;
        private readonly IMyEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;


        public EmployeeController(NBDContext context, 
            ApplicationDbContext identityContext, 
            IMyEmailSender emailSender, 
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _identityContext = identityContext;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        //Index
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Subscriptions)
                .Select(e => new EmployeeAdminVM
                {
                    Email = e.Email,
                    Prescriber = e.Prescriber,
                    Position = e.Position,
                    Active = e.Active,
                    ID = e.ID,
                    FirstName = e.FirstName,
                    MiddleName = e.MiddleName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    NumberOfPushSubscriptions = e.Subscriptions.Count
                }).ToListAsync();

            foreach(var e in employees)
            {
                var user = await _userManager.FindByEmailAsync(e.Email);

                if (user != null)
                {
                    e.UserRoles = (List<string>)await _userManager
                        .GetRolesAsync(user);
                }
            };



            return View(employees);
        }

        //GET: Employee/Create
        public IActionResult Create()
        {
            EmployeeAdminVM employee = new EmployeeAdminVM();
            PopulateAssignedRoleData(employee);
            return View(employee);
        }

        //POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName," +
            "MiddleName, Phone, Position, Prescriber, Email")] Employee employee,
            string[] selectedRoles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(employee);
                    await _context.SaveChangesAsync();

                    InsertIdentityUser(employee.Email, selectedRoles);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException dex)
            {
                if(dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Email", "Unable to save change. Email cannot be duplicate");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes");
                }
            }

            EmployeeAdminVM employeeAdminVM = new EmployeeAdminVM
            {
                Email = employee.Email,
                Prescriber = employee.Prescriber,
                Position = employee.Position,
                Active = employee.Active,
                ID = employee.ID,
                FirstName = employee.FirstName,
                MiddleName = employee.MiddleName,
                LastName = employee.LastName,
                Phone = employee.Phone,
            };
            foreach (var role in selectedRoles)
            {
                employeeAdminVM.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(employeeAdminVM);
            return View(employeeAdminVM);
        }

        //GET Employee/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Where(e => e.ID == id)
                .Select(e => new EmployeeAdminVM
                {
                    Email = e.Email,
                    Prescriber = e.Prescriber,
                    Position = e.Position,
                    Active = e.Active,
                    ID = e.ID,
                    FirstName = e.FirstName,
                    MiddleName = e.MiddleName,
                    LastName = e.LastName,
                    Phone = e.Phone,
                    
                }).FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByEmailAsync(employee.Email);
            if(user != null)
            {
                var r = await _userManager.GetRolesAsync(user);
                employee.UserRoles = (List<string>)r;
            }
            PopulateAssignedRoleData(employee);
            return View(employee);

        }

        //POST Employee/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, bool Active,
            string[] selectedRoles)
        {
            var employeeToUpdate = await _context.Employees
                .FirstOrDefaultAsync(e => e.ID == id);

            if(employeeToUpdate == null)
            {
                return NotFound();
            }

            bool ActiveStatus = employeeToUpdate.Active;
            string databaseEmail = employeeToUpdate.Email;

            if(await TryUpdateModelAsync<Employee>(employeeToUpdate,
                "", e => e.FirstName, e => e.MiddleName, e => e.LastName,
                e => e.Phone, e => e.Email, e=> e.Prescriber, e => e.Position,
                e => e.Active))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    if(employeeToUpdate.Active == false && ActiveStatus == true)
                    {
                        await DeleteIdentityUser(employeeToUpdate.Email);
                    }
                    else if(employeeToUpdate.Active == true && ActiveStatus == false)
                    {
                        InsertIdentityUser(employeeToUpdate.Email, selectedRoles);
                    }
                    else if (employeeToUpdate.Active == true && ActiveStatus == true)
                    {
                       if(employeeToUpdate.Email != databaseEmail)
                        {
                            InsertIdentityUser(employeeToUpdate.Email, selectedRoles);
                            await DeleteIdentityUser(employeeToUpdate.Email);
                        }
                        else
                        {
                           // await UpdateUserRoles(selectedRoles, employeeToUpdate.Email);
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employeeToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else { throw; }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Email", "Unable to save change. Email cannot be duplicate");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes");
                    }
                }

                
            }
            EmployeeAdminVM employeeAdminVM = new EmployeeAdminVM
            {
                Email = employeeToUpdate.Email,
                Prescriber = employeeToUpdate.Prescriber,
                Position = employeeToUpdate.Position,
                Active = employeeToUpdate.Active,
                ID = employeeToUpdate.ID,
                FirstName = employeeToUpdate.FirstName,
                MiddleName = employeeToUpdate.MiddleName,
                LastName = employeeToUpdate.LastName,
                Phone = employeeToUpdate.Phone,
            };
            foreach (var role in selectedRoles)
            {
                employeeAdminVM.UserRoles.Add(role);
            }
            PopulateAssignedRoleData(employeeAdminVM);
            return View(employeeAdminVM);
        }

        //METHODS
        private void PopulateAssignedRoleData(EmployeeAdminVM employee)
        {
            var allRoles = _identityContext.Roles;
            var currentRoles = employee.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach(var r  in allRoles)
            {
                viewModel.Add(new RoleVM
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Assigned = currentRoles.Contains(r.Name)
                });
            }
            ViewBag.Roles = viewModel;
        }

        private void InsertIdentityUser(string email, string[] selectedRoles)
        {
            if(_userManager.FindByEmailAsync(email).Result == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                string password = MakePassword.Generate();
                IdentityResult result = _userManager.CreateAsync(user, password).Result;

                if(result.Succeeded)
                {
                    foreach(string role in selectedRoles)
                    {
                        _userManager.AddToRoleAsync(user, role).Wait();
                    }
                }
            }
            else
            {
                TempData["message"] = "The Login Account for" + email
                    + "was already in the system.";
            }
        }

        private async Task InviteUseToResetPassword(Employee employee, string message)
        {
            message ??= "Hello" + employee.FirstName + "<br /><p>Please navigate to:<br />" +
                "create a new password for" + employee.Email + "using Forgot Password</p>";

            try
            {
                await _emailSender.SendOneAsync(employee.FullName, employee.Email,
                    "Account Registration", message);
                TempData["message"] = "Invitation email sent to" + employee.FullName + " at "
                    + employee.Email;
            }
            catch (Exception )
            {
                TempData["message"] = "Could not send Invitation email to "
                    + employee.FullName + " at " + employee.Email;
            }
        }

        private async Task DeleteIdentityUser(string Email)
        {
            var userToDelete = await _identityContext.Users.Where(u => u.Email == Email)
                .FirstOrDefaultAsync();
            if(userToDelete != null)
            {
                _identityContext.Users.Remove(userToDelete);
                await _identityContext.SaveChangesAsync();
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }

    }
}
