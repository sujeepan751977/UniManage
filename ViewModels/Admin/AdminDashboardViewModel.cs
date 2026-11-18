using System.Collections.Generic;

namespace UniManage.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveEnrollments { get; set; }
        
        public List<RecentAuditVM> RecentAudits { get; set; } = new List<RecentAuditVM>();
        public List<GrowthStatVM> EnrollmentGrowth { get; set; } = new List<GrowthStatVM>();
    }

    public class RecentAuditVM
    {
        public string Action { get; set; }
        public string User { get; set; }
        public string Details { get; set; }
        public System.DateTime Timestamp { get; set; }
    }

    public class GrowthStatVM
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }
}
