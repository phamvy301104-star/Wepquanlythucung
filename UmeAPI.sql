Use UmeAPI

-------------------------------------------
-- 1 . Rồi check user mới đăng ký
SELECT * from AspNetUsers

-- 2. Thêm vai trò Admin vào bảng AspNetRoles (nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName)
    VALUES (NEWID(), 'Admin', 'ADMIN'),
	 (NEWID(), 'User', 'USER');
END

-- 3. Rồi check User 
SELECT * from AspNetUsers --Xong rồi nhớ copy id user rồi quăng xuống dưới

-- 4. Gán vai trò Admin cho người dùng
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('ef8a7cf3-c051-416c-8153-37904594397f', '959caab9-4cc1-449b-96de-8aa3b3c4ecff');

-- 5. Rồi check Role
SELECT * from AspNetRoles --Rồi copy lên trên

-- 6. Check xem có là admin chưa
SELECT * from AspNetUserRoles

-----------------------------------
-- CHẠY --
-- 1. dotnet ef database update
-- 2. Xoa bin, obj
-- 3. dotnet build
-- 4. dotnet restore
-- 5. set ASPNETCORE_URLS=https://localhost:5120
-- 6. dotnet run --launch-profile https 
-- cd 'c:\Users\ADMIN\source\LapTrinhTrenTBDD\nhom6_admin'

select * from [dbo].[ChatMessages];
select * from [dbo].[ChatSessions];
select * from [dbo].[Categories];
