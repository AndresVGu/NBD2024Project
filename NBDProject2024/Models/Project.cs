using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class Project : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        //Summary Properties By Andres Villarreal
        #region Summary Properties
        [Display(Name = "Summary")]
        public string Summary
        {
            get
            {
                return $"{ProjectName} ,{ProjectSite}";
            }
        }

        //Start Date Summary
        [Display(Name = "Date")]
        public string StartDateSummary
        {
            get
            {
                return StartTime.ToString("f");
            }
        }

        //Time Summary
        [Display(Name = "Start")]
        public string TimeSummary
        {
            get
            {
                return StartTime.ToString("h:mm tt") + "to" + EndTimeSummary;
            }
        }

        //End Time Summary
        [Display(Name = "End")]
        public string EndTimeSummary
        {
            get
            {
                if (EndTime == null)
                {
                    return "Unknown";
                }
                else
                {
                    string endTime = EndTime.GetValueOrDefault().ToString("h:mm tt");
                    TimeSpan difference = ((TimeSpan)(EndTime - StartTime));
                    int days = difference.Days;

                    //Show the Days if there are any
                    if (days > 0)
                    {
                        return endTime + " (" + days + " day" + (days > 1 ? "s" : "");
                    }
                    else
                    {
                        return endTime;
                    }


                }
            }
        }

        //Project Duration
        [Display(Name = "Duration")]
        public string DurationSummary
        {
            get
            {
                if (EndTime == null)
                {
                    return "";
                }
                else
                {
                    TimeSpan d = ((TimeSpan)(EndTime - StartTime));
                    string duration = "";
                    if (d.Days > 0)
                    {
                        duration = d.Days.ToString() + " day" + (d.Days > 1 ? "s" : "");
                    }
                    return duration;
                }
            }
        }
        #endregion

        #region Project Properties:
        //Project Name
        [Display(Name = "Project Name")]
        [Required(ErrorMessage = "you cannot leave project name blank.")]
        [StringLength(100, ErrorMessage = "Project name cannot be more than 100 characters long.")]
        public string ProjectName { get; set; }

        //BidDate
        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "You cannot leave start date blank.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }


        //Estimate Complete Date
        [Display(Name = "Estimated Complete Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}", ApplyFormatInEditMode = false, NullDisplayText = "Not Completed")]
        public DateTime? EndTime { get; set; }

        //Project Site
        [Display(Name = "Project Site")]
        [Required(ErrorMessage = "You cannot leave project site blank.")]
        [StringLength(100, ErrorMessage = "Project Site cannot be more than 100 characters long.")]
        public string ProjectSite { get; set; }

        //City
        [Display(Name = "City")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select the City.")]
        public int? CityID { get; set; }
        public City City { get; set; }

        //Project Setup Notes
        [Display(Name = "Setup Notes")]
        [StringLength(3000, ErrorMessage = "Only 3000 characters for notes.")]
        [DataType(DataType.MultilineText)]
        public string SetupNotes { get; set; }


        #endregion
        //Concurrency:
        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }


        //Foreign Keys
        #region Foreign Keys:
        [Display(Name = "Client")]
        public int ClientID { get; set; }
        public Client Client { get; set; }
        #endregion


        public ICollection<Bid> Bids { get; set; } = new HashSet<Bid>();



        //Staff info:
       // public ICollection<Position> Positions { get; set; } = new HashSet<Position>();

        //Validation Date by Andres Villarreal
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Project Date cannot be before January 1st, yyyy because that is when NBD open.
            if (StartTime < DateTime.Parse("2000-01-01"))
            {
                yield return new ValidationResult("Date cannot be before January 1st, 2000", new[] { "StartTime" });
            }

            //Project Date cannot be more than 10 years in the future from the curren date.
            if (StartTime > DateTime.Now.AddYears(10))
            {
                yield return new ValidationResult("Date cannot be more than 10 years in the future.", new[] { "EndTime" });
            }

            //Project cannot end before it starts
            if (EndTime < StartTime)
            {
                yield return new ValidationResult("Project cannot end before it starts.", new[] { "EndTime" });
            }
        }
    }
}
