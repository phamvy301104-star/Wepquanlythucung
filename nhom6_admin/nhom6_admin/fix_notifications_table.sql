-- Fix Notifications Table - Add missing columns
-- Database: UmeAPI
-- Date: 2026-01-13
-- Run this script in SQL Server Management Studio

USE [UmeAPI]
GO

PRINT '=== Starting Notifications Table Fix ==='

-- Add ExpiresAt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'ExpiresAt')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [ExpiresAt] DATETIME2 NULL;
    PRINT 'Added ExpiresAt column';
END
ELSE
BEGIN
    PRINT 'ExpiresAt column already exists';
END
GO

-- Add ImageUrl column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'ImageUrl')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [ImageUrl] NVARCHAR(500) NULL;
    PRINT 'Added ImageUrl column';
END
ELSE
BEGIN
    PRINT 'ImageUrl column already exists';
END
GO

-- Add IsEmailSent column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'IsEmailSent')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [IsEmailSent] BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsEmailSent column';
END
ELSE
BEGIN
    PRINT 'IsEmailSent column already exists';
END
GO

-- Add Metadata column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'Metadata')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [Metadata] NVARCHAR(MAX) NULL;
    PRINT 'Added Metadata column';
END
ELSE
BEGIN
    PRINT 'Metadata column already exists';
END
GO

-- Add PushSentAt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'PushSentAt')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [PushSentAt] DATETIME2 NULL;
    PRINT 'Added PushSentAt column';
END
ELSE
BEGIN
    PRINT 'PushSentAt column already exists';
END
GO

-- Add ReadAt column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'ReadAt')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [ReadAt] DATETIME2 NULL;
    PRINT 'Added ReadAt column';
END
ELSE
BEGIN
    PRINT 'ReadAt column already exists';
END
GO

-- Add SenderUserId column with foreign key
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'SenderUserId')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [SenderUserId] NVARCHAR(450) NULL;
    PRINT 'Added SenderUserId column';
    
    -- Add foreign key constraint (optional - may fail if data integrity issues)
    BEGIN TRY
        ALTER TABLE [dbo].[Notifications] 
        ADD CONSTRAINT [FK_Notifications_SenderUser] 
        FOREIGN KEY ([SenderUserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);
        PRINT 'Added foreign key FK_Notifications_SenderUser';
    END TRY
    BEGIN CATCH
        PRINT 'Warning: Could not add FK_Notifications_SenderUser - ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'SenderUserId column already exists';
END
GO

-- Verify all columns exist
PRINT ''
PRINT '=== Verification ==='
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID('Notifications')
ORDER BY c.column_id;

PRINT ''
PRINT '=== Fix completed successfully! ==='
