using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManage.Models
{
    public class CoursePrerequisite
    {
        [Key, Column(Order = 0)]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Key, Column(Order = 1)]
        public int PrerequisiteCourseId { get; set; }
        public virtual Course PrerequisiteCourse { get; set; }
    }
}
