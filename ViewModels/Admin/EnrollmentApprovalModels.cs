using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels.Admin
{
    public class PendingEnrollmentsViewModel
    {
        public List<PendingEnrollmentVM> Items { get; set; } = new List<PendingEnrollmentVM>();
        public int TotalPending { get; set; }
        public int TodayCount { get; set; }
        public int ThisWeekCount { get; set; }
    }

    public class PendingEnrollmentVM
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; }
        public string StudentFullName { get; set; }
        public string StudentEmail { get; set; }
        public DateTime StudentDOB { get; set; }
        
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string CourseLecturer { get; set; }
        public int CourseCredits { get; set; }
        public int CourseMaxEnrollment { get; set; }
        public int CurrentActiveCount { get; set; }
        
        public DateTime EnrolledAt { get; set; }
        public List<string> StudentCompletedCourses { get; set; } = new List<string>();
        public bool HasMetPrerequisites { get; set; }
        public List<string> MissingPrerequisites { get; set; } = new List<string>();
    }

    public class EnrollmentDetailViewModel
    {
        public PendingEnrollmentVM Enrollment { get; set; }
        public double AverageGrade { get; set; }
        public int ActiveEnrollmentsCount { get; set; }
        public List<string> GradeHistory { get; set; } = new List<string>();
    }
}
