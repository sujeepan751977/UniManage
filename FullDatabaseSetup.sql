/*
    UniManage Database Initialization Script
    ----------------------------------------
    This script will:
    1. Drop existing application-specific and identity tables (if any).
    2. Create the complete schema for ASP.NET Identity (Roles, Users, etc.).
    3. Create the schema for the Core System (Courses, Modules, Assignments, etc.).
    4. Seed initial roles, admin users, and sample data.
*/

SET NOCOUNT ON;
GO

-- 1. DROP EXISTING TABLES (Reverse Dependency Order)
PRINT 'Dropping existing tables...';

-- Drop application tables
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Messages', 'U') IS NOT NULL DROP TABLE dbo.Messages;
IF OBJECT_ID('dbo.CoursePrerequisites', 'U') IS NOT NULL DROP TABLE dbo.CoursePrerequisites;
IF OBJECT_ID('dbo.CourseMaterials', 'U') IS NOT NULL DROP TABLE dbo.CourseMaterials;
IF OBJECT_ID('dbo.Submissions', 'U') IS NOT NULL DROP TABLE dbo.Submissions;
IF OBJECT_ID('dbo.Assignments', 'U') IS NOT NULL DROP TABLE dbo.Assignments;
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL DROP TABLE dbo.Enrollments;
IF OBJECT_ID('dbo.Modules', 'U') IS NOT NULL DROP TABLE dbo.Modules;
IF OBJECT_ID('dbo.Courses', 'U') IS NOT NULL DROP TABLE dbo.Courses;

-- Drop Identity tables
IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserLogins;
IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserClaims;
IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserRoles;
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NOT NULL DROP TABLE dbo.AspNetUsers;
IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetRoles;

GO

-- 2. CREATE IDENTITY TABLES
PRINT 'Creating Identity tables...';

CREATE TABLE [dbo].[AspNetRoles] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL,
    [Email]                NVARCHAR (256) NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR (MAX) NULL,
    [SecurityStamp]        NVARCHAR (MAX) NULL,
    [PhoneNumber]          NVARCHAR (20)  NOT NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [AccessFailedCount]    INT            NOT NULL,
    [UserName]             NVARCHAR (256) NOT NULL,
    -- Custom ApplicationUser fields
    [FirstName]            NVARCHAR (50)  NOT NULL,
    [LastName]             NVARCHAR (50)  NOT NULL,
    [DateOfBirth]          DATETIME       NOT NULL,
    [ProfilePicturePath]   NVARCHAR (MAX) NULL,
    [IsActive]             BIT            DEFAULT 1 NOT NULL,
    [CreatedAt]            DATETIME       DEFAULT GETUTCDATE() NOT NULL,
    [Bio]                  NVARCHAR (500) NULL,
    [NIC]                  NVARCHAR (20)  DEFAULT 'PENDING' NOT NULL,
    [Address]              NVARCHAR (200) NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (128) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

GO

-- 3. CREATE APPLICATION TABLES
PRINT 'Creating application tables...';

