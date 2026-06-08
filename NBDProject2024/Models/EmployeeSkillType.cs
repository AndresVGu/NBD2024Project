using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public enum EmployeeSkillType
    {
        [Display(Name = "Irrigation")]
        Irrigation = 1,
        [Display(Name = "Pruning")]
        Pruning = 2,
        [Display(Name = "Hardscape")]
        Hardscape = 3
    }
}
