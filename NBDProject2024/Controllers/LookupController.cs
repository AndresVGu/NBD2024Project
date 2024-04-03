using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles ="Admin,Supervisor")]
    public class LookupController : CognizantController
    {
        
        private readonly NBDContext _context;

        public LookupController(NBDContext context)
        {
            _context = context;
        }

        public IActionResult Index(string Tab = "Material-Tab")
        {
            ViewData["Tab"] = Tab;
            return View();
        }

        public PartialViewResult Material()
        {
            ViewData["MaterialID"] = new
                SelectList(_context.Materials
                .OrderBy(m => m.Name), "ID", "Summary");
            return PartialView("_Material");
        }

        public PartialViewResult Labour()
        {
            ViewData["LabourID"] = new
                SelectList(_context.Labours
                .OrderBy(l => l.Name), "ID", "Summary");
            return PartialView("_Labour");
        }
    }
}
