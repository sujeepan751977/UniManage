using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using UniManage.Models;
using UniManage.ViewModels.Lecturer;
using UniManage.Helpers;
using System.Data.Entity;
using System.IO;
using System.Threading.Tasks;

namespace UniManage.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Lecturer/Dashboard
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            
            // Get modules where the lecturer is assigned to at least one batch
            var moduleIds = db.BatchLecturers
                .Where(bl => bl.LecturerId == userId)
                .Select(bl => bl.Batch.CourseId)
                .Distinct()
                .Join(db.Modules, 
                      courseId => courseId, 
                      m => m.CourseId, 
                      (courseId, m) => m.ModuleId)
                .ToList();

            var modules = db.Modules
                .Include(m => m.Course)
                .Where(m => moduleIds.Contains(m.ModuleId) || m.LecturerId == userId)
                .ToList();
            
            var model = new LecturerDashboardViewModel
            {
                TeachingModules = modules.Select(m => {
                    var batches = db.BatchLecturers.Where(bl => bl.Batch.CourseId == m.CourseId && bl.LecturerId == userId).Select(bl => bl.Batch).ToList();
                    return new TeachingModuleVM
                    {
                        ModuleId = m.ModuleId,
                        CourseId = m.CourseId,
                        CourseCode = m.Course.CourseCode,
                        ModuleCode = m.ModuleCode,
                        Title = m.Title,
                        EnrolledCount = db.Enrollments.Count(e => e.CourseId == m.CourseId && e.Status == EnrollmentStatus.Active),
                        AssignmentCount = db.Assignments.Count(a => a.ModuleId == m.ModuleId),
                        MaterialCount = db.Materials.Count(mat => mat.ModuleId == m.ModuleId),
                        BatchCount = batches.Count,
                        BatchCodes = batches.Select(b => b.BatchCode).ToList()
                    };
                }).ToList(),
                TotalStudents = db.Enrollments.Count(e => db.BatchLecturers.Any(bl => bl.Batch.CourseId == e.CourseId && bl.LecturerId == userId) && e.Status == EnrollmentStatus.Active),
                PendingGrading = db.Submissions.Count(s => s.Assignment.Module.LecturerId == userId && !s.Grade.HasValue),
                RecentSubmissions = db.Submissions
                    .Where(s => (s.Assignment.Module.LecturerId == userId || s.Assignment.AssignmentReleases.Any(ar => ar.Batch.BatchLecturers.Any(bl => bl.LecturerId == userId))) && !s.Grade.HasValue)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(5)
                    .ToList()
                    .Select(s => new RecentSubmissionVM
                    {
                        SubmissionId = s.SubmissionId,
                        StudentName = s.Student.FullName,
                        AssignmentTitle = s.Assignment.Title,
                        CourseCode = s.Assignment.Module.Course.CourseCode,
                        SubmittedAt = s.SubmittedAt,
                        IsLate = s.IsLate
                    }).ToList()
            };

            return View(model);
        }

        // GET: Lecturer/MyCourses (Now shows assigned Modules)
        public ActionResult MyCourses()
        {
            var userId = User.Identity.GetUserId();
            var model = db.Modules
                .Where(m => m.LecturerId == userId)
                .Include(m => m.Course)
                .Include(m => m.Assignments)
                .Include(m => m.Materials)
                .ToList();
            return View(model);
        }

        // GET: Lecturer/CourseStudents/5 (CourseId)
        public ActionResult CourseStudents(int id, int? batchId)
        {
            var userId = User.Identity.GetUserId();
            // Verify lecturer teaches in this course
            var teachesInCourse = db.Modules.Any(m => m.CourseId == id && m.LecturerId == userId) || 
                                 db.BatchLecturers.Any(bl => bl.Batch.CourseId == id && bl.LecturerId == userId);
            if (!teachesInCourse) return HttpNotFound();

            var course = db.Courses.Find(id);
            var query = db.Enrollments.Where(e => e.CourseId == id);
            
            if (batchId.HasValue)
            {
                query = query.Where(e => e.BatchEnrollments.Any(be => be.BatchId == batchId));
            }

            var students = query
                .Include(e => e.Student)
                .Include("BatchEnrollments.Batch")
                .ToList();

            ViewBag.Course = course;
            
            // Get batches for filter
            ViewBag.Batches = db.BatchLecturers
                .Where(bl => bl.Batch.CourseId == id && bl.LecturerId == userId)
                .Select(bl => bl.Batch)
                .ToList();
            ViewBag.SelectedBatchId = batchId;

            return View(students);
        }

        // GET: Lecturer/Materials/5 (ModuleId)
        public ActionResult Materials(int id)
        {
            var userId = User.Identity.GetUserId();
            var module = db.Modules.Include(m => m.Course).FirstOrDefault(m => m.ModuleId == id && (m.LecturerId == userId || db.BatchLecturers.Any(bl => bl.Batch.CourseId == m.CourseId && bl.LecturerId == userId)));
            if (module == null) return HttpNotFound();

            var materials = db.Materials.Where(m => m.ModuleId == id).ToList();
            ViewBag.Module = module;
            
            return View(materials);
        }

        public ActionResult UploadMaterial(int moduleId, string title, string description, HttpPostedFileBase materialFile)
        {
            if (materialFile != null && materialFile.ContentLength > 0)
            {
                try
                {
                    string filePath = FileHelper.SaveFile(materialFile, "materials");
                    var material = new CourseMaterial
                    {
                        ModuleId = moduleId,
                        Title = title,
                        Description = description,
                        FilePath = filePath,
                        FileName = materialFile.FileName,
                        FileSize = materialFile.ContentLength,
                        FileType = Path.GetExtension(materialFile.FileName).Replace(".", ""),
                        UploadedById = User.Identity.GetUserId(),
                        UploadedAt = DateTime.Now
                    };
                    
                    db.Materials.Add(material);
                    db.SaveChanges();
                    TempData["Success"] = "Material uploaded! Now release it to your batches.";
                    int courseId = db.Modules.Where(m => m.ModuleId == moduleId).Select(m => m.CourseId).FirstOrDefault();
                    return RedirectToAction("ManageReleases", new { id = courseId });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }
            return RedirectToAction("Materials", new { id = moduleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMaterial(int id)
        {
            var userId = User.Identity.GetUserId();
            var material = db.Materials.Include(m => m.Module).FirstOrDefault(m => m.MaterialId == id && m.Module.LecturerId == userId);
            
            if (material != null)
            {
                int moduleId = material.ModuleId.GetValueOrDefault();
                FileHelper.DeleteFile(material.FilePath);
                db.Materials.Remove(material);
                db.SaveChanges();
                TempData["Success"] = "Material deleted successfully!";
                return RedirectToAction("Materials", new { id = moduleId });
            }
            return HttpNotFound();
        }

        // GET: Lecturer/Assignments/5 (ModuleId)
        public ActionResult Assignments(int? id)
        {
            var userId = User.Identity.GetUserId();
            
            if (id == null)
            {
                var allAssignments = db.Assignments
                    .Where(a => a.Module.LecturerId == userId || a.AssignmentReleases.Any(r => r.Batch.BatchLecturers.Any(bl => bl.LecturerId == userId)))
                    .Include(a => a.Module.Course)
                    .OrderByDescending(a => a.Deadline)
                    .ToList();
                return View("AllAssignments", allAssignments);
            }
            
            var module = db.Modules.Include(m => m.Course).FirstOrDefault(m => m.ModuleId == id && (m.LecturerId == userId || db.BatchLecturers.Any(bl => bl.Batch.CourseId == m.CourseId && bl.LecturerId == userId)));
            if (module == null) return HttpNotFound();

            var assignments = db.Assignments.Where(a => a.ModuleId == id).ToList();
            ViewBag.Module = module;
            
            return View(assignments);
        }

        public ActionResult CreateAssignment(Assignment model)
        {
            model.CreatedById = User.Identity.GetUserId();
            model.CreatedAt = DateTime.Now;
            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                db.Assignments.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Assignment created! Now release it to your batches.";
                int courseId = db.Modules.Where(m => m.ModuleId == model.ModuleId).Select(m => m.CourseId).FirstOrDefault();
                return RedirectToAction("ManageReleases", new { id = courseId });
            }
            
            TempData["Error"] = "Assignment creation failed. Please check all fields.";
            return RedirectToAction("Assignments", new { id = model.ModuleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAssignment(int id)
        {
            var userId = User.Identity.GetUserId();
            var assignment = db.Assignments.Include(a => a.Module).FirstOrDefault(a => a.AssignmentId == id && a.Module.LecturerId == userId);

            if (assignment != null)
            {
                int moduleId = assignment.ModuleId.GetValueOrDefault();
                
                var submissions = db.Submissions.Where(s => s.AssignmentId == id).ToList();
                foreach (var s in submissions)
                {
                    FileHelper.DeleteFile(s.FilePath);
                    db.Submissions.Remove(s);
                }

                db.Assignments.Remove(assignment);
                db.SaveChanges();
                TempData["Success"] = "Assignment deleted!";
                return RedirectToAction("Assignments", new { id = moduleId });
            }
            return HttpNotFound();
        }

        // GET: Lecturer/Submissions/5 (AssignmentId)
        public ActionResult Submissions(int id)
        {
            var userId = User.Identity.GetUserId();
            var assignment = db.Assignments.Include(a => a.Module.Course).FirstOrDefault(a => a.AssignmentId == id && a.Module.LecturerId == userId);
            if (assignment == null) return HttpNotFound();

            var submissions = db.Submissions.Where(s => s.AssignmentId == id).Include(s => s.Student).ToList();
            ViewBag.Assignment = assignment;
            return View(submissions);
        }

        // GET: Lecturer/GradeSubmission/5
        public ActionResult GradeSubmission(int id)
        {
            var userId = User.Identity.GetUserId();
            var submission = db.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Assignment.Module.Course)
                .Include(s => s.Student)
                .FirstOrDefault(s => s.SubmissionId == id && s.Assignment.Module.LecturerId == userId);

            if (submission == null) return HttpNotFound();

            var model = new GradeSubmissionViewModel
            {
                SubmissionId = submission.SubmissionId,
                StudentName = submission.Student.FullName,
                StudentEmail = submission.Student.Email,
                AssignmentTitle = submission.Assignment.Title,
                SubmittedAt = submission.SubmittedAt,
                FileName = submission.FileName,
                FilePath = submission.FilePath,
                IsLate = submission.IsLate,
                MaxMarks = submission.Assignment.MaxMarks,
                Grade = submission.Grade ?? 0,
                Feedback = submission.Feedback
            };

            return View(model);
        }

        // POST: Lecturer/GradeSubmission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GradeSubmission(GradeSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var submission = db.Submissions.Find(model.SubmissionId);
                submission.Grade = model.Grade;
                submission.Feedback = model.Feedback;
                submission.GradedAt = DateTime.Now;
                submission.GradedById = User.Identity.GetUserId();

                db.SaveChanges();
                TempData["Success"] = "Grade submitted successfully!";
                return RedirectToAction("Submissions", new { id = submission.AssignmentId });
            }
            return View(model);
        }

        // GET: Lecturer/Reports
        public ActionResult Reports()
        {
            var userId = User.Identity.GetUserId();
            var modules = db.Modules
                .Where(m => m.LecturerId == userId)
                .Include(m => m.Course)
                .ToList();

            var model = new LecturerReportsViewModel
            {
                ModulePerformances = modules.Select(m => new ModulePerformanceVM
                {
                    ModuleId = m.ModuleId,
                    ModuleCode = m.ModuleCode,
                    Title = m.Title,
                    CourseCode = m.Course.CourseCode,
                    EnrollmentCount = db.Enrollments.Count(e => e.CourseId == m.CourseId && e.Status == EnrollmentStatus.Active),
                    AverageGrade = db.Submissions.Where(s => s.Assignment.ModuleId == m.ModuleId && s.Grade.HasValue).Average(s => (double?)s.Grade) ?? 0,
                    SubmissionCount = db.Submissions.Count(s => s.Assignment.ModuleId == m.ModuleId),
                    PendingGradingCount = db.Submissions.Count(s => s.Assignment.ModuleId == m.ModuleId && !s.Grade.HasValue)
                }).ToList(),
                TotalEnrolledStudents = db.Enrollments.Count(e => db.BatchLecturers.Any(bl => bl.Batch.CourseId == e.CourseId && bl.LecturerId == userId) && e.Status == EnrollmentStatus.Active),
                CompletedAssignments = db.Submissions.Count(s => s.Assignment.Module.LecturerId == userId && s.Grade.HasValue)
            };

            if (model.ModulePerformances.Any(mp => mp.AverageGrade > 0))
            {
                model.AverageClassGrade = model.ModulePerformances.Where(mp => mp.AverageGrade > 0).Average(mp => mp.AverageGrade);
            }

            // Calculate Grade Distribution
            var allGrades = db.Submissions
                .Where(s => s.Assignment.Module.LecturerId == userId && s.Grade.HasValue)
                .Select(s => s.Grade.Value)
                .ToList();

            model.GradeDistribution = new List<int>
            {
                allGrades.Count(g => g <= 20),
                allGrades.Count(g => g > 20 && g <= 40),
                allGrades.Count(g => g > 40 && g <= 60),
                allGrades.Count(g => g > 60 && g <= 80),
                allGrades.Count(g => g > 80)
            };

            // Calculate Top Performers
            model.TopStudents = db.Submissions
                .Where(s => s.Assignment.Module.LecturerId == userId && s.Grade.HasValue)
                .GroupBy(s => s.StudentId)
                .Select(g => new TopStudentVM
                {
                    StudentId = g.Key,
                    Name = g.FirstOrDefault().Student.FirstName + " " + g.FirstOrDefault().Student.LastName,
                    Email = g.FirstOrDefault().Student.Email,
                    AverageGrade = g.Average(s => (double)s.Grade.Value),
                    SubmissionsCount = g.Count()
                })
                .OrderByDescending(s => s.AverageGrade)
                .Take(5)
                .ToList();

            return View(model);
        }

        // GET: Lecturer/DownloadModuleReport/5
        public ActionResult DownloadModuleReport(int id)
        {
            var userId = User.Identity.GetUserId();
            var module = db.Modules.Include(m => m.Course).FirstOrDefault(m => m.ModuleId == id && (m.LecturerId == userId || db.BatchLecturers.Any(bl => bl.Batch.CourseId == m.CourseId && bl.LecturerId == userId)));
            if (module == null) return HttpNotFound();

            var submissions = db.Submissions
                .Where(s => s.Assignment.ModuleId == id)
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .OrderBy(s => s.Student.FirstName)
                .ToList();

            var sw = new StringWriter();
            sw.WriteLine("Student Name,Email,Assignment,Grade,Max Marks,Submitted At,Status");

            foreach (var s in submissions)
            {
                sw.WriteLine($"{s.Student.FirstName} {s.Student.LastName},{s.Student.Email},{s.Assignment.Title},{s.Grade?.ToString() ?? "N/A"},{s.Assignment.MaxMarks},{s.SubmittedAt:yyyy-MM-dd HH:mm},{(s.IsLate ? "Late" : "On Time")}");
            }

            var res = sw.ToString();
            var bytes = System.Text.Encoding.UTF8.GetBytes(res);
            return File(bytes, "text/csv", $"Performance_Report_{module.ModuleCode}_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ==========================================
        // FEATURE: CONTENT RELEASE MANAGEMENT
        // ==========================================

        public async Task<ActionResult> ManageReleases(int? id)
        {
            if (id == null)
            {
                TempData["Info"] = "Please select a module first to manage its content releases.";
                return RedirectToAction("MyCourses");
            }

            var userId = User.Identity.GetUserId();
            var course = await db.Courses
                .Include(c => c.Modules.Select(m => m.Assignments.Select(a => a.AssignmentReleases)))
                .Include(c => c.Modules.Select(m => m.Materials.Select(mat => mat.MaterialReleases)))
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null) return HttpNotFound();

            // Verify lecturer is assigned to at least one batch in this course
            var userBatches = await db.BatchLecturers
                .Where(bl => bl.Batch.CourseId == id && bl.LecturerId == userId)
                .Select(bl => bl.Batch)
                .ToListAsync();

            if (!userBatches.Any()) return new HttpStatusCodeResult(403, "Not assigned to any batch in this course");

            var assignments = course.Modules.SelectMany(m => m.Assignments).ToList();
            var materials = course.Modules.SelectMany(m => m.Materials).ToList();

            var model = new ManageReleasesViewModel
            {
                CourseId = course.CourseId,
                CourseTitle = course.Title,
                CourseCode = course.CourseCode,
                Batches = userBatches,
                Assignments = assignments.Select(a => new AssignmentReleaseStatusVM
                {
                    AssignmentId = a.AssignmentId,
                    Title = a.Title,
                    Deadline = a.Deadline,
                    ReleaseInfo = userBatches.Select(b => {
                        var rel = a.AssignmentReleases.FirstOrDefault(r => r.BatchId == b.BatchId);
                        return new BatchReleaseInfo { 
                            BatchId = b.BatchId, 
                            BatchCode = b.BatchCode, 
                            IsReleased = rel != null,
                            CustomDeadline = rel?.CustomDeadline
                        };
                    }).ToList()
                }).ToList(),
                Materials = materials.Select(mat => new MaterialReleaseStatusVM
                {
                    MaterialId = mat.MaterialId,
                    Title = mat.Title,
                    ReleaseInfo = userBatches.Select(b => new BatchReleaseInfo {
                        BatchId = b.BatchId,
                        BatchCode = b.BatchCode,
                        IsReleased = mat.MaterialReleases.Any(r => r.BatchId == b.BatchId)
                    }).ToList()
                }).ToList()
            };

            return View(model);
        }

        public async Task<ActionResult> ReleaseAssignment(int id)
        {
            var userId = User.Identity.GetUserId();
            var assignment = await db.Assignments.Include(a => a.Module.Course).FirstOrDefaultAsync(a => a.AssignmentId == id);
            if (assignment == null) return HttpNotFound();

            var userBatches = await db.BatchLecturers
                .Where(bl => bl.Batch.CourseId == assignment.Module.CourseId && bl.LecturerId == userId)
                .Select(bl => bl.Batch)
                .ToListAsync();

            var model = new ReleaseAssignmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                Title = assignment.Title,
                Deadline = assignment.Deadline,
                CourseTitle = assignment.Module.Course.Title,
                BatchOptions = userBatches.Select(b => new BatchReleaseOptionVM
                {
                    BatchId = b.BatchId,
                    BatchCode = b.BatchCode,
                    IsAlreadyReleased = db.AssignmentReleases.Any(ar => ar.AssignmentId == id && ar.BatchId == b.BatchId),
                    ExistingCustomDeadline = db.AssignmentReleases.FirstOrDefault(ar => ar.AssignmentId == id && ar.BatchId == b.BatchId)?.CustomDeadline
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReleaseAssignment(ReleaseAssignmentViewModel model)
        {
            if (model.SelectedBatchIds == null) return RedirectToAction("ManageReleases", new { id = model.AssignmentId }); // Simplified

            var assignment = await db.Assignments.FindAsync(model.AssignmentId);
            foreach (var batchId in model.SelectedBatchIds)
            {
                if (!db.AssignmentReleases.Any(r => r.AssignmentId == model.AssignmentId && r.BatchId == batchId))
                {
                    var deadline = model.CustomDeadlines?.FirstOrDefault(d => d.BatchId == batchId)?.Deadline;
                    db.AssignmentReleases.Add(new AssignmentRelease
                    {
                        AssignmentId = model.AssignmentId,
                        BatchId = batchId,
                        CustomDeadline = deadline,
                        ReleasedAt = DateTime.UtcNow,
                        ReleasedById = User.Identity.GetUserId()
                    });
                }
            }
            await db.SaveChangesAsync();
            return RedirectToAction("ManageReleases", new { id = assignment.Module.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnreleaseAssignment(int assignmentId, int batchId)
        {
            var release = await db.AssignmentReleases.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.BatchId == batchId);
            if (release != null)
            {
                db.AssignmentReleases.Remove(release);
                await db.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        public async Task<ActionResult> ReleaseMaterial(int id)
        {
            var userId = User.Identity.GetUserId();
            var material = await db.Materials.Include(m => m.Module.Course).FirstOrDefaultAsync(m => m.MaterialId == id);
            if (material == null) return HttpNotFound();

            var userBatches = await db.BatchLecturers
                .Where(bl => bl.Batch.CourseId == material.Module.CourseId && bl.LecturerId == userId)
                .Select(bl => bl.Batch)
                .ToListAsync();

            var model = new ReleaseMaterialViewModel
            {
                MaterialId = material.MaterialId,
                Title = material.Title,
                CourseTitle = material.Module.Course.Title,
                BatchOptions = userBatches.Select(b => new BatchReleaseOptionVM
                {
                    BatchId = b.BatchId,
                    BatchCode = b.BatchCode,
                    IsAlreadyReleased = db.MaterialReleases.Any(mr => mr.MaterialId == id && mr.BatchId == b.BatchId)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReleaseMaterial(ReleaseMaterialViewModel model)
        {
            if (model.SelectedBatchIds == null) return RedirectToAction("ManageReleases", new { id = model.MaterialId });

            var material = await db.Materials.FindAsync(model.MaterialId);
            foreach (var batchId in model.SelectedBatchIds)
            {
                if (!db.MaterialReleases.Any(r => r.MaterialId == model.MaterialId && r.BatchId == batchId))
                {
                    db.MaterialReleases.Add(new MaterialRelease
                    {
                        MaterialId = model.MaterialId,
                        BatchId = batchId,
                        ReleasedAt = DateTime.UtcNow,
                        ReleasedById = User.Identity.GetUserId()
                    });
                }
            }
            await db.SaveChangesAsync();
            return RedirectToAction("ManageReleases", new { id = material.Module.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnreleaseMaterial(int materialId, int batchId)
        {
            var release = await db.MaterialReleases.FirstOrDefaultAsync(r => r.MaterialId == materialId && r.BatchId == batchId);
            if (release != null)
            {
                db.MaterialReleases.Remove(release);
                await db.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        // ==========================================
        // FEATURE: ADVANCED STUDENT SEARCH
        // ==========================================

        public async Task<ActionResult> SearchStudents(string query, int? batchId)
        {
            var userId = User.Identity.GetUserId();
            var lectBatches = await db.BatchLecturers
                .Where(bl => bl.LecturerId == userId)
                .Select(bl => bl.BatchId)
                .ToListAsync();

            var studentQuery = db.BatchEnrollments
                .Where(be => lectBatches.Contains(be.BatchId))
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                studentQuery = studentQuery.Where(be => be.Enrollment.Student.FirstName.Contains(query) || 
                                                       be.Enrollment.Student.LastName.Contains(query) || 
                                                       be.Enrollment.Student.Email.Contains(query));
            }

            if (batchId.HasValue)
            {
                studentQuery = studentQuery.Where(be => be.BatchId == batchId.Value);
            }

            var results = await studentQuery
                .Select(be => new StudentSearchVM
                {
                    StudentId = be.Enrollment.StudentId,
                    FullName = be.Enrollment.Student.FirstName + " " + be.Enrollment.Student.LastName,
                    Email = be.Enrollment.Student.Email,
                    BatchCode = be.Batch.BatchCode,
                    CourseTitle = be.Batch.Course.Title,
                    EnrollmentDate = be.Enrollment.EnrolledAt
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            ViewBag.Batches = await db.Batches.Where(b => lectBatches.Contains(b.BatchId)).ToListAsync();
            ViewBag.Query = query;
            ViewBag.SelectedBatchId = batchId;

            return View(results);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
