using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        public int? ModuleId { get; set; }
        public virtual Module Module { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxMarks { get; set; }

        public bool AllowLateSubmission { get; set; } = false;

        public string CreatedById { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Submission> Submissions { get; set; }
        public virtual ICollection<AssignmentRelease> AssignmentReleases { get; set; } = new List<AssignmentRelease>();
    }
}
