using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class InventoryMovement : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Date")]
        public DateTime MovementDate { get; set; } = DateTime.Today;

        [Display(Name = "Movement Type")]
        public InventoryMovementType MovementType { get; set; }

        [Range(1, int.MaxValue)]
        public int MaterialID { get; set; }
        public Material Material { get; set; }

        [Range(1, int.MaxValue)]
        public int StockLocationID { get; set; }
        public StockLocation StockLocation { get; set; }

        [Display(Name = "Quantity")]
        public double QuantityDelta { get; set; }

        [Display(Name = "Before")]
        public double QuantityBefore { get; set; }

        [Display(Name = "After")]
        public double QuantityAfter { get; set; }

        [Display(Name = "Unit Cost")]
        [Range(0, 100000)]
        public double UnitCost { get; set; }

        [Display(Name = "Reference")]
        [StringLength(200)]
        public string ReferenceCode { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }
    }
}
