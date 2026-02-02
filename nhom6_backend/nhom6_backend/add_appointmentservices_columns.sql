-- Add missing columns to AppointmentServices table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'PerformedByStaffId')
BEGIN
    ALTER TABLE [AppointmentServices] ADD [PerformedByStaffId] int NULL;
    PRINT 'Added PerformedByStaffId column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'Price')
BEGIN
    ALTER TABLE [AppointmentServices] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added Price column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'ServiceName')
BEGIN
    ALTER TABLE [AppointmentServices] ADD [ServiceName] nvarchar(200) NULL;
    PRINT 'Added ServiceName column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'ServiceOrder')
BEGIN
    ALTER TABLE [AppointmentServices] ADD [ServiceOrder] int NOT NULL DEFAULT 0;
    PRINT 'Added ServiceOrder column';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'Status')
BEGIN
    ALTER TABLE [AppointmentServices] ADD [Status] nvarchar(20) NOT NULL DEFAULT 'Pending';
    PRINT 'Added Status column';
END

PRINT 'AppointmentServices columns added!';
