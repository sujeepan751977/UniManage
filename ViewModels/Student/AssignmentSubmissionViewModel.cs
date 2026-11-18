using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace UniManage.ViewModels.Student
{
    public class AssignmentSubmissionViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int MaxMarks { get; set; }
        public string CourseTitle { get; set; }
        public bool IsLate { get; set; }

        [Required(ErrorMessage = "Please select a file to upload.")]
        public HttpPostedFileBase SubmissionFile { get; set; }

        public SubmissionDetailVM ExistingSubmission { get; set; }
    }

    public class SubmissionDetailVM
    {
        public int SubmissionId { get; set; }
        public string FileName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public decimal? Grade { get; set; }
        public string Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
        public string FilePath { get; set; }
    }
}
