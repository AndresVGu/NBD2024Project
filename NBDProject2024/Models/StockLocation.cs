using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class StockLocation : Auditable
    {
        public int ID { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; }

        [Display(Name = "Location Type")]
        public StockLocationType LocationType { get; set; } = StockLocationType.Warehouse;

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string Notes { get; set; }

        public ICollection<MaterialStock> MaterialStocks { get; set; } = new HashSet<MaterialStock>();
    }
}
