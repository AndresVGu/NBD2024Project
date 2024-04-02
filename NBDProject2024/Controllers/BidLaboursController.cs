using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;

namespace NBDProject2024.Controllers
{
    public class BidLaboursController : Controller
    {
        private readonly NBDContext _context;

        public BidLaboursController(NBDContext context)
        {
            _context = context;
        }

        // GET: BidLabours
        public async Task<IActionResult> Index()
        {
            var nBDContext = _context.BidLabours.Include(b => b.Bid).Include(b => b.Labours);
            return View(await nBDContext.ToListAsync());
        }

        // GET: BidLabours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BidLabours == null)
            {
                return NotFound();
            }

            var bidLabour = await _context.BidLabours
                .Include(b => b.Bid)
                .Include(b => b.Labours)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bidLabour == null)
            {
                return NotFound();
            }

            return View(bidLabour);
        }

        // GET: BidLabours/Create
        public IActionResult Create()
        {
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID");
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name");
            return View();
        }

        // POST: BidLabours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,HoursQuantity,BidID,LabourID")] BidLabour bidLabour)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bidLabour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidLabour.BidID);
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name", bidLabour.LabourID);
            return View(bidLabour);
        }

        // GET: BidLabours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BidLabours == null)
            {
                return NotFound();
            }

            var bidLabour = await _context.BidLabours.FindAsync(id);
            if (bidLabour == null)
            {
                return NotFound();
            }
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidLabour.BidID);
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name", bidLabour.LabourID);
            return View(bidLabour);
        }

        // POST: BidLabours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,HoursQuantity,BidID,LabourID")] BidLabour bidLabour)
        {
            if (id != bidLabour.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bidLabour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BidLabourExists(bidLabour.ID))
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
            ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidLabour.BidID);
            ViewData["LabourID"] = new SelectList(_context.Labours, "ID", "Name", bidLabour.LabourID);
            return View(bidLabour);
        }

        // GET: BidLabours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BidLabours == null)
            {
                return NotFound();
            }

            var bidLabour = await _context.BidLabours
                .Include(b => b.Bid)
                .Include(b => b.Labours)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bidLabour == null)
            {
                return NotFound();
            }

            return View(bidLabour);
        }

        // POST: BidLabours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BidLabours == null)
            {
                return Problem("Entity set 'NBDContext.BidLabours'  is null.");
            }
            var bidLabour = await _context.BidLabours.FindAsync(id);
            if (bidLabour != null)
            {
                _context.BidLabours.Remove(bidLabour);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BidLabourExists(int id)
        {
          return _context.BidLabours.Any(e => e.ID == id);
        }
    }
}
