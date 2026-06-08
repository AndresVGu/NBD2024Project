// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NBDProject2024.Data;
using NBDProject2024.Utilities;

namespace NBDProject2024.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly NBDContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private const string RootExceptionEmail = "root@test.com";

        public LoginModel(SignInManager<IdentityUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<IdentityUser> userManager,
            NBDContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/Dashboard");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //Clear username Cookie
            HttpContext.Response.Cookies.Delete("userName");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Dashboard");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var normalizedInputEmail = Input.Email?.Trim();
                var isRootExceptionEmail = string.Equals(normalizedInputEmail, RootExceptionEmail, StringComparison.OrdinalIgnoreCase);
                var identityUser = await _userManager.FindByEmailAsync(normalizedInputEmail);
                var signInUserName = identityUser?.UserName ?? normalizedInputEmail;

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(signInUserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Root is manually managed and is allowed without an Employee row.
                    if (isRootExceptionEmail)
                    {
                        var isRootRole = identityUser != null && await _userManager.IsInRoleAsync(identityUser, "Root");
                        if (!isRootRole)
                        {
                            await _signInManager.SignOutAsync();
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return Page();
                        }
                    }

                    //Code
                    var emp = _context.Employees.Where(e => e.Email == normalizedInputEmail).FirstOrDefault();

                    // Keep non-root accounts constrained to admin-created employee records.
                    if (!isRootExceptionEmail)
                    {
                        if (emp == null)
                        {
                            await _signInManager.SignOutAsync();
                            string msg = "Error: Account for " + Input.Email + " has not been created by the Admin.";
                            ModelState.AddModelError(string.Empty, msg);
                            return Page();
                        }

                        if (!emp.Active)
                        {
                            await _signInManager.SignOutAsync();
                            string msg = "Error: Account for login " + Input.Email + " is not active.";
                            ModelState.AddModelError(string.Empty, msg);
                            return Page();
                        }
                    }

                    var displayName = emp?.FullName ?? normalizedInputEmail;
                    CookieHelper.CookieSet(HttpContext, "userName", displayName, 3200);

                    if (emp != null && String.IsNullOrEmpty(emp.Phone))
                    {
                        //Nag to complete the profile
                        TempData["message"] = "Please enter the phone number.";
                        returnUrl = "~/EmployeeAccount/Edit";
                    }

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    var emp = _context.Employees.Where(e => e.Email == normalizedInputEmail).FirstOrDefault();
                    if (emp == null && !isRootExceptionEmail)
                    {
                        string msg = "Error: Account for " + Input.Email + " has not been created by the Admin.";
                        ModelState.AddModelError(string.Empty, msg);
                    }
                    else if (emp != null && !emp.Active)
                    {
                        string msg = "Error: Account for login " + Input.Email + " is not active.";
                        ModelState.AddModelError(string.Empty, msg);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
