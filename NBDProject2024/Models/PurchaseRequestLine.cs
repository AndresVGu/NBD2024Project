using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class PurchaseRequestLine
    {
        public int ID { get; set; }

        public int PurchaseRequestID { get; set; }
        public PurchaseRequest PurchaseRequest { get; set; }

        [Display(Name = "Material")]
        [Range(1, int.MaxValue)]
        public int MaterialID { get; set; }
        public Material Material { get; set; }

        [Display(Name = "Requested Qty")]
        [Range(0.01, 100000)]
        public double RequestedQty { get; set; }

        [Display(Name = "Estimated Unit Cost")]
        [Range(0, 100000)]
        public double EstimatedUnitCost { get; set; }
    }
}
