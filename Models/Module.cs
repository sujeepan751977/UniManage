using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(20)]
        public string ModuleCode { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        public string LecturerId { get; set; }
        public virtual ApplicationUser Lecturer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<CourseMaterial> Materials { get; set; }
    }
}
