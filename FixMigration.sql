/* 
  UNI-MANAGE DATABASE FIX SCRIPT (DYNAMIC VERSION)
  This version uses EXEC to handle columns that don't exist yet.
*/

-- 1. Create the Modules table if missing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Modules]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Modules] (
        [ModuleId] [int] IDENTITY(1,1) NOT NULL,
        [ModuleCode] [nvarchar](20) NOT NULL,
        [Title] [nvarchar](150) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [CourseId] [int] NOT NULL,
        [LecturerId] [nvarchar](128) NOT NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_dbo.Modules] PRIMARY KEY ([ModuleId])
    )
    PRINT 'Created [Modules] table.';
END

-- 2. Ensure ModuleId column exists (Nullable for now)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Assignments]') AND name = 'ModuleId')
BEGIN
    ALTER TABLE [dbo].[Assignments] ADD [ModuleId] int NULL;
    PRINT 'Added [ModuleId] to [Assignments].';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CourseMaterials]') AND name = 'ModuleId')
BEGIN
    ALTER TABLE [dbo].[CourseMaterials] ADD [ModuleId] int NULL;
    PRINT 'Added [ModuleId] to [CourseMaterials].';
END

-- 3. Create default modules for courses
DECLARE @DefaultUserId nvarchar(128);
SELECT TOP 1 @DefaultUserId = Id FROM [dbo].[AspNetUsers] WHERE Email LIKE '%lecturer%' OR Email LIKE '%admin%' ORDER BY Email;
IF @DefaultUserId IS NULL SET @DefaultUserId = '00000000-0000-0000-0000-000000000000';

INSERT INTO [dbo].[Modules] (ModuleCode, Title, Description, CourseId, LecturerId, CreatedAt)
SELECT c.CourseCode + '-MOD', 'Core Module', 'Fixing migration', c.CourseId, @DefaultUserId, GETDATE()
FROM [dbo].[Courses] c
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Modules] m WHERE m.CourseId = c.CourseId)
AND (
    EXISTS (SELECT 1 FROM [dbo].[Assignments] a WHERE a.CourseId = c.CourseId)
    OR EXISTS (SELECT 1 FROM [dbo].[CourseMaterials] mat WHERE mat.CourseId = c.CourseId)
)
PRINT 'Generated default modules.';

-- 4. Dynamic Update for Assignments
EXEC sp_executesql N'
UPDATE a
SET a.ModuleId = m.ModuleId
FROM [dbo].[Assignments] a
JOIN [dbo].[Modules] m ON m.CourseId = a.CourseId
WHERE a.ModuleId IS NULL OR a.ModuleId = 0
';
PRINT 'Linked Assignments.';

-- 5. Dynamic Update for Materials
EXEC sp_executesql N'
UPDATE mat
SET mat.ModuleId = m.ModuleId
FROM [dbo].[CourseMaterials] mat
JOIN [dbo].[Modules] m ON m.CourseId = mat.CourseId
WHERE mat.ModuleId IS NULL OR mat.ModuleId = 0
';
PRINT 'Linked Materials.';
