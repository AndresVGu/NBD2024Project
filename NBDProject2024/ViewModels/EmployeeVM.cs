using Microsoft.AspNetCore.Mvc;
using NBDProject2024.Models;
using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.ViewModels
{
    [ModelMetadataType(typeof(EmployeeMetaData))]
    public class EmployeeVM
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

        [Display(Name ="Number Of Push Subscriptions")]
        public int NumberOfPushSubscriptions { get; set; }

    }
}
