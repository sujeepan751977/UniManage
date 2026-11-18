-- SQL Script to fix corrupted password hashes
USE [UniManageDB_New];
GO

PRINT 'Fixing corrupted password hashes...';

-- Update admin@unimanage.edu
UPDATE [dbo].[AspNetUsers]
SET [PasswordHash] = 'AO9tGQZ1D70rBuev9dgIS3sEa54bCauFra+LtlMEApnt5nUWOITCLcDcPzg3z46W0A=='
WHERE [Email] = 'admin@unimanage.edu';

-- Update lecturer@unimanage.edu
UPDATE [dbo].[AspNetUsers]
SET [PasswordHash] = 'AO9tGQZ1D70rBuev9dgIS3sEa54bCauFra+LtlMEApnt5nUWOITCLcDcPzg3z46W0A=='
WHERE [Email] = 'lecturer@unimanage.edu';

PRINT 'Password hashes updated successfully.';
GO
