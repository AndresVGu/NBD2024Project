using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Root")]
    public class UserRoleController : CognizantController

    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ModelState.AddModelError("roleName", "Role name is required.");
                return View();
            }

            roleName = roleName.Trim();
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                ModelState.AddModelError("roleName", "Role already exists.");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("roleName", "Unable to create role.");
                return View();
            }

            await WriteAuditAsync(null, null, "RoleCreated", roleName, "Role created by Root module.");

            TempData["AlertMessage"] = "Role created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Audit()
        {
            var logs = await _context.RoleAuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(300)
                .ToListAsync();

            return View(logs);
        }
        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await (from u in _context.Users
                               .OrderBy(u => u.UserName)
                               select new UserVM
                               {
                                   ID = u.Id,
                                   UserName = u.UserName
                               }).ToListAsync();

            foreach (var u in users)
            {
                var _user = await _userManager.FindByIdAsync(u.ID);
                u.UserRoles = (List<string>)await _userManager.GetRolesAsync(_user);
                //Note: we needed the explicit cast above because GetRolesAsync() returns an IList<string>
            };
            return View(users);
        }
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            var _user = await _userManager.FindByIdAsync(id);//IdentityRole
            if (_user == null)
            {
                return NotFound();
            }
            UserVM user = new UserVM
            {
                ID = _user.Id,
                UserName = _user.UserName,
                UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
            };
            PopulateAssignedRoleData(user);
            await PopulateRoleProtectionDataAsync(user);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string Id, string[] selectedRoles)
        {
            var _user = await _userManager.FindByIdAsync(Id);//IdentityRole
            if (_user == null)
            {
                return NotFound();
            }

            UserVM user = new UserVM
            {
                ID = _user.Id,
                UserName = _user.UserName,
                UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
            };

            var currentUser = await _userManager.GetUserAsync(User);
            bool editingSelf = currentUser != null && currentUser.Id == Id;
            bool targetIsRoot = user.UserRoles.Contains("Root");
            bool removingRoot = targetIsRoot && (selectedRoles == null || !selectedRoles.Contains("Root"));
            int rootCount = (await _userManager.GetUsersInRoleAsync("Root")).Count;

            if (removingRoot && editingSelf)
            {
                ModelState.AddModelError(string.Empty, "Root cannot remove the Root role from itself.");
            }

            if (removingRoot && rootCount <= 1)
            {
                ModelState.AddModelError(string.Empty, "You cannot remove the last Root user from the system.");
            }

            if (!ModelState.IsValid)
            {
                PopulateAssignedRoleData(user);
                await PopulateRoleProtectionDataAsync(user);
                return View(user);
            }

            try
            {
                await UpdateUserRoles(selectedRoles, user);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty,
                                "Unable to save changes.");
            }
            PopulateAssignedRoleData(user);
            await PopulateRoleProtectionDataAsync(user);
            return View(user);
        }

        private void PopulateAssignedRoleData(UserVM user)
        {//Prepare checkboxes for all Roles
            var allRoles = _context.Roles;
            var currentRoles = user.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach (var r in allRoles)
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

        private async Task PopulateRoleProtectionDataAsync(UserVM user)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            bool editingSelf = currentUser != null && currentUser.Id == user.ID;
            bool targetIsRoot = user.UserRoles.Contains("Root");
            int rootCount = (await _userManager.GetUsersInRoleAsync("Root")).Count;

            ViewBag.LockRootRole = targetIsRoot && (editingSelf || rootCount <= 1);
            ViewBag.RootLockReason = editingSelf
                ? "Root role cannot be removed from your own account."
                : (rootCount <= 1 ? "At least one Root account must remain in the system." : string.Empty);
        }

        private async Task UpdateUserRoles(string[] selectedRoles, UserVM userToUpdate)
        {
            var UserRoles = userToUpdate.UserRoles;//Current roles use is in
            var _user = await _userManager.FindByIdAsync(userToUpdate.ID);//IdentityUser

            if (selectedRoles == null)
            {
                //No roles selected so just remove any currently assigned
                foreach (var r in UserRoles)
                {
                    await _userManager.RemoveFromRoleAsync(_user, r);
                    await WriteAuditAsync(userToUpdate.ID, userToUpdate.UserName, "RoleRemoved", r, "All roles were cleared from user.");
                }
            }
            else
            {
                //At least one role checked so loop through all the roles
                //and add or remove as required

                //We need to do this next line because foreach loops don't always work well
                //for data returned by EF when working async.  Pulling it into an IList<>
                //first means we can safely loop over the colleciton making async calls and avoid
                //the error 'New transaction is not allowed because there are other threads running in the session'
                IList<IdentityRole> allRoles = _context.Roles.ToList<IdentityRole>();

                foreach (var r in allRoles)
                {
                    if (selectedRoles.Contains(r.Name))
                    {
                        if (!UserRoles.Contains(r.Name))
                        {
                            await _userManager.AddToRoleAsync(_user, r.Name);
                            await WriteAuditAsync(userToUpdate.ID, userToUpdate.UserName, "RoleAssigned", r.Name, "Role assigned in user role editor.");
                        }
                    }
                    else
                    {
                        if (UserRoles.Contains(r.Name))
                        {
                            await _userManager.RemoveFromRoleAsync(_user, r.Name);
                            await WriteAuditAsync(userToUpdate.ID, userToUpdate.UserName, "RoleRemoved", r.Name, "Role removed in user role editor.");
                        }
                    }
                }
            }
        }

        private async Task WriteAuditAsync(string targetUserId, string targetUserName, string actionType, string roleName, string notes)
        {
            var actor = await _userManager.GetUserAsync(User);
            _context.RoleAuditLogs.Add(new RoleAuditLog
            {
                ActorUserId = actor?.Id,
                ActorUserName = actor?.UserName ?? User.Identity?.Name ?? "Unknown",
                TargetUserId = targetUserId,
                TargetUserName = targetUserName,
                ActionType = actionType,
                RoleName = roleName,
                Notes = notes,
                CreatedOnUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
