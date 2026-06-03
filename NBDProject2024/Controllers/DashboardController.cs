using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                Revenue = 34152,
                Orders = 5643,
                Customers = 45254,
                Growth = 12.58M
            };

            return View(model);
        }
    }
}