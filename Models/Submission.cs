using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }

        [Required]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public bool IsLate { get; set; }

        [Range(0, 100)]
        public decimal? Grade { get; set; }

        [StringLength(1000)]
        public string Feedback { get; set; }

        public DateTime? GradedAt { get; set; }

        public string GradedById { get; set; }
        public virtual ApplicationUser GradedBy { get; set; }
    }
}
