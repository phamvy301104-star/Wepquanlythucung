-- Add missing columns to Services table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'IncludedServices')
BEGIN
    ALTER TABLE [Services] ADD [IncludedServices] nvarchar(max) NULL;
    PRINT 'Added IncludedServices column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Notes')
BEGIN
    ALTER TABLE [Services] ADD [Notes] nvarchar(500) NULL;
    PRINT 'Added Notes column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Tags')
BEGIN
    ALTER TABLE [Services] ADD [Tags] nvarchar(500) NULL;
    PRINT 'Added Tags column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Services]') AND name = 'Warnings')
BEGIN
    ALTER TABLE [Services] ADD [Warnings] nvarchar(500) NULL;
    PRINT 'Added Warnings column';
END

PRINT 'Script completed!';
GO
