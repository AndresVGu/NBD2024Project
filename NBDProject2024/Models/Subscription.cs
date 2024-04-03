using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        [StringLength(512)]
        public string PushEndpoint { get; set; }
        [StringLength(512)]
        public string PushP256DH { get; set; }
        [StringLength(512)]
        public string PushAuth {  get; set; }

        [Required(ErrorMessage ="You must select the staff Member.")]
        [Display(Name ="Employee")]
        public int EmployeeID {  get; set; }
        public Employee Employee { get; set; }
    }
}
