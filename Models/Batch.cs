using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public enum BatchStatus { Upcoming = 0, Active = 1, Completed = 2, Cancelled = 3 }

    public class Batch
    {
        [Key]
        public int BatchId { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        [StringLength(100)]
        public string BatchName { get; set; }

        [Required]
        [StringLength(20)]
        public string BatchCode { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, 200)]
        public int MaxCapacity { get; set; }

        public BatchStatus Status { get; set; } = BatchStatus.Upcoming;

        [Required]
        public string CreatedById { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<BatchEnrollment> BatchEnrollments { get; set; } = new List<BatchEnrollment>();
        public virtual ICollection<BatchLecturer> BatchLecturers { get; set; } = new List<BatchLecturer>();
        public virtual ICollection<AssignmentRelease> AssignmentReleases { get; set; } = new List<AssignmentRelease>();
        public virtual ICollection<MaterialRelease> MaterialReleases { get; set; } = new List<MaterialRelease>();
    }
}
