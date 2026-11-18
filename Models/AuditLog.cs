using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        public string PerformedById { get; set; }
        public virtual ApplicationUser PerformedBy { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [StringLength(100)]
        public string EntityAffected { get; set; }

        [StringLength(50)]
        public string EntityId { get; set; }

        [StringLength(500)]
        public string Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(300)]
        public string UserAgent { get; set; }
    }
}
