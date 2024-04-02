using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class BidLabour
    {
        public int ID { get; set; }


        [Display(Name = "Hours")]
        [Required(ErrorMessage = "You cannot Leave number of Hours blank.")]
        public double HoursQuantity { get; set; } = 1;


        //Foreign Keys:
        [Display(Name = "Bid")]
        public int BidID { get; set; }
        public Bid Bid { get; set; }

        [Display(Name = "Labour")]
        public int LabourID { get; set; }
        public Labour Labours { get; set; }


    }
}
