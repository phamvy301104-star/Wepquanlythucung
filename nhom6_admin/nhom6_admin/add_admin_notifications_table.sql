-- Migration Script: Add AdminNotifications and update Notifications table
-- Database: UmeAPI
-- Date: 2026-01-13

-- Check if AdminNotifications table exists, if not create it
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminNotifications')
BEGIN
    CREATE TABLE [dbo].[AdminNotifications] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Type] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Content] NVARCHAR(1000) NULL,
        [Data] NVARCHAR(MAX) NULL,
        [ActionUrl] NVARCHAR(500) NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [ReadAt] DATETIME2 NULL,
        [RelatedEntityId] INT NULL,
        [RelatedEntityType] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    
    PRINT 'Created AdminNotifications table';
END
ELSE
BEGIN
    PRINT 'AdminNotifications table already exists';
END
GO

-- Create index for faster queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AdminNotifications_IsRead')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AdminNotifications_IsRead] 
    ON [dbo].[AdminNotifications] ([IsRead], [IsDeleted])
    INCLUDE ([Type], [Title], [CreatedAt]);
    
    PRINT 'Created index IX_AdminNotifications_IsRead';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AdminNotifications_CreatedAt')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AdminNotifications_CreatedAt] 
    ON [dbo].[AdminNotifications] ([CreatedAt] DESC)
    INCLUDE ([IsRead], [IsDeleted]);
    
    PRINT 'Created index IX_AdminNotifications_CreatedAt';
END
GO

-- Create Notifications table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Type] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Content] NVARCHAR(1000) NULL,
        [ActionUrl] NVARCHAR(500) NULL,
        [ReferenceType] NVARCHAR(50) NULL,
        [ReferenceId] INT NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [IsPushSent] BIT NOT NULL DEFAULT 0,
        [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Normal',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE NONCLUSTERED INDEX [IX_Notifications_UserId] 
    ON [dbo].[Notifications] ([UserId], [IsRead], [IsDeleted]);
    
    CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedAt] 
    ON [dbo].[Notifications] ([CreatedAt] DESC);
    
    PRINT 'Created Notifications table';
END
ELSE
BEGIN
    -- Add Priority column if table exists but column doesn't
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'Priority')
    BEGIN
        ALTER TABLE [dbo].[Notifications] ADD [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Normal';
        PRINT 'Added Priority column to Notifications';
    END
    ELSE
    BEGIN
        PRINT 'Notifications table already has Priority column';
    END
END
GO

-- Print summary
PRINT '=== Migration Summary ===';
SELECT 
    'AdminNotifications' AS TableName, 
    COUNT(*) AS RecordCount 
FROM [dbo].[AdminNotifications];

SELECT 
    'Notifications' AS TableName, 
    COUNT(*) AS RecordCount 
FROM [dbo].[Notifications];

PRINT 'Migration completed successfully!';
