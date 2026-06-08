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
            var guestOwnerId = Request.Cookies["GuestOwnerId"];
            if (string.IsNullOrWhiteSpace(guestOwnerId))
            {
                guestOwnerId = $"guest-{Guid.NewGuid():N}";
            }

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            var guestClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, guestOwnerId),
                new(ClaimTypes.Name, guestOwnerId),
                new(ClaimTypes.Email, $"{guestOwnerId}@nbd.local"),
                new(ClaimTypes.Role, "Guest"),
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

            HttpContext.Response.Cookies.Append("GuestOwnerId", guestOwnerId, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
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
