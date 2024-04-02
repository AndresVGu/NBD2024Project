using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;

namespace NBDProject2024.Controllers
{
    [AllowAnonymous]
    public class BidMaterialsController : Controller
    {
        private readonly NBDContext _context;

        public BidMaterialsController(NBDContext context)
        {
            _context = context;
        }

        // GET: BidMaterials
        public async Task<IActionResult> Index()
        {
            var nBDContext = _context.BidMaterials.Include(b => b.Bid).Include(b => b.Materials);
            return View(await nBDContext.ToListAsync());
        }

        // GET: BidMaterials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BidMaterials == null)
            {
                return NotFound();
            }

            var bidMaterial = await _context.BidMaterials
                .Include(b => b.Bid)
                .Include(b => b.Materials)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bidMaterial == null)
            {
                return NotFound();
            }

            return View(bidMaterial);
        }

        // GET: BidMaterials/Create
        public IActionResult Create()
        {
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID");
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name");
            return View();
        }

        // POST: BidMaterials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MaterialQuantity,BidID,MaterialID")] BidMaterial bidMaterial)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bidMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidMaterial.BidID);
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name", bidMaterial.MaterialID);
            return View(bidMaterial);
        }

        // GET: BidMaterials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BidMaterials == null)
            {
                return NotFound();
            }

            var bidMaterial = await _context.BidMaterials.FindAsync(id);
            if (bidMaterial == null)
            {
                return NotFound();
            }
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidMaterial.BidID);
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name", bidMaterial.MaterialID);
            return View(bidMaterial);
        }

        // POST: BidMaterials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MaterialQuantity,BidID,MaterialID")] BidMaterial bidMaterial)
        {
            if (id != bidMaterial.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bidMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BidMaterialExists(bidMaterial.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidMaterial.BidID);
            ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name", bidMaterial.MaterialID);
            return View(bidMaterial);
        }

        // GET: BidMaterials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BidMaterials == null)
            {
                return NotFound();
            }

            var bidMaterial = await _context.BidMaterials
                .Include(b => b.Bid)
                .Include(b => b.Materials)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bidMaterial == null)
            {
                return NotFound();
            }

            return View(bidMaterial);
        }

        // POST: BidMaterials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BidMaterials == null)
            {
                return Problem("Entity set 'NBDContext.BidMaterials'  is null.");
            }
            var bidMaterial = await _context.BidMaterials.FindAsync(id);
            if (bidMaterial != null)
            {
                _context.BidMaterials.Remove(bidMaterial);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BidMaterialExists(int id)
        {
          return _context.BidMaterials.Any(e => e.ID == id);
        }
    }
}
