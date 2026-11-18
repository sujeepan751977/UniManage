using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace UniManage.Models
{
    // --- Admin ViewModels ---

    public class BatchListViewModel
    {
        public List<BatchVM> Batches { get; set; } = new List<BatchVM>();
        public List<SelectListItem> Courses { get; set; } = new List<SelectListItem>();
        public int? SelectedCourseId { get; set; }
        public string SearchTerm { get; set; }
        public BatchStatus? SelectedStatus { get; set; }
    }

    public class BatchVM
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxCapacity { get; set; }
        public int EnrolledCount { get; set; }
        public BatchStatus Status { get; set; }
        public string LecturerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public double DurationInWeeks => (EndDate - StartDate).TotalDays / 7.0;
        public int DaysUntilStart => (StartDate - DateTime.Now).Days;
    }

    public class CreateBatchViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Batch Name")]
        public string BatchName { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "Batch Code")]
        public string BatchCode { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Required]
        [Range(1, 200)]
        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; }
        [Required]
        [Display(Name = "Assigned Lecturer")]
        public string LecturerId { get; set; }
    }

    public class EditBatchViewModel : CreateBatchViewModel
    {
        public int BatchId { get; set; }
        public BatchStatus Status { get; set; }
    }

    public class BatchDetailViewModel
    {
        public BatchVM Batch { get; set; }
        public List<BatchStudentVM> AssignedStudents { get; set; } = new List<BatchStudentVM>();
        public List<BatchStudentVM> UnassignedStudents { get; set; } = new List<BatchStudentVM>();
        public int RemainingCapacity => Batch.MaxCapacity - Batch.EnrolledCount;
    }

    public class BatchStudentVM
    {
        public int EnrollmentId { get; set; }
        public int? BatchEnrollmentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public DateTime EnrolledDate { get; set; }
        public decimal? AverageGrade { get; set; }
    }

    public class BatchAnalyticsVM
    {
        public string BatchName { get; set; }
        public string CourseTitle { get; set; }
        public int StudentCount { get; set; }
        public int Capacity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public double? AverageGrade { get; set; }
        public double FillRate => (double)StudentCount / Capacity * 100;
        public double ProgressPercent
        {
            get
            {
                if (DateTime.Now < StartDate) return 0;
                if (DateTime.Now > EndDate) return 100;
                var total = (EndDate - StartDate).TotalDays;
                var elapsed = (DateTime.Now - StartDate).TotalDays;
                return (elapsed / total) * 100;
            }
        }
    }

    public class BatchLecturerViewModel
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string CourseTitle { get; set; }
        public List<AssignedLecturerVM> CurrentLecturers { get; set; } = new List<AssignedLecturerVM>();
        public List<SelectListItem> AvailableLecturers { get; set; } = new List<SelectListItem>();
    }

    public class AssignedLecturerVM
    {
        public int BatchLecturerId { get; set; }
        public string LecturerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsPrimary { get; set; }
        public string Initials { get; set; }
    }

    // --- Lecturer ViewModels ---

    public class ReleaseAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int MaxMarks { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public List<BatchReleaseOptionVM> BatchOptions { get; set; } = new List<BatchReleaseOptionVM>();
        
        // For POST
        public List<int> SelectedBatchIds { get; set; } = new List<int>();
        // Using a list of custom objects for deadlines as Dictionary is harder to bind in MVC 5 sometimes
        public List<BatchDeadlineInput> CustomDeadlines { get; set; } = new List<BatchDeadlineInput>();
    }

    public class BatchDeadlineInput
    {
        public int BatchId { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class ReleaseMaterialViewModel
    {
        public int MaterialId { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public List<BatchReleaseOptionVM> BatchOptions { get; set; } = new List<BatchReleaseOptionVM>();
        public List<int> SelectedBatchIds { get; set; } = new List<int>();
    }

    public class BatchReleaseOptionVM
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public int EnrolledCount { get; set; }
        public bool IsAlreadyReleased { get; set; }
        public DateTime? ExistingCustomDeadline { get; set; }
        public bool IsCurrentLecturerBatch { get; set; }
    }

    public class ManageReleasesViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string CourseCode { get; set; }
        public List<AssignmentReleaseStatusVM> Assignments { get; set; } = new List<AssignmentReleaseStatusVM>();
        public List<MaterialReleaseStatusVM> Materials { get; set; } = new List<MaterialReleaseStatusVM>();
        public List<Batch> Batches { get; set; } = new List<Batch>();
    }

    public class AssignmentReleaseStatusVM
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public DateTime Deadline { get; set; }
        public List<BatchReleaseInfo> ReleaseInfo { get; set; } = new List<BatchReleaseInfo>();
    }

    public class MaterialReleaseStatusVM
    {
        public int MaterialId { get; set; }
        public string Title { get; set; }
        public List<BatchReleaseInfo> ReleaseInfo { get; set; } = new List<BatchReleaseInfo>();
    }

    public class BatchReleaseInfo
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; }
        public bool IsReleased { get; set; }
        public DateTime? CustomDeadline { get; set; }
    }

    // --- Student ViewModels ---

    public class AssignmentReleaseVM
    {
        public Assignment Assignment { get; set; }
        public DateTime? CustomDeadline { get; set; }
    }

    public class StudentSearchVM
    {
        public string StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string BatchCode { get; set; }
        public string CourseTitle { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
