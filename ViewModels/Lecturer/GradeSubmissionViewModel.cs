using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels.Lecturer
{
    public class GradeSubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string AssignmentTitle { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool IsLate { get; set; }
        public int MaxMarks { get; set; }

        [Required(ErrorMessage = "Please provide a grade.")]
        [Range(0, 100, ErrorMessage = "Grade must be between 0 and 100.")]
        public decimal Grade { get; set; }

        [MaxLength(1000)]
        public string Feedback { get; set; }
    }
}
