-- =============================================
-- Script: Clean Database - Remove Fake Data
-- Database: UmeAPI
-- Date: 2026-01-12
-- Description: Xóa dữ liệu ảo, giữ lại 7 dịch vụ thực
-- =============================================

PRINT '🧹 Bắt đầu dọn dẹp dữ liệu...';
GO

-- ==================================================
-- 1. XÓA TẤT CẢ DỊCH VỤ CŨ
-- ==================================================
PRINT '📌 Đang xóa tất cả dịch vụ cũ...';

-- Xóa ServiceReviews liên quan
DELETE FROM [ServiceReviews] WHERE [ServiceId] IN (SELECT [Id] FROM [Services]);
PRINT '   ✓ Đã xóa ServiceReviews';

-- Xóa AppointmentServices liên quan
DELETE FROM [AppointmentServices] WHERE [ServiceId] IN (SELECT [Id] FROM [Services]);
PRINT '   ✓ Đã xóa AppointmentServices';

-- Xóa StaffServices liên quan
DELETE FROM [StaffServices] WHERE [ServiceId] IN (SELECT [Id] FROM [Services]);
PRINT '   ✓ Đã xóa StaffServices';

-- Xóa ServiceImages liên quan
DELETE FROM [ServiceImages] WHERE [ServiceId] IN (SELECT [Id] FROM [Services]);
PRINT '   ✓ Đã xóa ServiceImages';

-- Xóa tất cả Services
DELETE FROM [Services];
PRINT '   ✓ Đã xóa tất cả Services';
GO

-- ==================================================
-- 2. THÊM 7 DỊCH VỤ THỰC
-- ==================================================
PRINT '📌 Đang thêm 7 dịch vụ cắt tóc nam...';

-- Reset Identity (nếu cần)
DBCC CHECKIDENT ('[Services]', RESEED, 0);
GO

-- 1. Cắt Tóc Nam Cơ Bản
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV001', N'Cắt Tóc Nam Cơ Bản', 'cat-toc-nam-co-ban',
    N'Cắt tóc nam kiểu cơ bản, tạo kiểu đơn giản, phù hợp mọi lứa tuổi',
    N'<p>Dịch vụ cắt tóc nam cơ bản bao gồm:</p><ul><li>Tư vấn kiểu tóc phù hợp với khuôn mặt</li><li>Cắt tóc bằng kéo và tông đơ chuyên nghiệp</li><li>Tạo kiểu cơ bản</li><li>Gội đầu sạch sẽ</li></ul><p>Thời gian: 30 phút</p>',
    'https://images.unsplash.com/photo-1622286342621-4bd786c2447c?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    100000.00, 100000.00, 100000.00, 100000.00,
    30, 10, 1, 'Male', 0, 2,
    1, 1, 0, 1, 1,
    4.8, 150, 450,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV001 - Cắt Tóc Nam Cơ Bản';

-- 2. Nhuộm Tóc Nam
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV002', N'Nhuộm Tóc Nam', 'nhuom-toc-nam',
    N'Nhuộm tóc thời trang, phủ bạc tự nhiên với thuốc nhuộm cao cấp nhập khẩu',
    N'<p>Dịch vụ nhuộm tóc nam chuyên nghiệp:</p><ul><li>Tư vấn màu phù hợp với da và phong cách</li><li>Sử dụng thuốc nhuộm cao cấp an toàn</li><li>Phủ bạc tự nhiên hoặc nhuộm màu thời trang</li><li>Gội sạch và dưỡng tóc sau nhuộm</li><li>Bảo hành màu 30 ngày</li></ul><p>Thời gian: 90 phút</p>',
    'https://images.unsplash.com/photo-1621605815971-fbc98d665033?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    250000.00, 280000.00, 250000.00, 350000.00,
    90, 15, 1, 'Male', 4, 4,
    1, 1, 0, 1, 2,
    4.7, 98, 320,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV002 - Nhuộm Tóc Nam';

