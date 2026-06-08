using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBD2024.Data;
using NBDProject2024.Data;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Root,Admin")]
    public class SystemMaintenanceController : Controller
    {
        private readonly NBDContext _nbdContext;

        public SystemMaintenanceController(NBDContext nbdContext)
        {
            _nbdContext = nbdContext;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetDomainData()
        {
            // Recreate only domain DB to replace stale production data with seed values.
            await _nbdContext.Database.EnsureDeletedAsync();
            await _nbdContext.Database.MigrateAsync();

            NBDInitializer.Seed(_nbdContext);

            TempData["AlertMessage"] = "Domain database was recreated and seeded successfully.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
