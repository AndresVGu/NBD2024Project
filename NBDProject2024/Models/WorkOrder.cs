using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class WorkOrder : Auditable, IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "Work Order")]
        [Required(ErrorMessage = "Work order title is required.")]
        [StringLength(120, ErrorMessage = "Title cannot be more than 120 characters long.")]
        public string Title { get; set; }

        [Display(Name = "Scheduled Date")]
        [Required(ErrorMessage = "Scheduled date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}", ApplyFormatInEditMode = false)]
        public DateTime ScheduledDate { get; set; } = DateTime.Today;

        [Display(Name = "Status")]
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending;

        [Display(Name = "Assigned Crew")]
        [StringLength(120, ErrorMessage = "Assigned crew cannot be more than 120 characters long.")]
        public string AssignedCrew { get; set; }

        [Display(Name = "Completion Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MMM-dd}", ApplyFormatInEditMode = false)]
        public DateTime? CompletedOn { get; set; }

        [Display(Name = "Estimated Hours")]
        public double EstimatedHoursTotal => CrewAssignments?.Sum(a => a.EstimatedHours) ?? 0;

        [Display(Name = "Actual Hours")]
        public double ActualHoursTotal => CrewAssignments?.Sum(a => a.ActualHours) ?? 0;

        [Display(Name = "Hours Variance")]
        public double HoursVariance => ActualHoursTotal - EstimatedHoursTotal;

        [Display(Name = "Notes")]
        [StringLength(3000, ErrorMessage = "Notes cannot be more than 3000 characters long.")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [Display(Name = "Project")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select the project.")]
        public int ProjectID { get; set; }
        public Project Project { get; set; }
        public ICollection<WorkOrderCrewAssignment> CrewAssignments { get; set; } = new HashSet<WorkOrderCrewAssignment>();
        public ICollection<WorkOrderMaterialConsumption> MaterialConsumptions { get; set; } = new HashSet<WorkOrderMaterialConsumption>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status == WorkOrderStatus.Completed && !CompletedOn.HasValue)
            {
                yield return new ValidationResult("Completion date is required when status is Completed.", new[] { "CompletedOn" });
            }

            if (CompletedOn.HasValue && CompletedOn.Value.Date < ScheduledDate.Date)
            {
                yield return new ValidationResult("Completion date cannot be before the scheduled date.", new[] { "CompletedOn" });
            }
        }
    }
}
