using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class EmployeeSkill
    {
        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        [Display(Name = "Skill")]
        public EmployeeSkillType Skill { get; set; }

        [Display(Name = "Level (1-5)")]
        [Range(1, 5, ErrorMessage = "Level must be between 1 and 5.")]
        public int Level { get; set; } = 3;
    }
}
