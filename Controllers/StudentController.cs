using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Models;
using UniManage.ViewModels.Student;
using UniManage.Helpers;
using System.Data.Entity;
using System.IO;

namespace UniManage.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Student/Dashboard
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var enrollmentList = db.Enrollments
                .Where(e => e.StudentId == userId)
                .Include(e => e.Course)
                .Include(e => e.Course.Modules.Select(m => m.Lecturer))
                .ToList();

            // Get student's current batch IDs
            var studentBatchIds = db.BatchEnrollments
                .Where(be => be.Enrollment.StudentId == userId && be.Enrollment.Status == EnrollmentStatus.Active)
                .Select(be => be.BatchId)
                .ToList();

            var model = new StudentDashboardViewModel
            {
                TotalEnrolled = enrollmentList.Count(e => e.Status == EnrollmentStatus.Active),
                EnrolledCourses = enrollmentList.Select(e => new EnrolledCourseVM
                {
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    Title = e.Course.Title,
                    LecturerName = e.Course.Modules.FirstOrDefault()?.Lecturer?.FullName ?? "Staff",
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status.ToString(),
                    RejectionReason = e.RejectionReason,
                    ThumbnailPath = e.Course.ThumbnailPath
                }).ToList(),
                UpcomingDeadlines = db.Assignments
                    .Where(a => a.AssignmentReleases.Any(r => studentBatchIds.Contains(r.BatchId)))
                    .OrderBy(a => a.Deadline)
                    .Take(5)
                    .ToList()
                    .Select(a => {
                        var studentBatchId = db.BatchEnrollments.FirstOrDefault(be => be.Enrollment.StudentId == userId && be.Enrollment.CourseId == a.Module.CourseId)?.BatchId;
                        var customDeadline = a.AssignmentReleases.FirstOrDefault(r => r.BatchId == studentBatchId)?.CustomDeadline;
                        return new UpcomingDeadlineVM
                        {
                            AssignmentId = a.AssignmentId,
                            AssignmentTitle = a.Title,
                            CourseTitle = a.Module.Course.Title,
                            Deadline = customDeadline ?? a.Deadline,
                            DaysRemaining = ((customDeadline ?? a.Deadline) - DateTime.Now).Days,
                            IsSubmitted = db.Submissions.Any(s => s.AssignmentId == a.AssignmentId && s.StudentId == userId)
                        };
                    }).ToList()
            };

            return View(model);
        }

        // GET: Student/BrowseCourses
        public ActionResult BrowseCourses(string search, string filter)
        {
            var userId = User.Identity.GetUserId();
            var query = db.Courses.Where(c => c.Status == CourseStatus.Active);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search) || c.CourseCode.Contains(search));
            }

            var studentEnrollments = db.Enrollments.Where(e => e.StudentId == userId).Select(e => e.CourseId).ToList();

            var model = new BrowseCoursesViewModel
            {
                SearchQuery = search,
                FilterStatus = filter,
                Courses = query.ToList().Select(c => new CourseCardVM
                {
                    CourseId = c.CourseId,
                    CourseCode = c.CourseCode,
                    Title = c.Title,
                    Description = c.Description,
                    LecturerName = c.Modules.FirstOrDefault()?.Lecturer?.FullName ?? "Staff",
                    Credits = c.Credits,
                    MaxEnrollment = c.MaxEnrollment,
                    CurrentEnrollment = db.Enrollments.Count(e => e.CourseId == c.CourseId && e.Status == EnrollmentStatus.Active),
                    ThumbnailPath = c.ThumbnailPath,
                    IsEnrolled = studentEnrollments.Contains(c.CourseId),
                    IsPending = db.Enrollments.Any(e => e.StudentId == userId && e.CourseId == c.CourseId && e.Status == EnrollmentStatus.Pending),
                    Prerequisites = db.Prerequisites.Where(p => p.CourseId == c.CourseId).Select(p => p.PrerequisiteCourse.CourseCode).ToList()
                }).ToList()
            };

            return View(model);
        }

        // POST: Student/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enroll(int courseId)
        {
            var userId = User.Identity.GetUserId();
            var course = db.Courses.Include(c => c.Prerequisites).FirstOrDefault(c => c.CourseId == courseId);

            // 1. Course exists
            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction("BrowseCourses");
            }

            // 2. Course Active
            if (course.Status != CourseStatus.Active)
            {
                TempData["Error"] = "This course is not available.";
                return RedirectToAction("BrowseCourses");
            }

            // 3. Not already enrolled or pending
            var existingEnrollment = db.Enrollments.FirstOrDefault(e => e.StudentId == userId && e.CourseId == courseId);
            if (existingEnrollment != null)
            {
                if (existingEnrollment.Status == EnrollmentStatus.Active)
                {
                    TempData["Error"] = "You are already enrolled in this course.";
                    return RedirectToAction("BrowseCourses");
                }
                if (existingEnrollment.Status == EnrollmentStatus.Pending)
                {
                    TempData["Info"] = "Your enrollment request is already pending approval.";
                    return RedirectToAction("BrowseCourses");
                }
                // If rejected, they can try again (delete old or update - here we'll let them create a new one by deleting old if needed, or just proceed)
                if (existingEnrollment.Status == EnrollmentStatus.Rejected)
                {
                    db.Enrollments.Remove(existingEnrollment);
                }
            }

            // 4. Capacity
            int activeEnrollments = db.Enrollments.Count(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
            if (activeEnrollments >= course.MaxEnrollment)
            {
                TempData["Error"] = "This course is full. You cannot enroll at this time.";
                return RedirectToAction("BrowseCourses");
            }

            // 5. Prerequisites
            var prerequisites = db.Prerequisites.Where(p => p.CourseId == courseId).ToList();
            foreach (var pre in prerequisites)
            {
                bool completed = db.Enrollments.Any(e => e.StudentId == userId && e.CourseId == pre.PrerequisiteCourseId && e.Status == EnrollmentStatus.Completed);
                if (!completed)
                {
                    var preCourse = db.Courses.Find(pre.PrerequisiteCourseId);
                    TempData["Error"] = $"You must complete {preCourse.CourseCode}: {preCourse.Title} before enrolling.";
                    return RedirectToAction("BrowseCourses");
                }
            }

            // All checks pass
            var enrollment = new Enrollment
            {
                StudentId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Pending
            };
            db.Enrollments.Add(enrollment);
            db.SaveChanges();

            AuditHelper.Log(userId, "Enroll", "Course", course.CourseId.ToString(), $"Student requested enrollment in {course.Title}", Request);
            TempData["Info"] = $"Your enrollment request for {course.Title} has been submitted and is awaiting administrator approval.";

            return RedirectToAction("MyCourses");
        }

        // GET: Student/MyCourses
        public ActionResult MyCourses()
        {
            var userId = User.Identity.GetUserId();
            var model = db.Enrollments
                .Where(e => e.StudentId == userId)
                .Include(e => e.Course)
                .Include(e => e.Course.Modules.Select(m => m.Lecturer))
                .ToList()
                .Select(e => new EnrolledCourseVM
                {
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    Title = e.Course.Title,
                    LecturerName = e.Course.Modules.FirstOrDefault()?.Lecturer?.FullName ?? "Staff",
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status.ToString(),
                    RejectionReason = e.RejectionReason,
                    ThumbnailPath = e.Course.ThumbnailPath,
                    Credits = e.Course.Credits
                }).ToList();

            return View(model);
        }

        // GET: Student/CourseDetail/5
        public ActionResult CourseDetail(int id)
        {
            var userId = User.Identity.GetUserId();
            var enrollment = db.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Course.Modules)
                .FirstOrDefault(e => e.CourseId == id && e.StudentId == userId && e.Status == EnrollmentStatus.Active);

            if (enrollment == null) return HttpNotFound();

            // Get student's batch ID for this course
            var studentBatchId = db.BatchEnrollments
                .Where(be => be.Enrollment.StudentId == userId && be.Enrollment.CourseId == id)
                .Select(be => be.BatchId)
                .FirstOrDefault();

            if (studentBatchId == 0) return RedirectToAction("MyCourses"); // Not yet assigned to a batch

            var studentBatch = db.Batches.Find(studentBatchId);
            ViewBag.StudentBatchId = studentBatchId;
            ViewBag.StudentBatchCode = studentBatch?.BatchCode;

            // Load released materials
            ViewBag.ReleasedMaterials = db.MaterialReleases
                .Where(mr => mr.BatchId == studentBatchId)
                .Select(mr => mr.Material)
                .Include(m => m.Module)
                .ToList();

            var releasedAssignments = db.AssignmentReleases
                .Where(ar => ar.BatchId == studentBatchId)
                .Include(ar => ar.Assignment)
                .Include(ar => ar.Assignment.Module)
                .ToList()
                .Select(ar => new AssignmentReleaseVM 
                {
                    Assignment = ar.Assignment,
                    CustomDeadline = ar.CustomDeadline
                })
                .ToList();

            ViewBag.ReleasedAssignments = releasedAssignments;

            ViewBag.SubmittedAssignmentIds = db.Submissions
                .Where(s => s.StudentId == userId)
                .Select(s => s.AssignmentId)
                .ToList();
            
            return View(enrollment.Course);
        }

        // GET: Student/SubmitAssignment/5
        public ActionResult SubmitAssignment(int id)
        {
            var userId = User.Identity.GetUserId();
            var assignment = db.Assignments.Include(a => a.Module.Course).FirstOrDefault(a => a.AssignmentId == id);
            if (assignment == null) return HttpNotFound();

            // Verify the assignment is released to the student's batch
            var studentBatchId = db.BatchEnrollments
                .Where(be => be.Enrollment.StudentId == userId && be.Enrollment.CourseId == assignment.Module.CourseId)
                .Select(be => be.BatchId)
                .FirstOrDefault();

            var release = db.AssignmentReleases.FirstOrDefault(r => r.AssignmentId == id && r.BatchId == studentBatchId);
            if (release == null) return HttpNotFound(); // Not released to this student

            var deadline = release.CustomDeadline ?? assignment.Deadline;

            var existing = db.Submissions.FirstOrDefault(s => s.AssignmentId == id && s.StudentId == userId);

            var model = new AssignmentSubmissionViewModel
            {
                AssignmentId = assignment.AssignmentId,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = deadline,
                MaxMarks = assignment.MaxMarks,
                CourseTitle = assignment.Module.Course.Title,
                IsLate = DateTime.Now > deadline,
                ExistingSubmission = existing == null ? null : new SubmissionDetailVM
                {
                    SubmissionId = existing.SubmissionId,
                    FileName = existing.FileName,
                    SubmittedAt = existing.SubmittedAt,
                    IsLate = existing.IsLate,
                    Grade = existing.Grade,
                    Feedback = existing.Feedback,
                    GradedAt = existing.GradedAt,
                    FilePath = existing.FilePath
                }
            };

            return View(model);
        }

        // POST: Student/SubmitAssignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAssignment(AssignmentSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var assignment = db.Assignments.Find(model.AssignmentId);
                
                if (DateTime.Now > assignment.Deadline && !assignment.AllowLateSubmission)
                {
                    TempData["Error"] = "Late submissions are not allowed for this assignment.";
                    return RedirectToAction("SubmitAssignment", new { id = model.AssignmentId });
                }

                // Check for existing submission to update or replace
                var existing = db.Submissions.FirstOrDefault(s => s.AssignmentId == model.AssignmentId && s.StudentId == userId);
                if (existing != null)
                {
                    FileHelper.DeleteFile(existing.FilePath);
                    db.Submissions.Remove(existing);
                }

                try
                {
                    string filePath = FileHelper.SaveFile(model.SubmissionFile, "assignments");
                    
                    var submission = new Submission
                    {
                        AssignmentId = model.AssignmentId,
                        StudentId = userId,
                        FilePath = filePath,
                        FileName = model.SubmissionFile.FileName,
                        FileSize = model.SubmissionFile.ContentLength,
                        SubmittedAt = DateTime.Now,
                        IsLate = DateTime.Now > assignment.Deadline
                    };

                    db.Submissions.Add(submission);
                    db.SaveChanges();

                    AuditHelper.Log(userId, "SubmitAssignment", "Assignment", assignment.AssignmentId.ToString(), $"Student submitted assignment: {assignment.Title}", Request);
                    TempData["Success"] = "Assignment submitted successfully!";
                    return RedirectToAction("CourseDetail", new { id = assignment.Module.CourseId });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Upload failed: " + ex.Message;
                    return RedirectToAction("SubmitAssignment", new { id = model.AssignmentId });
                }

                AuditHelper.Log(userId, "SubmitAssignment", "Assignment", assignment.AssignmentId.ToString(), $"Student submitted assignment: {assignment.Title}", Request);
                TempData["Success"] = "Assignment submitted successfully!";
                return RedirectToAction("CourseDetail", new { id = assignment.Module.CourseId });
            }
            return View(model);
        }

        // GET: Student/MyGrades
        public ActionResult MyGrades()
        {
            var userId = User.Identity.GetUserId();
            var activeCourseIds = db.Enrollments.Where(e => e.StudentId == userId && e.Status == EnrollmentStatus.Active).Select(e => e.CourseId).ToList();

            var submissions = db.Submissions
                .Where(s => s.StudentId == userId && s.Grade.HasValue && activeCourseIds.Contains(s.Assignment.Module.CourseId))
                .Include(s => s.Assignment)
                .Include(s => s.Assignment.Module.Course)
                .OrderByDescending(s => s.GradedAt)
                .ToList();

            return View(submissions);
        }

        // GET: Student/Assignments
        public ActionResult Assignments()
        {
            var userId = User.Identity.GetUserId();
            var studentBatchIds = db.BatchEnrollments
                .Where(be => be.Enrollment.StudentId == userId && be.Enrollment.Status == EnrollmentStatus.Active)
                .Select(be => be.BatchId)
                .ToList();

            var assignments = db.AssignmentReleases
                .Where(ar => studentBatchIds.Contains(ar.BatchId))
                .Include(ar => ar.Assignment.Module.Course)
                .ToList()
                .OrderBy(ar => ar.CustomDeadline ?? ar.Assignment.Deadline)
                .Select(ar => new AssignmentReleaseVM 
                {
                    Assignment = ar.Assignment,
                    CustomDeadline = ar.CustomDeadline
                })
                .ToList();

            ViewBag.Submissions = db.Submissions.Where(s => s.StudentId == userId).ToList();
            return View(assignments);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
