/*
    UniManage Feature Update: Enrollment Approval + Batch Management
    --------------------------------------------------------------
    Impact:
    1. Updates dbo.Enrollments table with approval workflow columns.
    2. Creates dbo.Batches table.
    3. Creates dbo.BatchEnrollments table.
*/

SET NOCOUNT ON;
GO

PRINT 'Starting database schema update...';

-- 1. Update Enrollments Table
IF COL_ID('dbo.Enrollments', 'ApprovedById') IS NULL
BEGIN
    PRINT 'Adding approval columns to Enrollments table...';
    ALTER TABLE dbo.Enrollments ADD 
        [ApprovedById] NVARCHAR(128) NULL,
        [ApprovedAt] DATETIME NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [AdminNotes] NVARCHAR(500) NULL;

    -- Add Foreign Key for ApprovedById
    ALTER TABLE dbo.Enrollments ADD CONSTRAINT [FK_dbo.Enrollments_dbo.AspNetUsers_ApprovedById] 
        FOREIGN KEY ([ApprovedById]) REFERENCES [dbo].[AspNetUsers] ([Id]);
END
GO

-- 2. Create Batches Table
IF OBJECT_ID('dbo.Batches', 'U') IS NULL
BEGIN
    PRINT 'Creating Batches table...';
    CREATE TABLE [dbo].[Batches] (
        [BatchId] INT IDENTITY(1,1) NOT NULL,
        [CourseId] INT NOT NULL,
        [BatchName] NVARCHAR(100) NOT NULL,
        [BatchCode] NVARCHAR(20) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NOT NULL,
        [MaxCapacity] INT NOT NULL,
        [Status] INT NOT NULL DEFAULT 0, -- Upcoming=0, Active=1, Completed=2, Cancelled=3
        [CreatedById] NVARCHAR(128) NOT NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_dbo.Batches] PRIMARY KEY CLUSTERED ([BatchId] ASC),
        CONSTRAINT [FK_dbo.Batches_dbo.Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
        CONSTRAINT [FK_dbo.Batches_dbo.AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );

    -- Unique constraint for BatchCode per Course
    CREATE UNIQUE INDEX [IX_BatchCode_CourseId] ON [dbo].[Batches] ([CourseId], [BatchCode]);
END
GO

-- 3. Create BatchEnrollments Table
IF OBJECT_ID('dbo.BatchEnrollments', 'U') IS NULL
BEGIN
    PRINT 'Creating BatchEnrollments table...';
    CREATE TABLE [dbo].[BatchEnrollments] (
        [BatchEnrollmentId] INT IDENTITY(1,1) NOT NULL,
        [BatchId] INT NOT NULL,
        [EnrollmentId] INT NOT NULL,
        [AssignedAt] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [AssignedById] NVARCHAR(128) NOT NULL,
        CONSTRAINT [PK_dbo.BatchEnrollments] PRIMARY KEY CLUSTERED ([BatchEnrollmentId] ASC),
        CONSTRAINT [FK_dbo.BatchEnrollments_dbo.Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batches] ([BatchId]) ON DELETE CASCADE,
        CONSTRAINT [FK_dbo.BatchEnrollments_dbo.Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [dbo].[Enrollments] ([EnrollmentId]),
        CONSTRAINT [FK_dbo.BatchEnrollments_dbo.AspNetUsers_AssignedById] FOREIGN KEY ([AssignedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );

    -- Unique constraint for Enrollment per Batch
    CREATE UNIQUE INDEX [IX_Batch_Enrollment] ON [dbo].[BatchEnrollments] ([BatchId], [EnrollmentId]);
END
GO

PRINT 'Database schema update completed successfully.';
GO
