using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class Bid : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        #region Summary Properties
        public double MaterialTotalAmount => BidMaterials?.Sum(m => m.MaterialQuantity * m.Materials.Price) ?? 0;

        public double LabourTotalAmount => BidLabours?.Sum(l => l.HoursQuantity * l.Labours.Price) ?? 0;

        public string Total
        {
            get
            {
                return (MaterialTotalAmount + LabourTotalAmount).ToString("C2");
            }
        }

        public string MaterialTotal
        {
            get
            {
                return MaterialTotalAmount.ToString("C2");
            }
        }

        public string LabourTotal
        {
            get
            {
                return LabourTotalAmount.ToString("C2");
            }
        }
        #endregion

        #region Bid Properties
        [Display(Name = "Bid Date")]
        [Required(ErrorMessage = "You cannot leave bid date blank.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}", ApplyFormatInEditMode = false)]

        public DateTime BidDate { get; set; } = DateTime.Today;

        //Concurrency:
        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }

        //Forgien keys
        [Display(Name = "Project")]
        public int ProjectID { get; set; }
        public Project Project { get; set; }

        #endregion

        [Display(Name = "Labour")]
        public ICollection<BidLabour> BidLabours { get; set; } = new HashSet<BidLabour>();
        [Display(Name = "Material")]
        public ICollection<BidMaterial> BidMaterials { get; set; } = new HashSet<BidMaterial>();

        #region Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Project Date cannot be in the past, yyyy because that is when NBD open.
            if (BidDate > DateTime.Today)
            {
                yield return new ValidationResult("Date cannot be in the future", new[] { "BidDate" });
            }
        }
        #endregion

    }
}
