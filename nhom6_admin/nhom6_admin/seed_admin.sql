-- Create Admin Role
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
    PRINT 'Admin role created';
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Staff')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Staff', 'STAFF', NEWID());
    PRINT 'Staff role created';
END

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Customer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Customer', 'CUSTOMER', NEWID());
    PRINT 'Customer role created';
END

-- Create Admin User
-- Password: @Yasuo123 (hashed using ASP.NET Identity V3 format)
DECLARE @AdminUserId NVARCHAR(450) = NEWID();
DECLARE @AdminEmail NVARCHAR(256) = 'tkvi9a4@gmail.com';

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @AdminEmail)
BEGIN
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail,
        EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
        PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount,
        FullName, IsActive, CreatedAt
    )
    VALUES (
        @AdminUserId,
        @AdminEmail,
        UPPER(@AdminEmail),
        @AdminEmail,
        UPPER(@AdminEmail),
        1,
        'AQAAAAIAAYagAAAAELfYPz4lBnYdTqC9WmQZpS5tR5F+dF5J5hE5QxYhIkXo3Lrk5ZbXtDpE1234567890=',
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        N'MinhVi',
        1,
        GETDATE()
    );
    PRINT 'Admin user created';

    -- Assign Admin role
    DECLARE @AdminRoleId NVARCHAR(450);
    SELECT @AdminRoleId = Id FROM AspNetRoles WHERE Name = 'Admin';
    
    IF @AdminRoleId IS NOT NULL
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@AdminUserId, @AdminRoleId);
        PRINT 'Admin role assigned to user';
    END
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END

-- Verify
SELECT 'Users:' AS Info;
SELECT Email, FullName, IsActive FROM AspNetUsers;

SELECT 'Roles:' AS Info;
SELECT Name FROM AspNetRoles;
