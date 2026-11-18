using System.Collections.Generic;

namespace UniManage.ViewModels.Student
{
    public class BrowseCoursesViewModel
    {
        public List<CourseCardVM> Courses { get; set; } = new List<CourseCardVM>();
        public string SearchQuery { get; set; }
        public string FilterStatus { get; set; }
        public int TotalCount { get; set; }
    }

    public class CourseCardVM
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string LecturerName { get; set; }
        public int Credits { get; set; }
        public int MaxEnrollment { get; set; }
        public int CurrentEnrollment { get; set; }
        public string ThumbnailPath { get; set; }
        public string Status { get; set; }
        public bool IsEnrolled { get; set; }
        public bool IsPending { get; set; }
        public bool CanEnroll { get; set; }
        public List<string> Prerequisites { get; set; } = new List<string>();
    }
}
