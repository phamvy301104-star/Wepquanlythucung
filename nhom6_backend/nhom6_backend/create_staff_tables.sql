-- =============================================
-- Script: create_staff_tables.sql
-- Purpose: Create missing Staff Management tables
-- Tables: Attendances, SalarySlips, StaffChatRooms, StaffChatMessages, AdminNotifications
-- Date: 2026-01-14
-- =============================================

-- 1. AdminNotifications Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminNotifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminNotifications] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Type] nvarchar(50) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(1000) NULL,
        [Data] nvarchar(max) NULL,
        [ActionUrl] nvarchar(500) NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        [ReadAt] datetime2 NULL,
        [RelatedEntityId] int NULL,
        [RelatedEntityType] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_AdminNotifications] PRIMARY KEY ([Id])
    );
    PRINT 'Created table: AdminNotifications';
END
ELSE
    PRINT 'Table AdminNotifications already exists';
GO

-- 2. Attendances Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Attendances]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Attendances] (
        [Id] int NOT NULL IDENTITY(1,1),
        [StaffId] int NOT NULL,
        [WorkDate] datetime2 NOT NULL,
        [CheckIn1_Time] datetime2 NULL,
        [CheckIn1_PhotoUrl] nvarchar(500) NULL,
        [CheckIn2_Time] datetime2 NULL,
        [CheckIn2_PhotoUrl] nvarchar(500) NULL,
        [CheckIn3_Time] datetime2 NULL,
        [CheckIn3_PhotoUrl] nvarchar(500) NULL,
        [CheckIn4_Time] datetime2 NULL,
        [CheckIn4_PhotoUrl] nvarchar(500) NULL,
        [ScheduledStart] time NOT NULL,
        [ScheduledBreakStart] time NULL,
        [ScheduledBreakEnd] time NULL,
        [ScheduledEnd] time NOT NULL,
        [LateMinutes] int NOT NULL DEFAULT 0,
        [EarlyLeaveMinutes] int NOT NULL DEFAULT 0,
        [OverBreakMinutes] int NOT NULL DEFAULT 0,
        [TotalWorkMinutes] int NOT NULL DEFAULT 0,
        [OvertimeMinutes] int NOT NULL DEFAULT 0,
        [CheckCount] int NOT NULL DEFAULT 0,
        [LatePenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [OverBreakPenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [EarlyLeavePenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [MissedCheckPenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [TotalPenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [Status] nvarchar(20) NOT NULL DEFAULT 'Pending',
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Attendances_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: Attendances';
    
    -- Create indexes
    CREATE UNIQUE INDEX [IX_Attendances_StaffId_WorkDate] ON [Attendances]([StaffId], [WorkDate]);
    CREATE INDEX [IX_Attendances_Status] ON [Attendances]([Status]);
    CREATE INDEX [IX_Attendances_WorkDate] ON [Attendances]([WorkDate]);
END
ELSE
    PRINT 'Table Attendances already exists';
GO

-- 3. SalarySlips Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalarySlips]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SalarySlips] (
        [Id] int NOT NULL IDENTITY(1,1),
        [StaffId] int NOT NULL,
        [Month] int NOT NULL,
        [Year] int NOT NULL,
        [WorkDays] int NOT NULL DEFAULT 26,
        [ActualWorkDays] int NOT NULL DEFAULT 0,
        [PaidLeaveDays] int NOT NULL DEFAULT 0,
        [UnpaidLeaveDays] int NOT NULL DEFAULT 0,
        [TotalLateMinutes] int NOT NULL DEFAULT 0,
        [LateCount] int NOT NULL DEFAULT 0,
        [MissedCheckDays] int NOT NULL DEFAULT 0,
        [TotalOvertimeMinutes] int NOT NULL DEFAULT 0,
        [BaseSalary] decimal(18,2) NOT NULL DEFAULT 0,
        [OvertimeBonus] decimal(18,2) NOT NULL DEFAULT 0,
        [CommissionBonus] decimal(18,2) NOT NULL DEFAULT 0,
        [OtherAllowance] decimal(18,2) NOT NULL DEFAULT 0,
        [GrossIncome] decimal(18,2) NOT NULL DEFAULT 0,
        [LatePenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [MissedCheckPenalty] decimal(18,2) NOT NULL DEFAULT 0,
        [AbsentDeduction] decimal(18,2) NOT NULL DEFAULT 0,
        [BHXH] decimal(18,2) NOT NULL DEFAULT 0,
        [BHYT] decimal(18,2) NOT NULL DEFAULT 0,
        [BHTN] decimal(18,2) NOT NULL DEFAULT 0,
        [OtherDeduction] decimal(18,2) NOT NULL DEFAULT 0,
        [TotalDeductions] decimal(18,2) NOT NULL DEFAULT 0,
        [NetSalary] decimal(18,2) NOT NULL DEFAULT 0,
        [Status] nvarchar(20) NOT NULL DEFAULT 'Draft',
        [ConfirmedAt] datetime2 NULL,
        [ConfirmedBy] nvarchar(100) NULL,
        [PaidAt] datetime2 NULL,
        [PaymentMethod] nvarchar(50) NULL,
        [TransactionId] nvarchar(100) NULL,
        [Note] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_SalarySlips] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalarySlips_Staff_StaffId] FOREIGN KEY ([StaffId]) REFERENCES [Staff]([Id]) ON DELETE CASCADE
    );
    PRINT 'Created table: SalarySlips';
    
    -- Create indexes
    CREATE UNIQUE INDEX [IX_SalarySlips_StaffId_Month_Year] ON [SalarySlips]([StaffId], [Month], [Year]);
    CREATE INDEX [IX_SalarySlips_Month_Year] ON [SalarySlips]([Month], [Year]);
    CREATE INDEX [IX_SalarySlips_Status] ON [SalarySlips]([Status]);
END
ELSE
    PRINT 'Table SalarySlips already exists';
GO

-- 4. StaffChatRooms Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StaffChatRooms]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StaffChatRooms] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Staff1Id] int NOT NULL,
        [Staff2Id] int NOT NULL,
        [LastMessageText] nvarchar(200) NULL,
        [LastMessageSenderId] int NULL,
        [LastMessageAt] datetime2 NULL,
        [Staff1UnreadCount] int NOT NULL DEFAULT 0,
        [Staff2UnreadCount] int NOT NULL DEFAULT 0,
        [Staff1Muted] bit NOT NULL DEFAULT 0,
        [Staff2Muted] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_StaffChatRooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffChatRooms_Staff_Staff1Id] FOREIGN KEY ([Staff1Id]) REFERENCES [Staff]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StaffChatRooms_Staff_Staff2Id] FOREIGN KEY ([Staff2Id]) REFERENCES [Staff]([Id]) ON DELETE NO ACTION
    );
    PRINT 'Created table: StaffChatRooms';
    
    -- Create indexes
    CREATE UNIQUE INDEX [IX_StaffChatRooms_Staff1Id_Staff2Id] ON [StaffChatRooms]([Staff1Id], [Staff2Id]);
    CREATE INDEX [IX_StaffChatRooms_Staff2Id] ON [StaffChatRooms]([Staff2Id]);
    CREATE INDEX [IX_StaffChatRooms_LastMessageAt] ON [StaffChatRooms]([LastMessageAt]);
