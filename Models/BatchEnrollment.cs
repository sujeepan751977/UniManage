using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class BatchEnrollment
    {
        [Key]
        public int BatchEnrollmentId { get; set; }

        [Required]
        public int BatchId { get; set; }
        public virtual Batch Batch { get; set; }

        [Required]
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AssignedById { get; set; }
        public virtual ApplicationUser AssignedBy { get; set; }
    }
}
