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
    public class MaterialsController : LookupsController
    {
        private readonly NBDContext _context;

        public MaterialsController(NBDContext context)
        {
            _context = context;
        }

        // GET: Inventories
        
       
        public IActionResult Index()
        {

            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Inventories/Details/5
      
      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var inventory = await _context.Materials
                .FirstOrDefaultAsync(m => m.ID == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // GET: Inventories/Create
       
     
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inventories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("ID,Name,Description, Price")] Material inventory)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(inventory);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Material Names.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(inventory);
        }

        // GET: Inventories/Edit/5
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var inventory = await _context.Materials.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        // POST: Inventories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public async Task<IActionResult> Edit(int id)
        {
            var inventoryToUpdate = await _context.Materials
                .FirstOrDefaultAsync(i => i.ID == id);

            if (inventoryToUpdate == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Material>(inventoryToUpdate, "",
                e => e.Name, e => e.Description, e => e.Price
             ))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryExists(inventoryToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Name", "Unable to save changes. Remember, you cannot have duplicate Equipment Names.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(inventoryToUpdate);

        }

        // GET: Inventories/Delete/5
     
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Materials == null)
            {
                return NotFound();
            }

            var inventory = await _context.Materials
                .FirstOrDefaultAsync(m => m.ID == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // POST: Inventories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Materials == null)
            {
                return Problem("No Material To Delete");
            }
            var inventory = await _context.Materials
                .FirstOrDefaultAsync(i => i.ID == id);

            try
            {

                if (inventory != null)
                {
                    _context.Materials.Remove(inventory);
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
            return View(inventory);
        }

        private bool InventoryExists(int id)
        {
            return _context.Materials.Any(e => e.ID == id);
        }
    }
}