END
ELSE
    PRINT 'Table StaffChatRooms already exists';
GO

-- 5. StaffChatMessages Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StaffChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StaffChatMessages] (
        [Id] int NOT NULL IDENTITY(1,1),
        [ChatRoomId] int NOT NULL,
        [SenderId] int NOT NULL,
        [Content] nvarchar(2000) NOT NULL,
        [MessageType] nvarchar(20) NOT NULL DEFAULT 'Text',
        [AttachmentUrl] nvarchar(500) NULL,
        [FileName] nvarchar(255) NULL,
        [FileSize] bigint NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        [ReadAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_StaffChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StaffChatMessages_StaffChatRooms_ChatRoomId] FOREIGN KEY ([ChatRoomId]) REFERENCES [StaffChatRooms]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StaffChatMessages_Staff_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Staff]([Id]) ON DELETE NO ACTION
    );
    PRINT 'Created table: StaffChatMessages';
    
    -- Create indexes
    CREATE INDEX [IX_StaffChatMessages_ChatRoomId_CreatedAt] ON [StaffChatMessages]([ChatRoomId], [CreatedAt]);
    CREATE INDEX [IX_StaffChatMessages_SenderId] ON [StaffChatMessages]([SenderId]);
    CREATE INDEX [IX_StaffChatMessages_IsRead] ON [StaffChatMessages]([IsRead]);
END
ELSE
    PRINT 'Table StaffChatMessages already exists';
GO

-- 6. Insert migration history record if not exists
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260113142213_AddStaffFeatures')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260113142213_AddStaffFeatures', N'9.0.0');
    PRINT 'Added migration history: 20260113142213_AddStaffFeatures';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'Staff tables creation completed!';
PRINT '=============================================';
