using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    [ModelMetadataType(typeof(EmployeeMetaData))]
    public class Employee : Auditable
    {
        public int ID { get; set; }
        #region Summary Properties

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



        public string PhoneFormatted
        {
            get
            {
                return "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone[6..];
            }
        }
        #endregion
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }
        public bool Prescriber { get; set; }

        public Positionemp Position { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; } = true;

        public ICollection<Subscription> Subscriptions { get; set; }


    }
}
