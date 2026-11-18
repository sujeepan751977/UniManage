using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class AssignmentRelease
    {
        [Key]
        public int ReleaseId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int BatchId { get; set; }

        public DateTime ReleasedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CustomDeadline { get; set; }
        // If set, overrides Assignment.Deadline for this batch only

        public bool IsVisible { get; set; } = true;

        public string ReleasedById { get; set; }

        public virtual Assignment Assignment { get; set; }
        public virtual Batch Batch { get; set; }
        public virtual ApplicationUser ReleasedBy { get; set; }
    }
}
