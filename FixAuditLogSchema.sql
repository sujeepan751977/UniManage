-- SQL Script to fix AuditLogs table schema
USE [UniManageDB_New];
GO

PRINT 'Fixing AuditLogs table schema...';

-- Drop the old table if it exists
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL 
    DROP TABLE dbo.AuditLogs;

-- Create the table with correct column names matching the AuditLog model
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

PRINT 'AuditLogs schema fixed successfully.';
GO
