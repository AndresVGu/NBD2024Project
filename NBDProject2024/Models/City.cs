using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class City
    {
        public int ID { get; set; }

        #region Summary Properties
        [Display(Name = "City")]
        [DisplayFormat(NullDisplayText = "No City Specified")]
        public string Summary
        {
            get
            {
                return Name + ", " + ProvinceID;
            }
        }

        [DisplayFormat(NullDisplayText = "No Province Specified")]
        public string ProvinceChar
        {
            get
            {
                return ProvinceID;
            }
        }

        [DisplayFormat(NullDisplayText = "No City Specified")]
        public string CityChar
        {
            get
            {
                return Name;
            }
        }
        #endregion

        #region City Properties
        [Display(Name = "City Name")]
        [Required(ErrorMessage = "You cannot leave the name of the city blank.")]
        [StringLength(255, ErrorMessage = "City name cannot be more than 255 characters log.")]
        public string Name { get; set; }
        [Display(Name = "Province")]
        [Required(ErrorMessage = "You must select a province.")]
        [StringLength(2, ErrorMessage = "Province code can only be two capital letters.")]
        public string ProvinceID { get; set; }
        public Province Province { get; set; }
        #endregion
        public ICollection<Client> Clients { get; set; }

    }
}
