using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class MaterialStock : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Material")]
        [Range(1, int.MaxValue)]
        public int MaterialID { get; set; }
        public Material Material { get; set; }

        [Display(Name = "Location")]
        [Range(1, int.MaxValue)]
        public int StockLocationID { get; set; }
        public StockLocation StockLocation { get; set; }

        [Display(Name = "On Hand")]
        [Range(0, 100000)]
        public double QuantityOnHand { get; set; }

        [Display(Name = "Minimum")]
        [Range(0, 100000)]
        public double MinQuantity { get; set; }

        [Display(Name = "Last Unit Cost")]
        [Range(0, 100000)]
        public double LastUnitCost { get; set; }

        [Display(Name = "Needs Reorder")]
        public bool NeedsReorder => QuantityOnHand <= MinQuantity;
    }
}
