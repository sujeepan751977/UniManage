namespace UniManage.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using UniManage.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<UniManage.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "UniManage.Models.ApplicationDbContext";
        }

        protected override void Seed(UniManage.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // 1. Roles
            string[] roles = { "Administrator", "Lecturer", "Student" };
            foreach (var role in roles)
            {
                if (!roleManager.RoleExists(role))
                {
                    roleManager.Create(new IdentityRole(role));
                }
            }

            // 2. Admin User
            string adminEmail = "admin@unimanage.edu";
            if (userManager.FindByEmail(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    DateOfBirth = new DateTime(1990, 1, 1),
                    NIC = "ADMIN-NIC-001",
                    Address = "University Data Center"
                };
                userManager.Create(admin, "Admin@123");
                userManager.AddToRole(admin.Id, "Administrator");
            }

            // 3. Lecturer User
            string lecturerEmail = "lecturer@unimanage.edu";
            if (userManager.FindByEmail(lecturerEmail) == null)
            {
                var lecturer = new ApplicationUser
                {
                    UserName = lecturerEmail,
                    Email = lecturerEmail,
                    FirstName = "Dr. Robert",
                    LastName = "Smith",
                    IsActive = true,
                    DateOfBirth = new DateTime(1980, 5, 15),
                    NIC = "LECT-NIC-1234",
                    Address = "No 45, Academic Square, Colombo",
                    Bio = "Experienced Professor in Computer Science with a focus on Software Engineering."
                };
                userManager.Create(lecturer, "Lecturer@123");
                userManager.AddToRole(lecturer.Id, "Lecturer");
            }

            // 4. Sample Courses
            var adminUser = userManager.FindByEmail(adminEmail);
            var lecturerUser = userManager.FindByEmail(lecturerEmail);
            
            if (adminUser != null && lecturerUser != null)
            {
                context.Courses.AddOrUpdate(
                    c => c.CourseCode,
                    new Course
                    {
                        CourseCode = "CS101",
                        Title = "Introduction to Computer Science",
                        Description = "Fundamentals of programming, algorithms, and data structures.",
                        Credits = 4,
                        MaxEnrollment = 50,
                        Status = CourseStatus.Active,
                        CreatedById = adminUser.Id,
                        CreatedAt = DateTime.Now
                    },
                    new Course
                    {
                        CourseCode = "CS202",
                        Title = "Web Development with ASP.NET",
                        Description = "Building robust web applications using MVC 5 and Entity Framework.",
                        Credits = 3,
                        MaxEnrollment = 30,
                        Status = CourseStatus.Active,
                        CreatedById = adminUser.Id,
                        CreatedAt = DateTime.Now
                    }
                );

                context.SaveChanges();

                // 5. Seed Modules for the courses
                var cs101 = context.Courses.FirstOrDefault(c => c.CourseCode == "CS101");
                var cs202 = context.Courses.FirstOrDefault(c => c.CourseCode == "CS202");

                if (cs101 != null)
                {
                    context.Modules.AddOrUpdate(
                        m => m.ModuleCode,
                        new Module { ModuleCode = "CS101-A", Title = "Programming Basics", CourseId = cs101.CourseId, LecturerId = lecturerUser.Id },
                        new Module { ModuleCode = "CS101-B", Title = "Data Structures", CourseId = cs101.CourseId, LecturerId = lecturerUser.Id }
                    );
                    context.SaveChanges();

                    var mod1A = context.Modules.FirstOrDefault(m => m.ModuleCode == "CS101-A");
                    if (mod1A != null) {
                        context.Assignments.AddOrUpdate(a => a.Title,
                            new Assignment { Title = "Write Your First C# Program", Description = "Hello World and Basic Syntax", ModuleId = mod1A.ModuleId, CreatedById = adminUser.Id, Deadline = DateTime.Now.AddDays(7), MaxMarks = 100 }
                        );
                        context.Materials.AddOrUpdate(m => m.Title,
                            new CourseMaterial { Title = "Introduction to C# Slides", FileName = "intro.pdf", FilePath = "~/App_Data/Uploads/intro.pdf", ModuleId = mod1A.ModuleId, UploadedById = lecturerUser.Id }
                        );
                    }
                }

                if (cs202 != null)
                {
                    context.Modules.AddOrUpdate(
                        m => m.ModuleCode,
                        new Module { ModuleCode = "CS202-A", Title = "Frontend with MVC", CourseId = cs202.CourseId, LecturerId = lecturerUser.Id },
                        new Module { ModuleCode = "CS202-B", Title = "Entity Framework Core", CourseId = cs202.CourseId, LecturerId = lecturerUser.Id }
                    );
                    context.SaveChanges();

                    var mod2A = context.Modules.FirstOrDefault(m => m.ModuleCode == "CS202-A");
                    if (mod2A != null) {
                        context.Assignments.AddOrUpdate(a => a.Title,
                            new Assignment { Title = "Building Your First View", Description = "Create a Razor view with layouts", ModuleId = mod2A.ModuleId, CreatedById = adminUser.Id, Deadline = DateTime.Now.AddDays(14), MaxMarks = 100 }
                        );
                    }
                }
            }

            context.SaveChanges();
        }
    }
}
