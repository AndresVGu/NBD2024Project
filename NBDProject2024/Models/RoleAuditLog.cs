using System.ComponentModel.DataAnnotations;

namespace NBDProject2024.Models
{
    public class RoleAuditLog
    {
        public int ID { get; set; }

        [StringLength(256)]
        public string ActorUserId { get; set; }

        [StringLength(256)]
        public string ActorUserName { get; set; }

        [StringLength(256)]
        public string TargetUserId { get; set; }

        [StringLength(256)]
        public string TargetUserName { get; set; }

        [Required]
        [StringLength(60)]
        public string ActionType { get; set; }

        [StringLength(256)]
        public string RoleName { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    }
}
