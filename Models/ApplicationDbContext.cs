using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace UniManage.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            // Strictly disable initializers to prevent EF from trying to create already-existing tables
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<CourseMaterial> Materials { get; set; }
        public DbSet<CoursePrerequisite> Prerequisites { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<BatchEnrollment> BatchEnrollments { get; set; }
        public DbSet<BatchLecturer> BatchLecturers { get; set; }
        public DbSet<AssignmentRelease> AssignmentReleases { get; set; }
        public DbSet<MaterialRelease> MaterialReleases { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Standard Entity Framework configuration for SQL Server
            // Disable cascade delete for Course -> CreatedBy and Course -> Lecturer to avoid multiple cascade paths
            modelBuilder.Entity<Course>()
                .HasRequired(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .WillCascadeOnDelete(false);

            // Module relationships
            modelBuilder.Entity<Module>()
                .HasRequired(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .WillCascadeOnDelete(true); // Deleting a course deletes its modules

            modelBuilder.Entity<Module>()
                .HasRequired(m => m.Lecturer)
                .WithMany()
                .HasForeignKey(m => m.LecturerId)
                .WillCascadeOnDelete(false);

            // Assignment parent update
            modelBuilder.Entity<Assignment>()
                .HasOptional(a => a.Module)
                .WithMany(m => m.Assignments)
                .HasForeignKey(a => a.ModuleId)
                .WillCascadeOnDelete(true);

            // Material parent update
            modelBuilder.Entity<CourseMaterial>()
                .HasOptional(cm => cm.Module)
                .WithMany(m => m.Materials)
                .HasForeignKey(cm => cm.ModuleId)
                .WillCascadeOnDelete(true);

            // Message relationships
            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .WillCascadeOnDelete(false);

            // Enrollment relationships
            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(s => s.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Enrollment>()
                .HasOptional(e => e.ApprovedBy)
                .WithMany()
                .HasForeignKey(e => e.ApprovedById)
                .WillCascadeOnDelete(false);

            // Submission relationships
            modelBuilder.Entity<Submission>()
                .HasRequired(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Submission>()
                .HasRequired(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Submission>()
                .HasOptional(s => s.GradedBy)
                .WithMany()
                .HasForeignKey(s => s.GradedById)
                .WillCascadeOnDelete(false);

            // CoursePrerequisite mapping
            modelBuilder.Entity<CoursePrerequisite>()
                .HasRequired(p => p.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(p => p.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CoursePrerequisite>()
                .HasRequired(p => p.PrerequisiteCourse)
                .WithMany()
                .HasForeignKey(p => p.PrerequisiteCourseId)
                .WillCascadeOnDelete(false);

            // Batch configuration
            modelBuilder.Entity<Batch>()
                .HasRequired(b => b.Course)
                .WithMany(c => c.Batches)
                .HasForeignKey(b => b.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Batch>()
                .HasRequired(b => b.CreatedBy)
                .WithMany()
                .HasForeignKey(b => b.CreatedById)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Batch>()
                .HasIndex(b => new { b.CourseId, b.BatchCode })
                .IsUnique();

            // BatchEnrollment configuration
            modelBuilder.Entity<BatchEnrollment>()
                .HasRequired(be => be.Batch)
                .WithMany(b => b.BatchEnrollments)
                .HasForeignKey(be => be.BatchId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<BatchEnrollment>()
                .HasRequired(be => be.Enrollment)
                .WithMany(e => e.BatchEnrollments)
                .HasForeignKey(be => be.EnrollmentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BatchEnrollment>()
                .HasIndex(be => new { be.BatchId, be.EnrollmentId })
                .IsUnique();

            // BatchLecturer — prevent multiple cascade paths
            modelBuilder.Entity<BatchLecturer>()
                .HasRequired(bl => bl.Batch)
                .WithMany(b => b.BatchLecturers)
                .HasForeignKey(bl => bl.BatchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BatchLecturer>()
                .HasRequired(bl => bl.Lecturer)
                .WithMany()
                .HasForeignKey(bl => bl.LecturerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BatchLecturer>()
                .HasOptional(bl => bl.AssignedBy)
                .WithMany()
                .HasForeignKey(bl => bl.AssignedById)
                .WillCascadeOnDelete(false);

            // Unique: one lecturer record per batch per person
            modelBuilder.Entity<BatchLecturer>()
                .HasIndex(bl => new { bl.BatchId, bl.LecturerId })
                .IsUnique();

            // AssignmentRelease
            modelBuilder.Entity<AssignmentRelease>()
                .HasRequired(ar => ar.Assignment)
                .WithMany(a => a.AssignmentReleases)
                .HasForeignKey(ar => ar.AssignmentId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<AssignmentRelease>()
                .HasRequired(ar => ar.Batch)
                .WithMany(b => b.AssignmentReleases)
                .HasForeignKey(ar => ar.BatchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssignmentRelease>()
                .HasOptional(ar => ar.ReleasedBy)
                .WithMany()
                .HasForeignKey(ar => ar.ReleasedById)
                .WillCascadeOnDelete(false);

            // Unique: one release record per assignment per batch
            modelBuilder.Entity<AssignmentRelease>()
                .HasIndex(ar => new { ar.AssignmentId, ar.BatchId })
                .IsUnique();

            // MaterialRelease
            modelBuilder.Entity<MaterialRelease>()
                .HasRequired(mr => mr.Material)
                .WithMany(m => m.MaterialReleases)
                .HasForeignKey(mr => mr.MaterialId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<MaterialRelease>()
                .HasRequired(mr => mr.Batch)
                .WithMany(b => b.MaterialReleases)
                .HasForeignKey(mr => mr.BatchId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MaterialRelease>()
                .HasOptional(mr => mr.ReleasedBy)
                .WithMany()
                .HasForeignKey(mr => mr.ReleasedById)
                .WillCascadeOnDelete(false);

            // Unique: one release per material per batch
            modelBuilder.Entity<MaterialRelease>()
                .HasIndex(mr => new { mr.MaterialId, mr.BatchId })
                .IsUnique();
        }
    }
}
