using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class WorkOrderCrewAssignment
    {
        public int WorkOrderID { get; set; }
        public WorkOrder WorkOrder { get; set; }

        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        [Display(Name = "Skill")]
        public EmployeeSkillType AssignedSkill { get; set; }

        [Display(Name = "Estimated Hours")]
        [Range(0, 24, ErrorMessage = "Estimated hours must be between 0 and 24.")]
        public double EstimatedHours { get; set; }

        [Display(Name = "Actual Hours")]
        [Range(0, 24, ErrorMessage = "Actual hours must be between 0 and 24.")]
        public double ActualHours { get; set; }
    }
}
