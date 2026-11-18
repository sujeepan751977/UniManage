using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniManage.Models
{
    public class CourseMaterial
    {
        [Key]
        public int MaterialId { get; set; }

        public int? ModuleId { get; set; }
        public virtual Module Module { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string FileType { get; set; }

        public string UploadedById { get; set; }
        public virtual ApplicationUser UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<MaterialRelease> MaterialReleases { get; set; } = new List<MaterialRelease>();
    }
}
