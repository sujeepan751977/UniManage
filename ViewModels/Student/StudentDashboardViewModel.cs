using System;
using System.Collections.Generic;

namespace UniManage.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public List<EnrolledCourseVM> EnrolledCourses { get; set; } = new List<EnrolledCourseVM>();
        public List<UpcomingDeadlineVM> UpcomingDeadlines { get; set; } = new List<UpcomingDeadlineVM>();
        public List<RecentGradeVM> RecentGrades { get; set; } = new List<RecentGradeVM>();
        
        public int TotalEnrolled { get; set; }
        public int AssignmentsDue { get; set; }
        public int MessagesUnread { get; set; }
        public decimal AverageGrade { get; set; }
    }

    public class EnrolledCourseVM
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string Title { get; set; }
        public string LecturerName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string Status { get; set; }
        public string RejectionReason { get; set; }
        public string ThumbnailPath { get; set; }
        public int Credits { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class UpcomingDeadlineVM
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; }
        public string CourseTitle { get; set; }
        public DateTime Deadline { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsSubmitted { get; set; }
    }

    public class RecentGradeVM
    {
        public string AssignmentTitle { get; set; }
        public string CourseTitle { get; set; }
        public decimal Grade { get; set; }
        public int MaxMarks { get; set; }
        public DateTime GradedAt { get; set; }
        public decimal Percentage { get; set; }
        public string Feedback { get; set; }
    }
}
