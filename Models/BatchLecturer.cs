using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class BatchLecturer
    {
        [Key]
        public int BatchLecturerId { get; set; }

        [Required]
        public int BatchId { get; set; }

        [Required]
        public string LecturerId { get; set; }

        public bool IsPrimary { get; set; } = true;
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string AssignedById { get; set; }

        public virtual Batch Batch { get; set; }
        public virtual ApplicationUser Lecturer { get; set; }
        public virtual ApplicationUser AssignedBy { get; set; }
    }
}
