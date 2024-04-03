using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.CustomControllers;
using NBDProject2024.Data;
using NBDProject2024.Models;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class LaboursController : LookupsController
    {
        private readonly NBDContext _context;

        public LaboursController(NBDContext context)
        {
            _context = context;
        }

        // GET: LabourTypes
      
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: LabourTypes/Details/5
      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Labours == null)
            {
                return NotFound();
            }

            var labourType = await _context.Labours
                .FirstOrDefaultAsync(m => m.ID == id);
            if (labourType == null)
            {
                return NotFound();
            }

            return View(labourType);
        }

        // GET: LabourTypes/Create
       
        public IActionResult Create()
        {
            return View();
        }

        // POST: LabourTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Create([Bind("ID,Name, Description, Price")] Labour labourType)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(labourType);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(labourType);
        }

        // GET: LabourTypes/Edit/5
       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Labours == null)
            {
                return NotFound();
            }

            var labourType = await _context.Labours.FindAsync(id);
            if (labourType == null)
            {
                return NotFound();
            }
            return View(labourType);
        }

        // POST: LabourTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id)
        {
            var labourToUpdate = await _context.Labours
                .FirstOrDefaultAsync(l => l.ID == id);

            if (labourToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Labour>(labourToUpdate, "",
                l => l.Name, l => l.Description, l => l.Price))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabourTypeExists(labourToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(labourToUpdate);
        }

        // GET: LabourTypes/Delete/5
     
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Labours == null)
            {
                return NotFound();
            }

            var labourType = await _context.Labours
                .FirstOrDefaultAsync(m => m.ID == id);
            if (labourType == null)
            {
                return NotFound();
            }

            return View(labourType);
        }

        // POST: LabourTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Labours == null)
            {
                return Problem("No Labours To Delete.");
            }
            var labour = await _context.Labours
                .FirstOrDefaultAsync(l => l.ID == id);

            try
            {

                if (labour != null)
                {
                    _context.Labours.Remove(labour);
                }

                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete " + ViewData["ControllerFriendlyName"] +
                        ". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] + " that has related records.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(labour);

        }

        private bool LabourTypeExists(int id)
        {
            return _context.Labours.Any(e => e.ID == id);
        }
    }
}