-- 3. Gội Đầu Massage Cổ Vai
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV003', N'Gội Đầu Massage Cổ Vai', 'goi-dau-massage-co-vai',
    N'Gội đầu dưỡng sinh kết hợp massage thư giãn cổ vai, giảm stress hiệu quả',
    N'<p>Liệu trình chăm sóc thư giãn toàn diện:</p><ul><li>Gội đầu bằng dầu gội cao cấp</li><li>Massage da đầu kích thích tuần hoàn</li><li>Massage cổ vai giảm đau mỏi</li><li>Xông hơi thư giãn</li><li>Sấy tạo kiểu cơ bản</li></ul><p>Thời gian: 30 phút</p>',
    'https://images.unsplash.com/photo-1519415510236-718bdfcd89c8?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    80000.00, 80000.00, 80000.00, 80000.00,
    30, 5, 1, 'Male', 0, 1,
    0, 1, 0, 1, 3,
    4.9, 210, 580,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV003 - Gội Đầu Massage Cổ Vai';

-- 4. Uốn/Duỗi Tóc Nam
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV004', N'Uốn/Duỗi Tóc Nam', 'uon-duoi-toc-nam',
    N'Uốn xoăn tự nhiên hoặc duỗi thẳng mượt với công nghệ Hàn Quốc',
    N'<p>Dịch vụ tạo kiểu tóc chuyên sâu:</p><ul><li>Tư vấn kiểu tóc phù hợp</li><li>Công nghệ uốn/duỗi Hàn Quốc</li><li>Thuốc uốn/duỗi cao cấp không gây hư tổn</li><li>Dưỡng phục hồi tóc sau làm</li><li>Hướng dẫn chăm sóc tại nhà</li></ul><p>Thời gian: 120 phút</p>',
    'https://images.unsplash.com/photo-1620331311520-246422fd82f9?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    300000.00, 350000.00, 300000.00, 400000.00,
    120, 20, 1, 'Male', 8, 12,
    1, 0, 1, 1, 4,
    4.6, 75, 180,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV004 - Uốn/Duỗi Tóc Nam';

-- 5. Cạo Râu Chuyên Nghiệp
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV005', N'Cạo Râu Chuyên Nghiệp', 'cao-rau-chuyen-nghiep',
    N'Cạo râu kiểu truyền thống với dao cạo, kết hợp chăm sóc da mặt nam',
    N'<p>Trải nghiệm cạo râu đẳng cấp:</p><ul><li>Làm sạch da mặt</li><li>Xông hơi mở lỗ chân lông</li><li>Cạo râu bằng dao cạo truyền thống</li><li>Massage mặt thư giãn</li><li>Đắp mặt nạ dưỡng da</li></ul><p>Thời gian: 20 phút</p>',
    'https://images.unsplash.com/photo-1503951914875-452162b0f3f1?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    50000.00, 50000.00, 50000.00, 50000.00,
    20, 5, 1, 'Male', 0, 1,
    0, 1, 0, 1, 5,
    4.8, 185, 520,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV005 - Cạo Râu Chuyên Nghiệp';

-- 6. Chăm Sóc Da Mặt Nam
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV006', N'Chăm Sóc Da Mặt Nam', 'cham-soc-da-mat-nam',
    N'Liệu trình chăm sóc da mặt chuyên sâu cho nam giới, làm sạch sâu và dưỡng da',
    N'<p>Quy trình chăm sóc da toàn diện:</p><ul><li>Phân tích da và tư vấn</li><li>Tẩy tế bào chết</li><li>Làm sạch sâu lỗ chân lông</li><li>Massage mặt lymphatic</li><li>Đắp mặt nạ dưỡng chất</li><li>Dưỡng ẩm và chống lão hóa</li></ul><p>Thời gian: 60 phút</p>',
    'https://images.unsplash.com/photo-1570172619644-dfd03ed5d881?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    200000.00, 220000.00, 200000.00, 250000.00,
    60, 10, 1, 'Male', 2, 3,
    1, 0, 1, 1, 6,
    4.7, 62, 145,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV006 - Chăm Sóc Da Mặt Nam';

