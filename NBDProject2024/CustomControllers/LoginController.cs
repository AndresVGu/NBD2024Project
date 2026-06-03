using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NBDProject2024.Utilities;
using System.Security.Claims;

namespace NBDProject2024.CustomControllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Guest(string returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            var guestClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "guest-user"),
                new(ClaimTypes.Name, "Guest User"),
                new(ClaimTypes.Email, "guest@nbd.local"),
                new(ClaimTypes.Role, "Guest"),
                new(ClaimTypes.Role, "Admin"),
                new(ClaimTypes.Role, "Supervisor"),
                new(ClaimTypes.Role, "Designer"),
                new(ClaimTypes.Role, "Sales"),
                new("GuestMode", "true")
            };

            var guestIdentity = new ClaimsIdentity(
                guestClaims,
                IdentityConstants.ApplicationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role);

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                new ClaimsPrincipal(guestIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            CookieHelper.CookieSet(HttpContext, "userName", "Guest User", 480);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExitGuest()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            HttpContext.Response.Cookies.Delete("userName");
            return RedirectToAction("Index", "Home");
        }
    }
}
