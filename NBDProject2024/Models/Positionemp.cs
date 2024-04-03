using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public enum Positionemp
    {
        None,
        [Display(Name ="On site Worker")]
        Worker,
        [Display(Name ="Sales Advisor")]
        Sales,
        [Display(Name ="Designer")]
        Design,
        [Display(Name ="Manager")]
        Management
    }
}