-- 7. Combo Premium: Cắt + Gội + Cạo
INSERT INTO [Services] (
    [ServiceCode], [Name], [Slug], [ShortDescription], [Description], [ImageUrl], 
    [GalleryImages], [VideoUrl], [ServiceCategoryId], [Price], [OriginalPrice], 
    [MinPrice], [MaxPrice], [DurationMinutes], [BufferMinutes], [RequiredStaff], 
    [Gender], [RequiredAdvanceBookingHours], [CancellationHours], [IsFeatured], 
    [IsPopular], [IsNew], [IsActive], [DisplayOrder], [AverageRating], 
    [TotalReviews], [TotalBookings], [CreatedAt], [UpdatedAt], [IsDeleted]
)
VALUES (
    'SV007', N'Combo Premium: Cắt + Gội + Cạo', 'combo-premium-cat-goi-cao',
    N'Trọn gói dịch vụ cao cấp: Cắt tóc + Gội đầu massage + Cạo râu chuyên nghiệp',
    N'<p>Combo tiết kiệm và trọn vẹn nhất:</p><ul><li><strong>Cắt tóc nam:</strong> Tư vấn và cắt kiểu</li><li><strong>Gội đầu massage:</strong> Thư giãn toàn diện</li><li><strong>Cạo râu:</strong> Kiểu truyền thống sang trọng</li><li><strong>Tạo kiểu:</strong> Wax/gel tạo phom</li></ul><p>Thời gian: 60 phút | Tiết kiệm 50.000đ</p>',
    'https://images.unsplash.com/photo-1585747860715-2ba37e788b70?w=800&h=600&fit=crop',
    NULL, NULL, NULL,
    180000.00, 230000.00, 180000.00, 180000.00,
    60, 10, 1, 'Male', 2, 4,
    1, 1, 1, 1, 7,
    5.0, 280, 720,
    GETDATE(), GETDATE(), 0
);
PRINT '   ✓ SV007 - Combo Premium';

GO

-- ==================================================
-- 3. VERIFY KẾT QUẢ
-- ==================================================
PRINT '';
PRINT '✅ Hoàn thành! Kết quả:';
PRINT '================================================';

SELECT 
    ServiceCode AS 'Mã DV',
    Name AS 'Tên Dịch Vụ',
    FORMAT(Price, 'N0') + 'đ' AS 'Giá',
    CAST(DurationMinutes AS VARCHAR) + ' phút' AS 'Thời Gian',
    CASE WHEN IsFeatured = 1 THEN 'Có' ELSE '' END AS 'Nổi Bật',
    CASE WHEN IsPopular = 1 THEN 'Có' ELSE '' END AS 'Phổ Biến',
    CASE WHEN IsNew = 1 THEN 'Có' ELSE '' END AS 'Mới'
FROM [Services]
WHERE ServiceCode IN ('SV001', 'SV002', 'SV003', 'SV004', 'SV005', 'SV006', 'SV007')
ORDER BY DisplayOrder;

PRINT '';
DECLARE @TotalServices INT = (SELECT COUNT(*) FROM [Services]);
PRINT '📊 Tổng số dịch vụ trong database: ' + CAST(@TotalServices AS VARCHAR);
PRINT '🎯 Featured: ' + CAST((SELECT COUNT(*) FROM [Services] WHERE IsFeatured = 1) AS VARCHAR);
PRINT '⭐ Popular: ' + CAST((SELECT COUNT(*) FROM [Services] WHERE IsPopular = 1) AS VARCHAR);
PRINT '🆕 New: ' + CAST((SELECT COUNT(*) FROM [Services] WHERE IsNew = 1) AS VARCHAR);
PRINT '';
PRINT '🎉 Database đã sạch sẽ và sẵn sàng sử dụng!';
