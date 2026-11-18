/* 
   Advanced Batch Management & Content Release Schema Update
   This script creates the formal junction tables for lecturer-batch assignments
   and batch-specific content release.
*/

-- 1. Drop old Many-to-Many junction tables if they exist from previous simplified implementation
IF OBJECT_ID('dbo.AssignmentBatches', 'U') IS NOT NULL DROP TABLE dbo.AssignmentBatches;
IF OBJECT_ID('dbo.MaterialBatches', 'U') IS NOT NULL DROP TABLE dbo.MaterialBatches;

-- 2. Create BatchLecturers Table
IF OBJECT_ID('dbo.BatchLecturers', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BatchLecturers] (
        [BatchLecturerId] INT IDENTITY (1, 1) NOT NULL,
        [BatchId]         INT NOT NULL,
        [LecturerId]      NVARCHAR (128) NOT NULL,
        [IsPrimary]       BIT NOT NULL DEFAULT 1,
        [AssignedAt]      DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [AssignedById]    NVARCHAR (128) NULL,
        CONSTRAINT [PK_dbo.BatchLecturers] PRIMARY KEY CLUSTERED ([BatchLecturerId] ASC),
        CONSTRAINT [FK_dbo.BatchLecturers_dbo.Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batches] ([BatchId]),
        CONSTRAINT [FK_dbo.BatchLecturers_dbo.AspNetUsers_LecturerId] FOREIGN KEY ([LecturerId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
        CONSTRAINT [FK_dbo.BatchLecturers_dbo.AspNetUsers_AssignedById] FOREIGN KEY ([AssignedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );
    CREATE UNIQUE INDEX [IX_BatchId_LecturerId] ON [dbo].[BatchLecturers] ([BatchId], [LecturerId]);
END

-- 3. Create AssignmentReleases Table
IF OBJECT_ID('dbo.AssignmentReleases', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AssignmentReleases] (
        [ReleaseId]       INT IDENTITY (1, 1) NOT NULL,
        [AssignmentId]    INT NOT NULL,
        [BatchId]         INT NOT NULL,
        [ReleasedAt]      DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [CustomDeadline]  DATETIME NULL,
        [IsVisible]       BIT NOT NULL DEFAULT 1,
        [ReleasedById]    NVARCHAR (128) NULL,
        CONSTRAINT [PK_dbo.AssignmentReleases] PRIMARY KEY CLUSTERED ([ReleaseId] ASC),
        CONSTRAINT [FK_dbo.AssignmentReleases_dbo.Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignments] ([AssignmentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_dbo.AssignmentReleases_dbo.Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batches] ([BatchId]),
        CONSTRAINT [FK_dbo.AssignmentReleases_dbo.AspNetUsers_ReleasedById] FOREIGN KEY ([ReleasedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );
    CREATE UNIQUE INDEX [IX_AssignmentId_BatchId] ON [dbo].[AssignmentReleases] ([AssignmentId], [BatchId]);
END

-- 4. Create MaterialReleases Table
IF OBJECT_ID('dbo.MaterialReleases', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MaterialReleases] (
        [ReleaseId]       INT IDENTITY (1, 1) NOT NULL,
        [MaterialId]      INT NOT NULL,
        [BatchId]         INT NOT NULL,
        [ReleasedAt]      DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [IsVisible]       BIT NOT NULL DEFAULT 1,
        [ReleasedById]    NVARCHAR (128) NULL,
        CONSTRAINT [PK_dbo.MaterialReleases] PRIMARY KEY CLUSTERED ([ReleaseId] ASC),
        CONSTRAINT [FK_dbo.MaterialReleases_dbo.Materials_MaterialId] FOREIGN KEY ([MaterialId]) REFERENCES [dbo].[CourseMaterials] ([MaterialId]) ON DELETE CASCADE,
        CONSTRAINT [FK_dbo.MaterialReleases_dbo.Batches_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batches] ([BatchId]),
        CONSTRAINT [FK_dbo.MaterialReleases_dbo.AspNetUsers_ReleasedById] FOREIGN KEY ([ReleasedById]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );
    CREATE UNIQUE INDEX [IX_MaterialId_BatchId] ON [dbo].[MaterialReleases] ([MaterialId], [BatchId]);
END

-- 5. Cleanup: Migrate existing single-lecturer assignments to the new table if applicable, then drop column
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'LecturerId' AND Object_ID = OBJECT_ID('dbo.Batches'))
BEGIN
    -- Optional: Migrate existing lecturer data to the new junction table
    INSERT INTO [dbo].[BatchLecturers] ([BatchId], [LecturerId], [IsPrimary], [AssignedById])
    SELECT [BatchId], [LecturerId], 1, [CreatedById]
    FROM [dbo].[Batches] b
    WHERE [LecturerId] IS NOT NULL 
    AND NOT EXISTS (SELECT 1 FROM [dbo].[BatchLecturers] bl WHERE bl.BatchId = b.BatchId AND bl.LecturerId = b.LecturerId);

    -- IMPORTANT: Drop constraints first if any
    DECLARE @ConstraintName nvarchar(200)
    SELECT @ConstraintName = Name FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.Batches') AND referenced_object_id = OBJECT_ID('dbo.AspNetUsers')
    IF @ConstraintName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE dbo.Batches DROP CONSTRAINT ' + @ConstraintName)
    END

    -- Drop the column
    ALTER TABLE [dbo].[Batches] DROP COLUMN [LecturerId];
END
GO
