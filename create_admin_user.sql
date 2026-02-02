-- ===================================================
-- TẠO TÀI KHOẢN ADMIN CHO UME SALON
-- ===================================================
-- Email: admin@umesalon.com
-- Password: Admin@123 (được hash bằng ASP.NET Identity)
-- ===================================================

USE UmeAPI;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- 1. Tạo các Role nếu chưa có
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID())
    PRINT 'Role Admin created'
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Staff')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Staff', 'STAFF', NEWID())
    PRINT 'Role Staff created'
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Customer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Customer', 'CUSTOMER', NEWID())
    PRINT 'Role Customer created'
END

-- 2. Xóa admin cũ nếu tồn tại
IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@umesalon.com')
BEGIN
    DELETE FROM AspNetUserRoles 
    WHERE UserId = (SELECT Id FROM AspNetUsers WHERE Email = 'admin@umesalon.com')
    
    DELETE FROM AspNetUsers 
    WHERE Email = 'admin@umesalon.com'
    
    PRINT 'Deleted old admin account'
END

-- 3. Tạo admin user mới
DECLARE @AdminId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450))
DECLARE @AdminRoleId NVARCHAR(450)

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
    'AQAAAAIAAYagAAAAEJhv9lh4o3gE7KI8z5uHDKJU7sO2E0L8KTBM9VlC5oH1bPJHHPM+jQMDXEFvF5V1zQ==',
    CAST(NEWID() AS NVARCHAR(450)),
    CAST(NEWID() AS NVARCHAR(450)),
    0,
    0,
    1,
    0,
    'Administrator UME',
    1,
    GETDATE()
)

-- 4. Gán role Admin cho user
SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin'
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES (@AdminId, @AdminRoleId)

PRINT '✅ Admin account created successfully!'
PRINT 'Email: admin@umesalon.com'
PRINT 'Password: Admin@123'

-- 5. Kiểm tra kết quả
SELECT 
    'Admin Account Info' AS [Info],
    u.Id,
    u.Email,
    u.FullName,
    u.IsActive,
    r.Name AS Role
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@umesalon.com'
