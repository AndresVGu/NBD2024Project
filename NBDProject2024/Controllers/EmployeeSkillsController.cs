using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.ViewModels;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class EmployeeSkillsController : ElephantController
    {
        private readonly NBDContext _context;

        public EmployeeSkillsController(NBDContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Employees
                .Include(e => e.EmployeeSkills)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .AsNoTracking()
                .ToListAsync();

            return View(data);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeSkills)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ID == id);

            if (employee == null)
            {
                return NotFound();
            }

            var vm = new EmployeeSkillEditVM
            {
                EmployeeID = employee.ID,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Skills = Enum.GetValues(typeof(EmployeeSkillType))
                    .Cast<EmployeeSkillType>()
                    .Select(skill =>
                    {
                        var existing = employee.EmployeeSkills.FirstOrDefault(s => s.Skill == skill);
                        return new SkillSelectionVM
                        {
                            Skill = skill,
                            Selected = existing != null,
                            Level = existing?.Level ?? 3
                        };
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeSkillEditVM vm)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.ID == vm.EmployeeID);
            if (employee == null)
            {
                return NotFound();
            }

            var existing = await _context.EmployeeSkills
                .Where(s => s.EmployeeID == vm.EmployeeID)
                .ToListAsync();

            _context.EmployeeSkills.RemoveRange(existing);

            foreach (var skill in vm.Skills.Where(s => s.Selected))
            {
                _context.EmployeeSkills.Add(new EmployeeSkill
                {
                    EmployeeID = vm.EmployeeID,
                    Skill = skill.Skill,
                    Level = Math.Clamp(skill.Level, 1, 5)
                });
            }

            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Employee skills updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
