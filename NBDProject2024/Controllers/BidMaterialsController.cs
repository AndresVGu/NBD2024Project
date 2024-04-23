using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;
using NBDProject2024.Utilities;

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

        public PartialViewResult CreateMaterial(int? GenreID, int? AlbumID)
        {
            //Having the Album's GenreID allows us to set it as
            //the default for the new song.
            ViewData["BidID"] = new
                SelectList(_context.Bids
                .OrderBy(a => a.BidDate), "ID", "Date", GenreID.GetValueOrDefault());

            //So we can save it in the Form
            ViewData["BiID"] = AlbumID.GetValueOrDefault();

            return PartialView("_CreateMaterial");
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

        public async Task<PartialViewResult> EditMaterial(int ID)
        {
            //Get the Song to edit
            Material song = await _context.Materials
                .Include(p => p.BidMaterials)
                .Where(p => p.ID == ID)
                .FirstOrDefaultAsync();

            ViewData["BidID"] = new
                SelectList(_context.Bids
                .OrderBy(a => a.BidDate), "ID", "Date", song.ID);

            return PartialView("_EditMaterial", song);
        }

        // POST: BidMaterials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MaterialQuantity,BidID,MaterialID")] BidMaterial bidMaterial)
        {
            var songToUpdate = await _context.BidMaterials.Include(e => e.Materials)
               .FirstOrDefaultAsync(m => m.ID == id);

            if (songToUpdate == null)
            {
                return NotFound("Unable to get the data. The Song was deleted by another user.");
            }

            //Put the original RowVersion value in the OriginalValues collection for the entity
            //_context.Entry(songToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            if (await TryUpdateModelAsync<BidMaterial>(songToUpdate, "",
                p => p.MaterialQuantity, p => p.MaterialID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Ok();// RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    //ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please go back and refresh.");
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (BidMaterial)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Song was deleted by another user.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "are listed below.");
                        var databaseValues = (BidMaterial)databaseEntry.ToObject();
                        if (databaseValues.MaterialID != clientValues.MaterialID)
                            ModelState.AddModelError("Title", "Title: Current value: "
                                + databaseValues.MaterialID);
                        if (databaseValues.MaterialQuantity != clientValues.MaterialQuantity)
                            ModelState.AddModelError("DateRecorded", "Date Recorded: Current value: "
                                + String.Format("{0:d}", databaseValues.MaterialQuantity));
                        //For the foreign key, we need to go to the database to get the information to show
                        if (databaseValues.BidID != clientValues.BidID)
                        {
                            Bid databaseGenre = await _context.Bids.FirstOrDefaultAsync(i => i.ID == databaseValues.ID);
                            ModelState.AddModelError("GenreID", $"Genre: Current value: {databaseGenre?.ID}");
                        }
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                    {
                        ModelState.AddModelError("Title", "Remember, you cannot have duplicate Song Titles on the same Album");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
                if (id != bidMaterial.ID)
                {
                    return NotFound();
                }

                //if (ModelState.IsValid)
                //{
                //    try
                //    {
                //        _context.Update(bidMaterial);
                //        await _context.SaveChangesAsync();
                //    }
                //    catch (DbUpdateConcurrencyException)
                //    {
                //        if (!BidMaterialExists(bidMaterial.ID))
                //        {
                //            return NotFound();
                //        }
                //        else
                //        {
                //            throw;
                //        }
                //    }
                //    return RedirectToAction(nameof(Index));
                //}
                //ViewData["BidID"] = new SelectList(_context.Bids, "ID", "ID", bidMaterial.BidID);
                //ViewData["MaterialID"] = new SelectList(_context.Materials, "ID", "Name", bidMaterial.MaterialID);
                //return View(bidMaterial);
            }

            return BadRequest(BuildMessages.ErrorMessage(ModelState));
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
