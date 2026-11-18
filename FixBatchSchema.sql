-- Fix for Batch Management Schema
-- This script adds the missing LecturerId column to Batches and creates junction tables for many-to-many relationships.

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Batches') AND name = 'LecturerId')
BEGIN
    ALTER TABLE Batches ADD LecturerId NVARCHAR(128) NULL;
    
    -- Add Foreign Key constraint (assuming AspNetUsers is the table name)
    ALTER TABLE Batches ADD CONSTRAINT FK_dbo_Batches_dbo_AspNetUsers_LecturerId 
    FOREIGN KEY (LecturerId) REFERENCES AspNetUsers (Id);
END
GO

-- Create Junction Table for Assignment-Batch Many-to-Many relationship
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('AssignmentBatches') AND type in (N'U'))
BEGIN
    CREATE TABLE AssignmentBatches (
        AssignmentId INT NOT NULL,
        BatchId INT NOT NULL,
        CONSTRAINT PK_AssignmentBatches PRIMARY KEY (AssignmentId, BatchId),
        CONSTRAINT FK_AssignmentBatches_Assignments FOREIGN KEY (AssignmentId) REFERENCES Assignments (AssignmentId) ON DELETE CASCADE,
        CONSTRAINT FK_AssignmentBatches_Batches FOREIGN KEY (BatchId) REFERENCES Batches (BatchId) ON DELETE CASCADE
    );
END
GO

-- Create Junction Table for CourseMaterial-Batch Many-to-Many relationship
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('MaterialBatches') AND type in (N'U'))
BEGIN
    CREATE TABLE MaterialBatches (
        MaterialId INT NOT NULL,
        BatchId INT NOT NULL,
        CONSTRAINT PK_MaterialBatches PRIMARY KEY (MaterialId, BatchId),
        CONSTRAINT FK_MaterialBatches_Materials FOREIGN KEY (MaterialId) REFERENCES CourseMaterials (MaterialId) ON DELETE CASCADE,
        CONSTRAINT FK_MaterialBatches_Batches FOREIGN KEY (BatchId) REFERENCES Batches (BatchId) ON DELETE CASCADE
    );
END
GO

-- Ensure EnrollmentId column in BatchEnrollments is named correctly (fix forTurn 5 error if needed)
-- If you see 'Enrollment_EnrollmentId' errors, it usually means EF is confused about the property mapping.
-- The following ensures the column exists as EnrollmentId.
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BatchEnrollments') AND name = 'EnrollmentId')
AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BatchEnrollments') AND name = 'Enrollment_EnrollmentId')
BEGIN
    EXEC sp_rename 'BatchEnrollments.Enrollment_EnrollmentId', 'EnrollmentId', 'COLUMN';
END
GO
