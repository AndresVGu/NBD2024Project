using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class PurchaseReceiptLine
    {
        public int ID { get; set; }

        public int PurchaseRequestID { get; set; }
        public PurchaseRequest PurchaseRequest { get; set; }

        [Display(Name = "Material")]
        [Range(1, int.MaxValue)]
        public int MaterialID { get; set; }
        public Material Material { get; set; }

        [Display(Name = "Receive Into")]
        [Range(1, int.MaxValue)]
        public int StockLocationID { get; set; }
        public StockLocation StockLocation { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReceivedDate { get; set; } = DateTime.Today;

        [Display(Name = "Received Qty")]
        [Range(0.01, 100000)]
        public double ReceivedQty { get; set; }

        [Display(Name = "Actual Unit Cost")]
        [Range(0, 100000)]
        public double ActualUnitCost { get; set; }

        [Display(Name = "Supplier Invoice")]
        [StringLength(120)]
        public string SupplierInvoice { get; set; }
    }
}
