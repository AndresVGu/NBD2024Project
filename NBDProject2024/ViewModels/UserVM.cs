using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.ViewModels
{
    public class UserVM
    {
        public string ID { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name ="Roles")]
        public List<string> UserRoles { get; set; }
    }
}
