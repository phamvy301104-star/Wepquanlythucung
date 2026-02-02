-- =============================================
-- Script: Fix Product Images
-- Database: UmeAPI
-- Date: 2026-01-12
-- Description: Update ImageUrl cho products không có ảnh
-- =============================================

PRINT '🖼️  Bắt đầu cập nhật ảnh cho products...';
GO

-- Update từng product với ảnh phù hợp từ Unsplash
-- Dầu gội
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?w=600&h=600&fit=crop'
WHERE Id = 6 AND Name LIKE N'%Dầu Gội Gatsby%';
PRINT '   ✓ ID 6: Dầu Gội Gatsby';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1556228578-0d85b1a4d571?w=600&h=600&fit=crop'
WHERE Id = 7 AND Name LIKE N'%Dầu Gội Romano Classic%';
PRINT '   ✓ ID 7: Dầu Gội Romano Classic';

-- Sáp vuốt tóc / Pomade
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=600&h=600&fit=crop'
WHERE Id = 9 AND Name LIKE N'%Sáp Vuốt Tóc Gatsby%';
PRINT '   ✓ ID 9: Sáp Vuốt Tóc Gatsby';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1629118760089-b53c994f5e4a?w=600&h=600&fit=crop'
WHERE Id = 10 AND Name LIKE N'%Reuzel Blue%';
PRINT '   ✓ ID 10: Reuzel Blue Pomade';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1617897903246-719242758050?w=600&h=600&fit=crop'
WHERE Id = 11 AND Name LIKE N'%Suavecito%';
PRINT '   ✓ ID 11: Suavecito Pomade';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1571875257727-256c39da42af?w=600&h=600&fit=crop'
WHERE Id = 12 AND Name LIKE N'%Gatsby Moving Rubber%';
PRINT '   ✓ ID 12: Gatsby Moving Rubber';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1616394584738-fc6e612e71b9?w=600&h=600&fit=crop'
WHERE Id = 13 AND Name LIKE N'%Reuzel Pink%';
PRINT '   ✓ ID 13: Reuzel Pink Grease';

-- Gel tóc
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1620916566398-39f1143ab7be?w=600&h=600&fit=crop'
WHERE Id = 14 AND Name LIKE N'%Gel Tóc Gatsby Set%';
PRINT '   ✓ ID 14: Gatsby Gel Set & Keep';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=600&h=600&fit=crop'
WHERE Id = 16 AND Name LIKE N'%Gel Vuốt Tóc Gatsby Water%';
PRINT '   ✓ ID 16: Gatsby Water Gloss';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1574101656628-bfee1fccdc4e?w=600&h=600&fit=crop'
WHERE Id = 18 AND Name LIKE N'%Gel Tóc Clear Men%';
PRINT '   ✓ ID 18: Clear Men Hair Gel';

-- Gôm xịt tóc
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1594035910387-fea47794261f?w=600&h=600&fit=crop'
WHERE Id = 15 AND Name LIKE N'%Gôm Romano Hair Spray%';
PRINT '   ✓ ID 15: Romano Hair Spray';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1619451334792-150fd785ee74?w=600&h=600&fit=crop'
WHERE Id = 17 AND Name LIKE N'%Gôm Xịt Romano Extra%';
PRINT '   ✓ ID 17: Romano Extra Hold';

-- Dầu xả
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1598452963314-b09f397a5c48?w=600&h=600&fit=crop'
WHERE Id = 20 AND Name LIKE N'%Dầu Xả Clear Men%';
PRINT '   ✓ ID 20: Clear Men Dầu Xả';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1527799820374-dcf8d9d4a388?w=600&h=600&fit=crop'
WHERE Id = 21 AND Name LIKE N'%Dầu Xả Gatsby Hair Treatment%';
PRINT '   ✓ ID 21: Gatsby Hair Treatment';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1556228852-80c3a083d6a3?w=600&h=600&fit=crop'
WHERE Id = 22 AND Name LIKE N'%Dầu Xả Romano Classic%';
PRINT '   ✓ ID 22: Romano Dầu Xả';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1571875257727-256c39da42af?w=600&h=600&fit=crop'
WHERE Id = 23 AND Name LIKE N'%Dầu Xả Gatsby Silk%';
PRINT '   ✓ ID 23: Gatsby Silk Protein';

-- Dụng cụ barber
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1620843002805-05a08cb72f57?w=600&h=600&fit=crop'
WHERE Id = 24 AND Name LIKE N'%Lược Cắt Tóc%';
PRINT '   ✓ ID 24: Lược Cắt Tóc';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1582735689369-4fe89db7114c?w=600&h=600&fit=crop'
WHERE Id = 25 AND Name LIKE N'%Khăn Tắm Barber%';
PRINT '   ✓ ID 25: Khăn Barber';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1592647420148-bfcc177e2117?w=600&h=600&fit=crop'
WHERE Id = 26 AND Name LIKE N'%Lưỡi Dao Cạo%';
PRINT '   ✓ ID 26: Lưỡi Dao Feather';

UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1585747860715-2ba37e788b70?w=600&h=600&fit=crop'
WHERE Id = 27 AND Name LIKE N'%Máy Cắt Tóc%';
PRINT '   ✓ ID 27: Máy Cắt Barber';

-- TEST product
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1503951914875-452162b0f3f1?w=600&h=600&fit=crop'
WHERE Id = 29 AND Name = 'TEST 1';
PRINT '   ✓ ID 29: TEST Product';

GO

-- Verify kết quả
PRINT '';
PRINT '✅ Hoàn thành! Kiểm tra kết quả:';
PRINT '================================================';

SELECT 
    Id,
    Name,
    CASE 
        WHEN ImageUrl IS NULL THEN '❌ NULL'
        ELSE '✓ OK'
    END AS [Status],
    LEFT(ImageUrl, 50) AS ImageUrl_Preview
FROM [Products]
WHERE Id IN (6,7,9,10,11,12,13,14,15,16,17,18,20,21,22,23,24,25,26,27,29)
ORDER BY Id;

DECLARE @NullCount INT = (SELECT COUNT(*) FROM [Products] WHERE ImageUrl IS NULL AND IsDeleted = 0);
PRINT '';
PRINT '📊 Tổng products còn NULL: ' + CAST(@NullCount AS VARCHAR);
IF @NullCount = 0
    PRINT '🎉 Tất cả products đã có ảnh!';
ELSE
    PRINT '⚠️  Còn ' + CAST(@NullCount AS VARCHAR) + ' products chưa có ảnh';
