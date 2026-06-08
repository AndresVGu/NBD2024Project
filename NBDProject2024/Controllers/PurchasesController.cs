using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Data;
using NBDProject2024.Models;

namespace NBDProject2024.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Root")]
    public class PurchasesController : Controller
    {
        private readonly NBDContext _context;

        public PurchasesController(NBDContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.PurchaseRequests
                .Include(r => r.Lines)
                .Include(r => r.Receipts)
                .OrderByDescending(r => r.RequestDate)
                .ThenByDescending(r => r.ID)
                .ToListAsync();

            return View(requests);
        }

        public IActionResult Create()
        {
            return View(new PurchaseRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequestDate,SupplierName,Status,Notes")] PurchaseRequest request)
        {
            request.Status = PurchaseRequestStatus.Draft;
            if (ModelState.IsValid)
            {
                _context.PurchaseRequests.Add(request);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Purchase request created.";
                return RedirectToAction(nameof(Details), new { id = request.ID });
            }

            return View(request);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.PurchaseRequests
                .Include(r => r.Lines)
                .ThenInclude(l => l.Material)
                .Include(r => r.Receipts)
                .ThenInclude(rc => rc.Material)
                .Include(r => r.Receipts)
                .ThenInclude(rc => rc.StockLocation)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (request == null)
            {
                return NotFound();
            }

            PopulateSelections();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLine(int id, int materialID, double requestedQty, double estimatedUnitCost)
        {
            var request = await _context.PurchaseRequests.FirstOrDefaultAsync(r => r.ID == id);
            if (request == null)
            {
                return NotFound();
            }

            if (requestedQty <= 0)
            {
                TempData["AlertMessage"] = "Requested quantity must be greater than 0.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (request.Status == PurchaseRequestStatus.Received || request.Status == PurchaseRequestStatus.Cancelled)
            {
                TempData["AlertMessage"] = "Lines cannot be edited for received or cancelled requests.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.PurchaseRequestLines.Add(new PurchaseRequestLine
            {
                PurchaseRequestID = id,
                MaterialID = materialID,
                RequestedQty = requestedQty,
                EstimatedUnitCost = Math.Max(0, estimatedUnitCost)
            });

            if (request.Status == PurchaseRequestStatus.Draft)
            {
                request.Status = PurchaseRequestStatus.Submitted;
            }

            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Line added.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id, int materialID, int stockLocationID, DateTime receivedDate,
            double receivedQty, double actualUnitCost, string supplierInvoice)
        {
            var request = await _context.PurchaseRequests
                .Include(r => r.Lines)
                .Include(r => r.Receipts)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (request == null)
            {
                return NotFound();
            }

            if (receivedQty <= 0)
            {
                TempData["AlertMessage"] = "Received quantity must be greater than 0.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (request.Status == PurchaseRequestStatus.Draft || request.Status == PurchaseRequestStatus.Cancelled)
            {
                TempData["AlertMessage"] = "Request must be submitted/approved before receiving.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.PurchaseReceiptLines.Add(new PurchaseReceiptLine
            {
                PurchaseRequestID = id,
                MaterialID = materialID,
                StockLocationID = stockLocationID,
                ReceivedDate = receivedDate.Date,
                ReceivedQty = receivedQty,
                ActualUnitCost = Math.Max(0, actualUnitCost),
                SupplierInvoice = supplierInvoice
            });

            var stock = await _context.MaterialStocks
                .FirstOrDefaultAsync(s => s.MaterialID == materialID && s.StockLocationID == stockLocationID);

            if (stock == null)
            {
                stock = new MaterialStock
                {
                    MaterialID = materialID,
                    StockLocationID = stockLocationID,
                    QuantityOnHand = 0,
                    MinQuantity = 0,
                    LastUnitCost = 0
                };
                _context.MaterialStocks.Add(stock);
            }

            stock.QuantityOnHand += receivedQty;
            stock.LastUnitCost = Math.Max(0, actualUnitCost);

            _context.InventoryMovements.Add(new InventoryMovement
            {
                MovementDate = receivedDate.Date,
                MovementType = InventoryMovementType.PurchaseReceipt,
                MaterialID = materialID,
                StockLocationID = stockLocationID,
                QuantityDelta = receivedQty,
                QuantityBefore = stock.QuantityOnHand - receivedQty,
                QuantityAfter = stock.QuantityOnHand,
                UnitCost = Math.Max(0, actualUnitCost),
                ReferenceCode = $"PR-{id}",
                Notes = $"Supplier: {request.SupplierName}; Invoice: {supplierInvoice}"
            });

            await _context.SaveChangesAsync();
            await RefreshStatusAsync(request.ID);

            TempData["AlertMessage"] = "Receipt registered and inventory updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var request = await _context.PurchaseRequests
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.ID == id);
            if (request == null)
            {
                return NotFound();
            }

            if (!request.Lines.Any())
            {
                TempData["AlertMessage"] = "Add at least one line before submitting.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (request.Status == PurchaseRequestStatus.Draft)
            {
                request.Status = PurchaseRequestStatus.Submitted;
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Purchase request submitted.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.PurchaseRequests.FirstOrDefaultAsync(r => r.ID == id);
            if (request == null)
            {
                return NotFound();
            }

            if (request.Status == PurchaseRequestStatus.Submitted || request.Status == PurchaseRequestStatus.PartiallyReceived)
            {
                request.ApprovedBy = User.Identity?.Name ?? "Unknown";
                request.ApprovedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Purchase request approved.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _context.PurchaseRequests
                .Include(r => r.Receipts)
                .FirstOrDefaultAsync(r => r.ID == id);
            if (request == null)
            {
                return NotFound();
            }

            if (request.Receipts.Any())
            {
                TempData["AlertMessage"] = "Cannot cancel a request with receipts.";
                return RedirectToAction(nameof(Details), new { id });
            }

            request.Status = PurchaseRequestStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Purchase request cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task RefreshStatusAsync(int requestId)
        {
            var request = await _context.PurchaseRequests
                .Include(r => r.Lines)
                .Include(r => r.Receipts)
                .FirstOrDefaultAsync(r => r.ID == requestId);

            if (request == null)
            {
                return;
            }

            if (!request.Lines.Any())
            {
                request.Status = PurchaseRequestStatus.Draft;
                await _context.SaveChangesAsync();
                return;
            }

            bool allReceived = request.Lines.All(line =>
                request.Receipts.Where(r => r.MaterialID == line.MaterialID).Sum(r => r.ReceivedQty) >= line.RequestedQty);

            bool anyReceived = request.Receipts.Any();

            request.Status = allReceived
                ? PurchaseRequestStatus.Received
                : anyReceived ? PurchaseRequestStatus.PartiallyReceived : PurchaseRequestStatus.Submitted;

            await _context.SaveChangesAsync();
        }

        private void PopulateSelections()
        {
            ViewData["MaterialID"] = new SelectList(_context.Materials.OrderBy(m => m.Name), "ID", "Name");
            ViewData["StockLocationID"] = new SelectList(_context.StockLocations.Where(l => l.IsActive).OrderBy(l => l.Name), "ID", "Name");
        }
    }
}
