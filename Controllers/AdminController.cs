using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using UniManage.Models;
using UniManage.ViewModels.Admin;
using UniManage.Helpers;
using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Text;

namespace UniManage.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            var studentRole = db.Roles.FirstOrDefault(r => r.Name == "Student");
            var lecturerRole = db.Roles.FirstOrDefault(r => r.Name == "Lecturer");
            
            string studentRoleId = studentRole?.Id ?? "";
            string lecturerRoleId = lecturerRole?.Id ?? "";

            var model = new AdminDashboardViewModel
            {
                TotalStudents = db.Users.Count(u => u.Roles.Any(r => r.RoleId == studentRoleId)),
                TotalLecturers = db.Users.Count(u => u.Roles.Any(r => r.RoleId == lecturerRoleId)),
                TotalCourses = db.Courses.Count(),
                ActiveEnrollments = db.Enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                RecentAudits = db.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList()
                    .Select(a => new RecentAuditVM
                    {
                        Action = a.Action,
                        User = a.PerformedBy?.FullName ?? "System",
                        Details = a.Details,
                        Timestamp = a.Timestamp
                    }).ToList()
            };
            return View(model);
        }

        // GET: Admin/Users
        public ActionResult Users(string searchTerm, string role, bool? isActive)
        {
            var query = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.FirstName.Contains(searchTerm) || 
                                       u.LastName.Contains(searchTerm) || 
                                       u.Email.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var roleObj = db.Roles.FirstOrDefault(r => r.Name == role);
                if (roleObj != null) query = query.Where(u => u.Roles.Any(r => r.RoleId == roleObj.Id));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // Get roles for display lookup
            ViewBag.AllRoles = db.Roles.ToList();

            var model = new UserListViewModel
            {
                Users = query.OrderBy(u => u.LastName).ToList(),
                SearchTerm = searchTerm,
                Role = role,
                IsActive = isActive
            };

            return View(model);
        }

        // GET: Admin/EditUser/5
        public async Task<ActionResult> EditUser(string id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return HttpNotFound();

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roles = await userManager.GetRolesAsync(user.Id);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                NIC = user.NIC,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Role = roles.FirstOrDefault() ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = await userManager.FindByIdAsync(model.Id);

                if (user == null) return HttpNotFound();

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NIC = model.NIC;
                user.Address = model.Address;
                user.DateOfBirth = model.DateOfBirth;
                user.PhoneNumber = model.PhoneNumber;

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update Role
                    var currentRoles = await userManager.GetRolesAsync(user.Id);
                    if (!currentRoles.Contains(model.Role))
                    {
                        await userManager.RemoveFromRolesAsync(user.Id, currentRoles.ToArray());
                        await userManager.AddToRoleAsync(user.Id, model.Role);
                    }

                    // Update Password if provided
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        await userManager.RemovePasswordAsync(user.Id);
                        await userManager.AddPasswordAsync(user.Id, model.NewPassword);
                    }

                    AuditHelper.Log(User.Identity.GetUserId(), "Update", "User", user.Id, $"Admin updated user details for {user.Email}", Request);
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Users");
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByIdAsync(id);
            
            if (user != null)
            {
                // Prevent self-deletion
                if (user.Id == User.Identity.GetUserId())
                {
                    TempData["Error"] = "You cannot delete your own administrative account.";
                    return RedirectToAction("Users");
                }

                AuditHelper.Log(User.Identity.GetUserId(), "Delete", "User", user.Id, $"Admin permanently deleted user: {user.Email}", Request);
                await userManager.DeleteAsync(user);
                TempData["Success"] = "User deleted successfully!";
                return RedirectToAction("Users");
            }
            return HttpNotFound();
        }

        // GET: Admin/Courses
        public ActionResult Courses(string searchTerm)
        {
            var query = db.Courses.Include(c => c.Modules).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c => c.Title.Contains(searchTerm) || c.CourseCode.Contains(searchTerm));
            }

            ViewBag.SearchTerm = searchTerm;
            var courses = query.ToList();
            return View(courses);
        }

        // GET: Admin/CreateCourse
        public ActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCourse(Course model)
        {
            // Manually assign the creator ID as it's required by the model but not by the form
            model.CreatedById = User.Identity.GetUserId();
            ModelState.Remove("CreatedById");

            if (ModelState.IsValid)
            {
                db.Courses.Add(model);
                db.SaveChanges();
                AuditHelper.Log(User.Identity.GetUserId(), "Create", "Course", model.CourseId.ToString(), $"Admin created course: {model.Title}", Request);
                TempData["Success"] = "Course created successfully! Now add modules to this course.";
                return RedirectToAction("Modules", new { id = model.CourseId });
            }

            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["Error"] = "Failed to create course. Errors: " + errors;

            return View(model);
        }

        // GET: Admin/EditCourse/5
        public ActionResult EditCourse(int id)
        {
            var course = db.Courses.Find(id);
            if (course == null) return HttpNotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse(Course model)
        {
            if (ModelState.IsValid)
            {
                var original = db.Courses.Find(model.CourseId);
                if (original == null) return HttpNotFound();

                original.Title = model.Title;
                original.CourseCode = model.CourseCode;
                original.Description = model.Description;
                original.Credits = model.Credits;
                original.MaxEnrollment = model.MaxEnrollment;
                original.Status = model.Status;

                db.SaveChanges();
                AuditHelper.Log(User.Identity.GetUserId(), "Update", "Course", model.CourseId.ToString(), $"Admin updated course: {model.Title}", Request);
                TempData["Success"] = "Course updated successfully!";
                return RedirectToAction("Courses");
            }
            return View(model);
        }


        // GET: Admin/Modules/5
        public ActionResult Modules(int id)
        {
            var course = db.Courses.Include(c => c.Modules.Select(m => m.Lecturer)).FirstOrDefault(c => c.CourseId == id);
            if (course == null) return HttpNotFound();

            var lecturerRole = db.Roles.FirstOrDefault(r => r.Name == "Lecturer");
            var lecturers = db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == lecturerRole.Id))
                .Select(u => new LecturerSelectionVM { Id = u.Id, FullName = u.FirstName + " " + u.LastName })
                .ToList();
            
            ViewBag.Lecturers = new SelectList(lecturers, "Id", "FullName");
            return View(course);
        }

        // POST: Admin/CreateModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateModule(Module model)
        {
            if (ModelState.IsValid)
            {
                db.Modules.Add(model);
                db.SaveChanges();
                AuditHelper.Log(User.Identity.GetUserId(), "Create", "Module", model.ModuleId.ToString(), $"Admin created module: {model.Title} for course {model.CourseId}", Request);
                TempData["Success"] = "Module created successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to create module. Please check inputs.";
            }

            return RedirectToAction("Modules", new { id = model.CourseId });
        }

        // POST: Admin/DeleteModule/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteModule(int id)
        {
            var module = db.Modules.Find(id);
            if (module != null)
            {
                int courseId = module.CourseId;
                db.Modules.Remove(module);
                db.SaveChanges();
                AuditHelper.Log(User.Identity.GetUserId(), "Delete", "Module", id.ToString(), $"Admin deleted module: {module.Title}", Request);
                TempData["Success"] = "Module deleted successfully!";
                return RedirectToAction("Modules", new { id = courseId });
            }
            return HttpNotFound();
        }

        // GET: Admin/AuditLogs
        public ActionResult AuditLogs()
        {
            var logs = db.AuditLogs.Include(l => l.PerformedBy).OrderByDescending(l => l.Timestamp).Take(100).ToList();
            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleUserStatus(string id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                db.SaveChanges();
                AuditHelper.Log(User.Identity.GetUserId(), "Update", "User", user.Id, $"Admin toggled user status for {user.Email}", Request);
                return Json(new { success = true, isActive = user.IsActive });
            }
            return Json(new { success = false });
        }

        // GET: Admin/AddUser
        public ActionResult AddUser()
        {
            return View();
        }

        // POST: Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    NIC = model.NIC,
                    Address = model.Address,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user.Id, model.Role);
                    AuditHelper.Log(User.Identity.GetUserId(), "Create", "User", user.Id, $"Admin created {model.Role}: {user.Email}", Request);
                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction("Users");
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error);
            }
            return View(model);
        }

        // ==========================================
        // FEATURE 1: ENROLLMENT APPROVAL WORKFLOW
        // ==========================================

        public async Task<ActionResult> PendingEnrollments()
        {
            var pending = await db.Enrollments
                .Where(e => e.Status == EnrollmentStatus.Pending)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var model = new PendingEnrollmentsViewModel
            {
                TotalPending = pending.Count,
                TodayCount = pending.Count(e => e.EnrolledAt >= today),
                ThisWeekCount = pending.Count(e => e.EnrolledAt >= weekStart),
                Items = pending.Select(e => new PendingEnrollmentVM
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    StudentFullName = e.Student.FullName,
                    StudentEmail = e.Student.Email,
                    StudentDOB = e.Student.DateOfBirth,
                    CourseCode = e.Course.CourseCode,
                    CourseTitle = e.Course.Title,
                    CourseCredits = e.Course.Credits,
                    CourseMaxEnrollment = e.Course.MaxEnrollment,
                    CurrentActiveCount = db.Enrollments.Count(en => en.CourseId == e.CourseId && en.Status == EnrollmentStatus.Active),
                    EnrolledAt = e.EnrolledAt,
                    HasMetPrerequisites = CheckPrerequisites(e.StudentId, e.CourseId, out var missing),
                    MissingPrerequisites = missing
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveEnrollment(int enrollmentId, string adminNotes)
        {
            var enrollment = await db.Enrollments.Include(e => e.Course).Include(e => e.Student).FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null) return HttpNotFound();

            enrollment.Status = EnrollmentStatus.Active;
            enrollment.ApprovedById = User.Identity.GetUserId();
            enrollment.ApprovedAt = DateTime.UtcNow;
            enrollment.AdminNotes = adminNotes;

            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "EnrollmentApproved", "Enrollment", enrollment.EnrollmentId.ToString(), $"Admin approved enrollment for {enrollment.Student.Email} in {enrollment.Course.CourseCode}", Request);
            TempData["Success"] = $"Enrollment approved for {enrollment.Student.FullName} in {enrollment.Course.CourseCode}.";

            return RedirectToAction("PendingEnrollments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RejectEnrollment(int enrollmentId, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["Error"] = "A rejection reason is required.";
                return RedirectToAction("PendingEnrollments");
            }

            var enrollment = await db.Enrollments.Include(e => e.Course).Include(e => e.Student).FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null) return HttpNotFound();

            enrollment.Status = EnrollmentStatus.Rejected;
            enrollment.ApprovedById = User.Identity.GetUserId();
            enrollment.ApprovedAt = DateTime.UtcNow;
            enrollment.RejectionReason = rejectionReason;

            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "EnrollmentRejected", "Enrollment", enrollment.EnrollmentId.ToString(), $"Admin rejected enrollment for {enrollment.Student.Email} in {enrollment.Course.CourseCode}. Reason: {rejectionReason}", Request);
            TempData["Error"] = $"Enrollment rejected for {enrollment.Student.FullName}.";

            return RedirectToAction("PendingEnrollments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkApprove(List<int> enrollmentIds)
        {
            if (enrollmentIds == null || !enrollmentIds.Any())
            {
                TempData["Error"] = "No enrollments selected.";
                return RedirectToAction("PendingEnrollments");
            }

            var enrollments = await db.Enrollments.Where(e => enrollmentIds.Contains(e.EnrollmentId)).ToListAsync();
            int count = 0;
            string adminId = User.Identity.GetUserId();

            foreach (var e in enrollments)
            {
                if (e.Status == EnrollmentStatus.Pending)
                {
                    e.Status = EnrollmentStatus.Active;
                    e.ApprovedById = adminId;
                    e.ApprovedAt = DateTime.UtcNow;
                    count++;
                }
            }

            await db.SaveChangesAsync();
            TempData["Success"] = $"{count} enrollments approved successfully.";
            return RedirectToAction("PendingEnrollments");
        }

        private bool CheckPrerequisites(string studentId, int courseId, out List<string> missing)
        {
            missing = new List<string>();
            var prerequisites = db.Prerequisites.Where(p => p.CourseId == courseId).Include(p => p.PrerequisiteCourse).ToList();
            
            foreach (var pre in prerequisites)
            {
                bool completed = db.Enrollments.Any(e => e.StudentId == studentId && e.CourseId == pre.PrerequisiteCourseId && e.Status == EnrollmentStatus.Completed);
                if (!completed)
                {
                    missing.Add(pre.PrerequisiteCourse.CourseCode);
                }
            }
            return !missing.Any();
        }

        // ==========================================
        // FEATURE 2: BATCH MANAGEMENT
        // ==========================================

        public async Task<ActionResult> Batches(int? SelectedCourseId, BatchStatus? status)
        {
            var query = db.Batches.Include(b => b.Course).Include(b => b.BatchEnrollments).Include(b => b.BatchLecturers.Select(bl => bl.Lecturer)).AsQueryable();

            if (SelectedCourseId.HasValue) query = query.Where(b => b.CourseId == SelectedCourseId);
            if (status.HasValue) query = query.Where(b => b.Status == status);

            var batches = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            var model = new UniManage.Models.BatchListViewModel
            {
                Batches = batches.Select(b => new BatchVM
                {
                    BatchId = b.BatchId,
                    BatchCode = b.BatchCode,
                    BatchName = b.BatchName,
                    CourseCode = b.Course.CourseCode,
                    CourseTitle = b.Course.Title,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    MaxCapacity = b.MaxCapacity,
                    EnrolledCount = b.BatchEnrollments.Count,
                    Status = b.Status,
                    LecturerName = b.BatchLecturers.FirstOrDefault(bl => bl.IsPrimary)?.Lecturer.FirstName + " " + b.BatchLecturers.FirstOrDefault(bl => bl.IsPrimary)?.Lecturer.LastName 
                                   ?? b.BatchLecturers.FirstOrDefault()?.Lecturer.FirstName + " " + b.BatchLecturers.FirstOrDefault()?.Lecturer.LastName 
                                   ?? "Not Assigned",
                    CreatedAt = b.CreatedAt
                }).ToList(),
                Courses = await db.Courses.Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.CourseCode + " - " + c.Title }).ToListAsync(),
                SelectedCourseId = SelectedCourseId,
                SelectedStatus = status
            };

            return View(model);
        }

        public async Task<ActionResult> CreateBatch(int? courseId)
        {
            await PopulateBatchViewBags();
            return View(new UniManage.Models.CreateBatchViewModel { CourseId = courseId ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateBatch(UniManage.Models.CreateBatchViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Unique check
                if (await db.Batches.AnyAsync(b => b.CourseId == model.CourseId && b.BatchCode == model.BatchCode))
                {
                    ModelState.AddModelError("BatchCode", "A batch with this code already exists for this course.");
                }
                else if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                }
                else
                {
                    var batch = new Batch
                    {
                        CourseId = model.CourseId,
                        BatchName = model.BatchName,
                        BatchCode = model.BatchCode,
                        Description = model.Description,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        MaxCapacity = model.MaxCapacity,
                        CreatedById = User.Identity.GetUserId()
                    };
                    db.Batches.Add(batch);
                    await db.SaveChangesAsync();

                    AuditHelper.Log(User.Identity.GetUserId(), "BatchCreated", "Batch", batch.BatchId.ToString(), $"Admin created batch {batch.BatchCode}", Request);
                    TempData["Success"] = $"Batch {batch.BatchCode} created successfully. Now assign lecturers to this batch.";
                    return RedirectToAction("ManageBatchLecturers", new { id = batch.BatchId });
                }
            }

            await PopulateBatchViewBags();
            
            return View(model);
        }

        public async Task<ActionResult> EditBatch(int id)
        {
            var batch = await db.Batches.FindAsync(id);
            if (batch == null) return HttpNotFound();

            var model = new UniManage.Models.EditBatchViewModel
            {
                BatchId = batch.BatchId,
                CourseId = batch.CourseId,
                BatchName = batch.BatchName,
                BatchCode = batch.BatchCode,
                Description = batch.Description,
                StartDate = batch.StartDate,
                EndDate = batch.EndDate,
                MaxCapacity = batch.MaxCapacity,
                Status = batch.Status
            };

            await PopulateBatchViewBags();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditBatch(UniManage.Models.EditBatchViewModel model)
        {
            if (ModelState.IsValid)
            {
                var batch = await db.Batches.FindAsync(model.BatchId);
                if (batch == null) return HttpNotFound();

                // Unique check (excluding self)
                if (await db.Batches.AnyAsync(b => b.CourseId == model.CourseId && b.BatchCode == model.BatchCode && b.BatchId != model.BatchId))
                {
                    ModelState.AddModelError("BatchCode", "Another batch with this code already exists for this course.");
                }
                else if (model.EndDate <= model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be after start date.");
                }
                else
                {
                    batch.BatchName = model.BatchName;
                    batch.BatchCode = model.BatchCode;
                    batch.Description = model.Description;
                    batch.StartDate = model.StartDate;
                    batch.EndDate = model.EndDate;
                    batch.MaxCapacity = model.MaxCapacity;
                    batch.Status = model.Status;

                    await db.SaveChangesAsync();

                    AuditHelper.Log(User.Identity.GetUserId(), "BatchUpdated", "Batch", batch.BatchId.ToString(), $"Admin updated batch {batch.BatchCode}", Request);
                    TempData["Success"] = $"Batch {batch.BatchCode} updated successfully.";
                    return RedirectToAction("Batches", new { SelectedCourseId = model.CourseId });
                }
            }

            await PopulateBatchViewBags();
            
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteBatch(int id)
        {
            var batch = await db.Batches
                .Include(b => b.BatchEnrollments)
                .Include(b => b.BatchLecturers)
                .Include(b => b.AssignmentReleases)
                .Include(b => b.MaterialReleases)
                .FirstOrDefaultAsync(b => b.BatchId == id);

            if (batch == null) return HttpNotFound();

            // Clear secondary data first to avoid FK constraints
            db.BatchEnrollments.RemoveRange(batch.BatchEnrollments);
            db.BatchLecturers.RemoveRange(batch.BatchLecturers);
            db.AssignmentReleases.RemoveRange(batch.AssignmentReleases);
            db.MaterialReleases.RemoveRange(batch.MaterialReleases);

            db.Batches.Remove(batch);
            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "BatchDeleted", "Batch", id.ToString(), $"Admin deleted batch {batch.BatchCode}", Request);
            return Json(new { success = true });
        }


        public async Task<ActionResult> BatchDetail(int id)
        {
            var batch = await db.Batches
                .Include(b => b.Course)
                .Include(b => b.BatchLecturers.Select(bl => bl.Lecturer))
                .Include(b => b.BatchEnrollments.Select(be => be.Enrollment.Student))
                .FirstOrDefaultAsync(b => b.BatchId == id);

            if (batch == null) return HttpNotFound();

            var assignedEnrollmentIds = batch.BatchEnrollments.Select(be => be.EnrollmentId).ToList();

            // Students enrolled in this course but not in this batch OR any other batch for this course
            var courseEnrollments = await db.Enrollments
                .Where(e => e.CourseId == batch.CourseId && e.Status == EnrollmentStatus.Active)
                .Include(e => e.Student)
                .ToListAsync();

            // Find students already in other batches for the same course
            var otherBatchEnrollments = await db.BatchEnrollments
                .Where(be => be.Batch.CourseId == batch.CourseId && be.BatchId != id)
                .Select(be => be.EnrollmentId)
                .ToListAsync();

            var model = new UniManage.Models.BatchDetailViewModel
            {
                Batch = new BatchVM
                {
                    BatchId = batch.BatchId,
                    BatchCode = batch.BatchCode,
                    BatchName = batch.BatchName,
                    CourseCode = batch.Course.CourseCode,
                    CourseTitle = batch.Course.Title,
                    MaxCapacity = batch.MaxCapacity,
                    EnrolledCount = batch.BatchEnrollments.Count,
                    Status = batch.Status,
                    LecturerName = string.Join(", ", batch.BatchLecturers.Select(bl => bl.Lecturer.FullName)),
                    StartDate = batch.StartDate,
                    EndDate = batch.EndDate
                },
                AssignedStudents = batch.BatchEnrollments.Select(be => new BatchStudentVM
                {
                    EnrollmentId = be.EnrollmentId,
                    BatchEnrollmentId = be.BatchEnrollmentId,
                    StudentName = be.Enrollment.Student.FullName,
                    StudentEmail = be.Enrollment.Student.Email,
                    EnrolledDate = be.Enrollment.EnrolledAt
                }).ToList(),
                UnassignedStudents = courseEnrollments
                    .Where(e => !assignedEnrollmentIds.Contains(e.EnrollmentId) && !otherBatchEnrollments.Contains(e.EnrollmentId))
                    .Select(e => new BatchStudentVM
                    {
                        EnrollmentId = e.EnrollmentId,
                        StudentName = e.Student.FullName,
                        StudentEmail = e.Student.Email,
                        EnrolledDate = e.EnrolledAt
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignStudentToBatch(int batchId, int enrollmentId)
        {
            var batch = await db.Batches.Include(b => b.BatchEnrollments).FirstOrDefaultAsync(b => b.BatchId == batchId);
            if (batch == null) return HttpNotFound();

            if (batch.BatchEnrollments.Count >= batch.MaxCapacity)
            {
                TempData["Error"] = "Batch is at maximum capacity.";
                return RedirectToAction("BatchDetail", new { id = batchId });
            }

            // Check if student is already in ANOTHER batch for this same course
            var existingAssignment = await db.BatchEnrollments
                .FirstOrDefaultAsync(be => be.EnrollmentId == enrollmentId && be.Batch.CourseId == batch.CourseId);

            if (existingAssignment != null)
            {
                // If they are already in THIS batch, just return
                if (existingAssignment.BatchId == batchId)
                {
                    TempData["Success"] = "Student is already in this batch.";
                    return RedirectToAction("BatchDetail", new { id = batchId });
                }

                // Remove from the old batch first
                db.BatchEnrollments.Remove(existingAssignment);
                await db.SaveChangesAsync();
                
                TempData["Success"] = "Student has been moved from their previous batch to this one.";
            }

            var batchEnrollment = new BatchEnrollment
            {
                BatchId = batchId,
                EnrollmentId = enrollmentId,
                AssignedAt = DateTime.UtcNow,
                AssignedById = User.Identity.GetUserId()
            };

            db.BatchEnrollments.Add(batchEnrollment);
            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "StudentAssignedToBatch", "BatchEnrollment", batchEnrollment.BatchEnrollmentId.ToString(), $"Student assigned to batch {batch.BatchCode}", Request);
            return RedirectToAction("BatchDetail", new { id = batchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveStudentFromBatch(int? batchEnrollmentId)
        {
            int realId = batchEnrollmentId ?? 0;
            
            // Fallback to route 'id' if 'batchEnrollmentId' is not set
            if (realId == 0 && RouteData.Values["id"] != null)
                int.TryParse(RouteData.Values["id"].ToString(), out realId);

            if (realId == 0) return new HttpStatusCodeResult(400, "Missing ID");

            var be = await db.BatchEnrollments.FindAsync(realId);
            if (be == null) return HttpNotFound();

            int batchId = be.BatchId;
            db.Entry(be).State = EntityState.Deleted;
            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "StudentRemovedFromBatch", "BatchEnrollment", realId.ToString(), "Student removed from batch", Request);
            return RedirectToAction("BatchDetail", new { id = batchId });
        }

        public async Task<FileResult> ExportBatchStudents(int id)
        {
            var batch = await db.Batches.Include(b => b.Course).Include(b => b.BatchEnrollments.Select(be => be.Enrollment.Student)).FirstOrDefaultAsync(b => b.BatchId == id);
            if (batch == null) return null;

            var csv = new StringBuilder();
            csv.AppendLine("Student Name,Email,Enrolled Date,Batch Name,Course Code");

            foreach (var be in batch.BatchEnrollments)
            {
                csv.AppendLine($"{be.Enrollment.Student.FullName},{be.Enrollment.Student.Email},{be.Enrollment.EnrollmentId},{batch.BatchName},{batch.Course.CourseCode}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"{batch.BatchCode}_students_{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<ActionResult> ManageBatchLecturers(int id)
        {
            var batch = await db.Batches
                .Include(b => b.Course)
                .Include(b => b.BatchLecturers.Select(bl => bl.Lecturer))
                .FirstOrDefaultAsync(b => b.BatchId == id);

            if (batch == null) return HttpNotFound();

            var lecturerRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Lecturer");
            var roleId = lecturerRole?.Id;

            var assignedIds = batch.BatchLecturers.Select(bl => bl.LecturerId).ToList();

            var availableLecturers = await db.Users
                .Where(u => u.Roles.Any(r => r.RoleId == roleId) && !assignedIds.Contains(u.Id))
                .Select(u => new { u.Id, FullName = u.FirstName + " " + u.LastName, u.Email })
                .ToListAsync();

            var model = new BatchLecturerViewModel
            {
                BatchId = batch.BatchId,
                BatchCode = batch.BatchCode,
                BatchName = batch.BatchName,
                CourseTitle = batch.Course.Title,
                CurrentLecturers = batch.BatchLecturers.Select(bl => new AssignedLecturerVM
                {
                    BatchLecturerId = bl.BatchLecturerId,
                    LecturerId = bl.LecturerId,
                    FullName = bl.Lecturer.FullName,
                    Email = bl.Lecturer.Email,
                    IsPrimary = bl.IsPrimary,
                    Initials = (bl.Lecturer.FirstName.Substring(0, 1) + bl.Lecturer.LastName.Substring(0, 1)).ToUpper()
                }).ToList(),
                AvailableLecturers = availableLecturers.Select(l => new SelectListItem
                {
                    Value = l.Id,
                    Text = $"{l.FullName} ({l.Email})"
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignBatchLecturer(int batchId, string lecturerId, bool? isPrimary)
        {
            if (string.IsNullOrEmpty(lecturerId)) return RedirectToAction("ManageBatchLecturers", new { id = batchId });

            bool isPrimaryInstructor = isPrimary ?? false;

            // If making primary, un-primary others
            if (isPrimaryInstructor)
            {
                var others = await db.BatchLecturers.Where(bl => bl.BatchId == batchId && bl.IsPrimary).ToListAsync();
                foreach (var o in others) o.IsPrimary = false;
            }

            var assignment = new BatchLecturer
            {
                BatchId = batchId,
                LecturerId = lecturerId,
                IsPrimary = isPrimaryInstructor,
                AssignedAt = DateTime.UtcNow,
                AssignedById = User.Identity.GetUserId()
            };

            db.BatchLecturers.Add(assignment);
            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "LecturerAssignedToBatch", "BatchLecturer", assignment.BatchLecturerId.ToString(), $"Lecturer assigned to batch {batchId}", Request);
            return RedirectToAction("ManageBatchLecturers", new { id = batchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveBatchLecturer(int id)
        {
            var assignment = await db.BatchLecturers.FindAsync(id);
            if (assignment == null) return HttpNotFound();

            int batchId = assignment.BatchId;
            db.BatchLecturers.Remove(assignment);
            await db.SaveChangesAsync();

            AuditHelper.Log(User.Identity.GetUserId(), "LecturerRemovedFromBatch", "BatchLecturer", id.ToString(), "Lecturer removed from batch", Request);
            return RedirectToAction("ManageBatchLecturers", new { id = batchId });
        }

        public async Task<ActionResult> Reports()
        {
            var batches = await db.Batches
                .Include(b => b.Course)
                .Include(b => b.BatchEnrollments.Select(be => be.Enrollment))
                .ToListAsync();

            var model = batches.Select(b => new BatchAnalyticsVM
            {
                BatchName = b.BatchName,
                CourseTitle = b.Course.Title,
                StudentCount = b.BatchEnrollments.Count,
                Capacity = b.MaxCapacity,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Status = b.Status.ToString(),
                AverageGrade = b.BatchEnrollments.Any() ? 
                    (double?)b.BatchEnrollments.Average(be => be.Enrollment.FinalGrade ?? 0) : 0
            }).ToList();

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult SidebarStats()
        {
            var count = db.Enrollments.Count(e => e.Status == EnrollmentStatus.Pending);
            return PartialView("_AdminSidebarStats", count);
        }

        private async Task PopulateBatchViewBags()
        {
            ViewBag.Courses = await db.Courses
                .Select(c => new SelectListItem { Value = c.CourseId.ToString(), Text = c.CourseCode + " - " + c.Title })
                .ToListAsync();

            var lecturerRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Lecturer");
            if (lecturerRole != null)
            {
                var roleId = lecturerRole.Id;
                var lecturers = await db.Users
                    .Where(u => u.Roles.Any(r => r.RoleId == roleId))
                    .ToListAsync();
                
                ViewBag.Lecturers = lecturers
                    .Select(u => new SelectListItem { Value = u.Id, Text = u.FirstName + " " + u.LastName + " (" + u.Email + ")" })
                    .ToList();
            }
            else
            {
                ViewBag.Lecturers = new List<SelectListItem>();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
