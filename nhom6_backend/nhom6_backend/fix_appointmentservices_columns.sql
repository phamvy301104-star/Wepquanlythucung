-- Drop redundant columns from AppointmentServices
-- UnitPrice and TotalPrice are old columns, now using Price instead

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'UnitPrice')
BEGIN
    ALTER TABLE [AppointmentServices] DROP COLUMN [UnitPrice];
    PRINT 'Dropped UnitPrice column';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AppointmentServices]') AND name = 'TotalPrice')
BEGIN
    ALTER TABLE [AppointmentServices] DROP COLUMN [TotalPrice];
    PRINT 'Dropped TotalPrice column';
END

PRINT 'Cleanup completed!';
