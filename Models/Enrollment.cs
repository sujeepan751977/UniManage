using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public enum EnrollmentStatus { Pending = 0, Active = 1, Rejected = 2, Dropped = 3, Completed = 4 }

    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        [Range(0, 100)]
        public decimal? FinalGrade { get; set; }

        // Approval workflow fields
        public string ApprovedById { get; set; }
        public virtual ApplicationUser ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(500)]
        public string RejectionReason { get; set; }

        [StringLength(500)]
        public string AdminNotes { get; set; }

        // Batch Assignment
        public virtual ICollection<BatchEnrollment> BatchEnrollments { get; set; }
    }
}
