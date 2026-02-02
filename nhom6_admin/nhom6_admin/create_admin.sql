-- Tạo tài khoản Admin cho UME Salon
-- Password: Admin@123 (hashed với ASP.NET Identity)

SET QUOTED_IDENTIFIER ON
GO

-- Đầu tiên tạo role Admin nếu chưa có
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Staff')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Staff', 'STAFF', NEWID())
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Customer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Customer', 'CUSTOMER', NEWID())
END

-- Tạo user admin nếu chưa có
DECLARE @AdminId NVARCHAR(450) = NEWID()
DECLARE @AdminRoleId NVARCHAR(450)

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@umesalon.com')
BEGIN
    -- Password hash cho 'Admin@123' - Tạo bằng ASP.NET Identity
    -- Lưu ý: Hash này được tạo bằng ASP.NET Identity hasher
    INSERT INTO AspNetUsers (
        Id, 
        UserName, 
        NormalizedUserName, 
        Email, 
        NormalizedEmail, 
        EmailConfirmed, 
        PasswordHash,
        SecurityStamp, 
        ConcurrencyStamp, 
        PhoneNumberConfirmed, 
        TwoFactorEnabled, 
        LockoutEnabled, 
        AccessFailedCount,
        FullName,
        IsActive,
        CreatedAt
    )
    VALUES (
        @AdminId,
        'admin@umesalon.com',
        'ADMIN@UMESALON.COM',
        'admin@umesalon.com',
        'ADMIN@UMESALON.COM',
        1,
        'AQAAAAIAAYagAAAAEJhv9lh4o3gE7KI8z5uHDKJU7sO2E0L8KTBM9VlC5oH1bPJHHPM+jQMDXEFvF5V1zQ==', -- Admin@123
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        'Admin UME',
        1,
        GETDATE()
    )
    
    -- Gán role Admin cho user
    SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'
    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AdminId, @AdminRoleId)
    
    PRINT 'Admin account created successfully!'
END
ELSE
BEGIN
    PRINT 'Admin account already exists'
END

-- Hiển thị kết quả
SELECT Id, Email, UserName, FullName, IsActive FROM AspNetUsers WHERE Email = 'admin@umesalon.com'
