using NBDProject2024.Models;
using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.ViewModels
{
    public class EmployeeSkillEditVM
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }

        public List<SkillSelectionVM> Skills { get; set; } = new List<SkillSelectionVM>();
    }

    public class SkillSelectionVM
    {
        public EmployeeSkillType Skill { get; set; }

        [Display(Name = "Enabled")]
        public bool Selected { get; set; }

        [Display(Name = "Level")]
        [Range(1, 5)]
        public int Level { get; set; } = 3;
    }
}
