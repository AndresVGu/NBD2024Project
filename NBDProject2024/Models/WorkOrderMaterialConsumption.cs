using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class WorkOrderMaterialConsumption : Auditable
    {
        public int ID { get; set; }

        [Range(1, int.MaxValue)]
        public int WorkOrderID { get; set; }
        public WorkOrder WorkOrder { get; set; }

        [Range(1, int.MaxValue)]
        public int MaterialID { get; set; }
        public Material Material { get; set; }

        [Range(1, int.MaxValue)]
        public int StockLocationID { get; set; }
        public StockLocation StockLocation { get; set; }

        [DataType(DataType.Date)]
        public DateTime ConsumedOn { get; set; } = DateTime.Today;

        [Range(0.01, 100000)]
        public double QuantityUsed { get; set; }

        [Range(0, 100000)]
        public double UnitCostAtUse { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
