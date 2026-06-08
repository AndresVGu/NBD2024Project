using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class PurchaseRequest : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Request Date")]
        [DataType(DataType.Date)]
        public DateTime RequestDate { get; set; } = DateTime.Today;

        [Display(Name = "Supplier")]
        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; }

        [Display(Name = "Status")]
        public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Draft;

        [Display(Name = "Approved By")]
        [StringLength(256)]
        public string ApprovedBy { get; set; }

        [Display(Name = "Approved On")]
        public DateTime? ApprovedOn { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public ICollection<PurchaseRequestLine> Lines { get; set; } = new HashSet<PurchaseRequestLine>();
        public ICollection<PurchaseReceiptLine> Receipts { get; set; } = new HashSet<PurchaseReceiptLine>();
    }
}
