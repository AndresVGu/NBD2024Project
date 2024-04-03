using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using NBDProject2024.Models;
using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.ViewModels
{
    public class EmployeeAdminVM :EmployeeVM
    {
        public string Email { get; set; }
        public bool Prescriber { get; set; }

        public Positionemp Position { get; set; }

        public bool Active { get; set; }

        [Display(Name ="Roles")]
        public List<string> UserRoles { get; set; } = new List<string>();

    }
}
