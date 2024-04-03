using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class Client : Auditable
    {
        public int ID { get; set; }

        #region Summary Properties
        public string Summary => FullName;

        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                    (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }


        [Display(Name = "Phone")]
        public string PhoneFormatted
        {
            get
            {
                return "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone[6..];
            }
        }
        #endregion

        #region Client Properties
        //Client Personal Info:

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        [Display(Name = "Company Name")]
        [Required(ErrorMessage = "You cannot leave company name blank.")]
        [StringLength(300, ErrorMessage = "Company name cannot be more than 300 characters long.")]
        public string CompanyName { get; set; }

        [Display(Name = "Phone Number")]
        [Required(ErrorMessage = "You cannot leave the phone number blank.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "The phone number must be exactly 10 numeric digits.")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        //Client Address Info:
        //Country
        [Display(Name = "Country")]
        [Required(ErrorMessage = "Country required.")]
        [StringLength(50, ErrorMessage = "Country connot be more than 50 characters long.")]
        public string AddressCountry { get; set; }
        //Street
        [Display(Name = "Street")]
        [Required(ErrorMessage = "Street is required.")]
        [StringLength(255)]
        public string AddressStreet { get; set; }
        //City
        [Display(Name = "City")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select the City.")]
        public int? CityID { get; set; }
        public City City { get; set; }

        //Postal Code
        [Display(Name = "Postal Code")]
        [Required(ErrorMessage = "Postal Code is required.")]
        [StringLength(6, ErrorMessage = "Postal code cannot be more than 6 characters long.")]
        public string PostalCode { get; set; }

        //Concurrency:
        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }

        #endregion
        public ICollection<Project> Projects { get; set; } = new HashSet<Project>();

    }
}