CREATE TABLE [dbo].[Courses] (
    [CourseId]       INT             IDENTITY (1, 1) NOT NULL,
    [CourseCode]     NVARCHAR (20)   NOT NULL,
    [Title]          NVARCHAR (150)  NOT NULL,
    [Description]    NVARCHAR (1000) NOT NULL,
    [Credits]        INT             NOT NULL,
    [MaxEnrollment]  INT             NOT NULL,
    [ThumbnailPath]  NVARCHAR (MAX)  NULL,
    [Status]         INT             DEFAULT 0 NOT NULL, -- 0: Active, 1: Archived, 2: Draft
    [CreatedAt]      DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [CreatedById]    NVARCHAR (128)  NOT NULL,
    CONSTRAINT [PK_dbo.Courses] PRIMARY KEY CLUSTERED ([CourseId] ASC),
    CONSTRAINT [FK_dbo.Courses_dbo.AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE TABLE [dbo].[Modules] (
    [ModuleId]    INT             IDENTITY (1, 1) NOT NULL,
    [ModuleCode]  NVARCHAR (20)   NOT NULL,
    [Title]       NVARCHAR (150)  NOT NULL,
    [Description] NVARCHAR (1000) NULL,
    [CourseId]    INT             NOT NULL,
    [LecturerId]  NVARCHAR (128)  NOT NULL,
    [Status]      INT             DEFAULT 0 NOT NULL,
    [CreatedAt]   DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT [PK_dbo.Modules] PRIMARY KEY CLUSTERED ([ModuleId] ASC),
    CONSTRAINT [FK_dbo.Modules_dbo.Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.Modules_dbo.AspNetUsers_LecturerId] FOREIGN KEY ([LecturerId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE TABLE [dbo].[Enrollments] (
    [EnrollmentId] INT             IDENTITY (1, 1) NOT NULL,
    [CourseId]     INT             NOT NULL,
    [StudentId]    NVARCHAR (128)  NOT NULL,
    [EnrolledAt]   DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [CompletedAt]  DATETIME        NULL,
    [Status]       INT             DEFAULT 0 NOT NULL, -- 0: Active, 1: Dropped, 2: Completed
    [FinalGrade]   DECIMAL (5, 2)  NULL,
    CONSTRAINT [PK_dbo.Enrollments] PRIMARY KEY CLUSTERED ([EnrollmentId] ASC),
    CONSTRAINT [FK_dbo.Enrollments_dbo.Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
    CONSTRAINT [FK_dbo.Enrollments_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE TABLE [dbo].[Assignments] (
    [AssignmentId] INT             IDENTITY (1, 1) NOT NULL,
    [Title]        NVARCHAR (150)  NOT NULL,
    [Description]  NVARCHAR (2000) NOT NULL,
    [ModuleId]     INT             NULL,
    [Deadline]     DATETIME        NOT NULL,
    [MaxMarks]             INT             DEFAULT 100 NOT NULL,
    [AllowLateSubmission] BIT             DEFAULT 0 NOT NULL,
    [FilePath]             NVARCHAR (MAX)  NULL,
    [CreatedAt]    DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [CreatedById]  NVARCHAR (128)  NOT NULL,
    CONSTRAINT [PK_dbo.Assignments] PRIMARY KEY CLUSTERED ([AssignmentId] ASC),
    CONSTRAINT [FK_dbo.Assignments_dbo.Modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Modules] ([ModuleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.Assignments_dbo.AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE TABLE [dbo].[Submissions] (
    [SubmissionId] INT             IDENTITY (1, 1) NOT NULL,
    [AssignmentId] INT             NOT NULL,
    [StudentId]    NVARCHAR (128)  NOT NULL,
    [FilePath]     NVARCHAR (MAX)  NOT NULL,
    [FileName]     NVARCHAR (MAX)  NOT NULL,
    [FileSize]     BIGINT          NOT NULL,
    [SubmittedAt]  DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [IsLate]       BIT             DEFAULT 0 NOT NULL,
    [Grade]        DECIMAL (18, 2) NULL,
    [Feedback]     NVARCHAR (1000) NULL,
    [GradedAt]     DATETIME        NULL,
    [GradedById]   NVARCHAR (128)  NULL,
    CONSTRAINT [PK_dbo.Submissions] PRIMARY KEY CLUSTERED ([SubmissionId] ASC),
    CONSTRAINT [FK_dbo.Submissions_dbo.Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments] ([AssignmentId]),
    CONSTRAINT [FK_dbo.Submissions_dbo.AspNetUsers_GradedById] FOREIGN KEY ([GradedById]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_dbo.Submissions_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE TABLE [dbo].[CourseMaterials] (
    [MaterialId]   INT             IDENTITY (1, 1) NOT NULL,
    [Title]        NVARCHAR (150)  NOT NULL,
    [Description]  NVARCHAR (500)  NULL,
    [ModuleId]     INT             NULL,
    [FilePath]     NVARCHAR (MAX)  NOT NULL,
    [FileName]     NVARCHAR (MAX)  NOT NULL,
    [FileSize]     BIGINT          DEFAULT 0 NOT NULL,
    [FileType]     NVARCHAR (50)   NULL,
    [UploadedAt]   DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [UploadedById] NVARCHAR (128)  NOT NULL,
    CONSTRAINT [PK_dbo.CourseMaterials] PRIMARY KEY CLUSTERED ([MaterialId] ASC),
    CONSTRAINT [FK_dbo.CourseMaterials_dbo.Modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [dbo].[Modules] ([ModuleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.CourseMaterials_dbo.AspNetUsers_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE TABLE [dbo].[CoursePrerequisites] (
    [CourseId]             INT NOT NULL,
    [PrerequisiteCourseId] INT NOT NULL,
    CONSTRAINT [PK_dbo.CoursePrerequisites] PRIMARY KEY CLUSTERED ([CourseId] ASC, [PrerequisiteCourseId] ASC),
    CONSTRAINT [FK_dbo.CoursePrerequisites_dbo.Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
    CONSTRAINT [FK_dbo.CoursePrerequisites_dbo.Courses_PrerequisiteCourseId] FOREIGN KEY ([PrerequisiteCourseId]) REFERENCES [dbo].[Courses] ([CourseId])
);

CREATE TABLE [dbo].[Messages] (
    [MessageId]         INT             IDENTITY (1, 1) NOT NULL,
    [SenderId]          NVARCHAR (128)  NOT NULL,
    [ReceiverId]        NVARCHAR (128)  NOT NULL,
    [CourseId]          INT             NULL,
    [Subject]           NVARCHAR (200)  NULL,
    [Content]           NVARCHAR (2000) NULL,
    [SentAt]            DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [IsRead]            BIT             DEFAULT 0 NOT NULL,
    [ReadAt]            DATETIME        NULL,
    [IsDeletedBySender] BIT             DEFAULT 0 NOT NULL,
    [IsDeletedByReceiver] BIT           DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo.Messages] PRIMARY KEY CLUSTERED ([MessageId] ASC),
    CONSTRAINT [FK_dbo.Messages_dbo.AspNetUsers_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_dbo.Messages_dbo.AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_dbo.Messages_dbo.Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId])
);

CREATE TABLE [dbo].[AuditLogs] (
    [LogId]          INT             IDENTITY (1, 1) NOT NULL,
    [Action]         NVARCHAR (100)  NOT NULL,
    [EntityAffected] NVARCHAR (100)  NOT NULL,
    [EntityId]       NVARCHAR (100)  NULL,
    [Details]        NVARCHAR (MAX)  NULL,
    [Timestamp]      DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [PerformedById]  NVARCHAR (128)  NULL,
    [IpAddress]      NVARCHAR (50)   NULL,
    [UserAgent]      NVARCHAR (300)  NULL,
    CONSTRAINT [PK_dbo.AuditLogs] PRIMARY KEY CLUSTERED ([LogId] ASC),
    CONSTRAINT [FK_dbo.AuditLogs_dbo.AspNetUsers_PerformedById] FOREIGN KEY ([PerformedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

GO

-- 4. SEED INITIAL DATA
PRINT 'Seeding data...';

-- Roles
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name]) VALUES ('1', 'Administrator');
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name]) VALUES ('2', 'Lecturer');
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name]) VALUES ('3', 'Student');

-- Admin Account: admin@unimanage.edu / Admin@123
-- Note: Password hash for Admin@123
DECLARE @AdminId NVARCHAR(128) = NEWID();
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [DateOfBirth], [IsActive], [CreatedAt], [NIC])
VALUES (@AdminId, 'admin@unimanage.edu', 1, 'AO9tGQZ1D70rBuev9dgIS3sEa54bCauFra+LtlMEApnt5nUWOITCLcDcPzg3z46W0A==', NEWID(), 0, 0, 1, 0, 'admin@unimanage.edu', 'System', 'Administrator', '1990-01-01', 1, GETUTCDATE(), 'ADMIN-NIC-001');

INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (@AdminId, '1');

-- Lecturer Account: lecturer@unimanage.edu / Lecturer@123
DECLARE @LecturerId NVARCHAR(128) = NEWID();
INSERT INTO [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount], [UserName], [FirstName], [LastName], [DateOfBirth], [IsActive], [CreatedAt], [NIC], [Bio])
VALUES (@LecturerId, 'lecturer@unimanage.edu', 1, 'AO9tGQZ1D70rBuev9dgIS3sEa54bCauFra+LtlMEApnt5nUWOITCLcDcPzg3z46W0A==', NEWID(), 0, 0, 1, 0, 'lecturer@unimanage.edu', 'Robert', 'Smith', '1980-05-15', 1, GETUTCDATE(), 'LECT-NIC-1234', 'Experienced Professor in AI.');

INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (@LecturerId, '2');

-- Sample Courses
INSERT INTO [dbo].[Courses] ([CourseCode], [Title], [Description], [Credits], [MaxEnrollment], [Status], [CreatedById])
VALUES ('CS101', 'Introduction to Computer Science', 'Fundamentals of programming and logic.', 4, 100, 0, @AdminId);

DECLARE @CourseId INT = SCOPE_IDENTITY();

-- Sample Modules
INSERT INTO [dbo].[Modules] ([ModuleCode], [Title], [Description], [CourseId], [LecturerId])
VALUES ('CS101-A', 'Programming Basics', 'Learn C# fundamentals.', @CourseId, @LecturerId);

DECLARE @ModuleId INT = SCOPE_IDENTITY();

-- Sample Assignment
INSERT INTO [dbo].[Assignments] ([Title], [Description], [ModuleId], [Deadline], [MaxMarks], [CreatedById])
VALUES ('Hello World Project', 'Write your first program.', @ModuleId, DATEADD(day, 7, GETDATE()), 100, @AdminId);

PRINT 'Schema restoration complete.';
GO
