using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class MaterialRelease
    {
        [Key]
        public int ReleaseId { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int BatchId { get; set; }

        public DateTime ReleasedAt { get; set; } = DateTime.UtcNow;

        public bool IsVisible { get; set; } = true;

        public string ReleasedById { get; set; }

        public virtual CourseMaterial Material { get; set; }
        public virtual Batch Batch { get; set; }
        public virtual ApplicationUser ReleasedBy { get; set; }
    }
}
