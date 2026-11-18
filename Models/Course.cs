using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public enum CourseStatus { Active = 0, Archived = 1, Draft = 2 }

    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Range(1, 10)]
        public int Credits { get; set; }

        [Required]
        [Range(1, 500)]
        public int MaxEnrollment { get; set; }

        public string ThumbnailPath { get; set; }

        public CourseStatus Status { get; set; } = CourseStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedById { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<Module> Modules { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<CoursePrerequisite> Prerequisites { get; set; }
        public virtual ICollection<Batch> Batches { get; set; } = new HashSet<Batch>();
    }
}
