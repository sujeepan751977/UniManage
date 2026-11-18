-- Final Schema Reconciliation Script
-- Matches Enrollments table to C# Model
USE [UniManageDB_New];
GO

PRINT 'Reconciling Enrollments table...';

-- Drop old table if exists
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL 
    DROP TABLE dbo.Enrollments;

-- Recreate with correct column names matching the Enrollment model
CREATE TABLE [dbo].[Enrollments] (
    [EnrollmentId] INT             IDENTITY (1, 1) NOT NULL,
    [CourseId]     INT             NOT NULL,
    [StudentId]    NVARCHAR (128)  NOT NULL,
    [EnrolledAt]   DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [CompletedAt]  DATETIME        NULL,
    [Status]       INT             DEFAULT 0 NOT NULL, 
    [FinalGrade]   DECIMAL (5, 2)  NULL,
    CONSTRAINT [PK_dbo.Enrollments] PRIMARY KEY CLUSTERED ([EnrollmentId] ASC),
    CONSTRAINT [FK_dbo.Enrollments_dbo.Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Courses] ([CourseId]),
    CONSTRAINT [FK_dbo.Enrollments_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

PRINT 'Enrollments schema synchronized successfully.';
GO
