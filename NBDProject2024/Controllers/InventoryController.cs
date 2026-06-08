using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Root")]
    public class InventoryController : Controller
    {
        private readonly NBDContext _context;

        public InventoryController(NBDContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stocks = await _context.MaterialStocks
                .Include(s => s.Material)
                .Include(s => s.StockLocation)
                .OrderBy(s => s.StockLocation.LocationType)
                .ThenBy(s => s.StockLocation.Name)
                .ThenBy(s => s.Material.Name)
                .ToListAsync();

            ViewData["AlertCount"] = stocks.Count(s => s.NeedsReorder);
            return View(stocks);
        }

        public async Task<IActionResult> Movements(int? locationID, int? materialID)
        {
            var query = _context.InventoryMovements
                .AsNoTracking()
                .Include(m => m.Material)
                .Include(m => m.StockLocation)
                .AsQueryable();

            if (locationID.HasValue)
            {
                query = query.Where(m => m.StockLocationID == locationID.Value);
            }

            if (materialID.HasValue)
            {
                query = query.Where(m => m.MaterialID == materialID.Value);
            }

            ViewData["LocationID"] = new SelectList(_context.StockLocations.OrderBy(l => l.Name), "ID", "Name", locationID);
            ViewData["MaterialID"] = new SelectList(_context.Materials.OrderBy(m => m.Name), "ID", "Name", materialID);

            var movements = await query
                .OrderByDescending(m => m.MovementDate)
                .ThenByDescending(m => m.ID)
                .ToListAsync();

            return View(movements);
        }

        public async Task<IActionResult> Locations()
        {
            var locations = await _context.StockLocations
                .OrderBy(l => l.LocationType)
                .ThenBy(l => l.Name)
                .ToListAsync();
            return View(locations);
        }

        public IActionResult CreateLocation()
        {
            return View(new StockLocation());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation([Bind("Name,LocationType,IsActive,Notes")] StockLocation location)
        {
            if (ModelState.IsValid)
            {
                _context.StockLocations.Add(location);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Location created.";
                return RedirectToAction(nameof(Locations));
            }
            return View(location);
        }

        public async Task<IActionResult> EditLocation(int id)
        {
            var location = await _context.StockLocations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(int id, [Bind("ID,Name,LocationType,IsActive,Notes")] StockLocation form)
        {
            var location = await _context.StockLocations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(location, "", l => l.Name, l => l.LocationType, l => l.IsActive, l => l.Notes))
            {
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Location updated.";
                return RedirectToAction(nameof(Locations));
            }

            return View(location);
        }

        public IActionResult CreateStock()
        {
            PopulateSelections();
            return View(new MaterialStock());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStock([Bind("MaterialID,StockLocationID,QuantityOnHand,MinQuantity,LastUnitCost")] MaterialStock stock)
        {
            if (ModelState.IsValid)
            {
                _context.MaterialStocks.Add(stock);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Stock record created.";
                return RedirectToAction(nameof(Index));
            }

            PopulateSelections(stock.MaterialID, stock.StockLocationID);
            return View(stock);
        }

        public async Task<IActionResult> EditStock(int id)
        {
            var stock = await _context.MaterialStocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            PopulateSelections(stock.MaterialID, stock.StockLocationID);
            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStock(int id, [Bind("ID,MaterialID,StockLocationID,QuantityOnHand,MinQuantity,LastUnitCost")] MaterialStock form)
        {
            var stock = await _context.MaterialStocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(stock, "", s => s.MaterialID, s => s.StockLocationID, s => s.QuantityOnHand, s => s.MinQuantity, s => s.LastUnitCost))
            {
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Stock updated.";
                return RedirectToAction(nameof(Index));
            }

            PopulateSelections(stock.MaterialID, stock.StockLocationID);
            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(int id, double deltaQty)
        {
            var stock = await _context.MaterialStocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            var before = stock.QuantityOnHand;
            stock.QuantityOnHand = Math.Max(0, stock.QuantityOnHand + deltaQty);

            _context.InventoryMovements.Add(new InventoryMovement
            {
                MovementDate = DateTime.Today,
                MovementType = InventoryMovementType.ManualAdjustment,
                MaterialID = stock.MaterialID,
                StockLocationID = stock.StockLocationID,
                QuantityDelta = stock.QuantityOnHand - before,
                QuantityBefore = before,
                QuantityAfter = stock.QuantityOnHand,
                UnitCost = stock.LastUnitCost,
                ReferenceCode = $"ADJ-{stock.ID}",
                Notes = "Manual stock adjustment"
            });

            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Stock adjusted.";
            return RedirectToAction(nameof(Index));
        }

        private void PopulateSelections(int? selectedMaterial = null, int? selectedLocation = null)
        {
            ViewData["MaterialID"] = new SelectList(_context.Materials.OrderBy(m => m.Name), "ID", "Name", selectedMaterial);
            ViewData["StockLocationID"] = new SelectList(_context.StockLocations.OrderBy(l => l.Name), "ID", "Name", selectedLocation);
        }
    }
}
