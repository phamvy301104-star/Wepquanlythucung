-- Add missing columns to Appointments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'CheckInTime')
BEGIN
    ALTER TABLE [Appointments] ADD [CheckInTime] datetime2 NULL;
    PRINT 'Added CheckInTime column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'CheckOutTime')
BEGIN
    ALTER TABLE [Appointments] ADD [CheckOutTime] datetime2 NULL;
    PRINT 'Added CheckOutTime column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'IsReviewed')
BEGIN
    ALTER TABLE [Appointments] ADD [IsReviewed] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsReviewed column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'ParentAppointmentId')
BEGIN
    ALTER TABLE [Appointments] ADD [ParentAppointmentId] int NULL;
    PRINT 'Added ParentAppointmentId column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Appointments]') AND name = 'RecurrenceType')
BEGIN
    ALTER TABLE [Appointments] ADD [RecurrenceType] nvarchar(20) NOT NULL DEFAULT 'None';
    PRINT 'Added RecurrenceType column';
END

PRINT 'Script completed!';
