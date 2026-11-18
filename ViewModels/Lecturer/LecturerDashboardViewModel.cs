using System.Collections.Generic;

namespace UniManage.ViewModels.Lecturer
{
    public class LecturerDashboardViewModel
    {
        public List<TeachingModuleVM> TeachingModules { get; set; } = new List<TeachingModuleVM>();
        public int TotalStudents { get; set; }
        public int PendingGrading { get; set; }
        public int UnreadMessages { get; set; }
        public List<RecentSubmissionVM> RecentSubmissions { get; set; } = new List<RecentSubmissionVM>();
    }

    public class TeachingModuleVM
    {
        public int ModuleId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string ModuleCode { get; set; }
        public string Title { get; set; }
        public int EnrolledCount { get; set; }
        public int AssignmentCount { get; set; }
        public int MaterialCount { get; set; }
        public int BatchCount { get; set; }
        public List<string> BatchCodes { get; set; } = new List<string>();
    }

    public class RecentSubmissionVM
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; }
        public string AssignmentTitle { get; set; }
        public string CourseCode { get; set; }
        public System.DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }
    }

    public class LecturerReportsViewModel
    {
        public List<ModulePerformanceVM> ModulePerformances { get; set; } = new List<ModulePerformanceVM>();
        public int TotalEnrolledStudents { get; set; }
        public double AverageClassGrade { get; set; }
        public int CompletedAssignments { get; set; }
        public List<TopStudentVM> TopStudents { get; set; } = new List<TopStudentVM>();
        public List<int> GradeDistribution { get; set; } = new List<int>(); // Counts for: 0-20, 21-40, 41-60, 61-80, 81-100
    }

    public class TopStudentVM
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public double AverageGrade { get; set; }
        public int SubmissionsCount { get; set; }
    }

    public class ModulePerformanceVM
    {
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; }
        public string Title { get; set; }
        public string CourseCode { get; set; }
        public int EnrollmentCount { get; set; }
        public double AverageGrade { get; set; }
        public int SubmissionCount { get; set; }
        public int PendingGradingCount { get; set; }
    }
}
