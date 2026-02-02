-- Fix Product Images - Version 2 (Direct ID update)
PRINT '🖼️  Cập nhật ảnh cho products bằng ID...';

-- Update by direct ID
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?w=600&h=600&fit=crop' WHERE Id = 6;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1556228578-0d85b1a4d571?w=600&h=600&fit=crop' WHERE Id = 7;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=600&h=600&fit=crop' WHERE Id = 9;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1620916566398-39f1143ab7be?w=600&h=600&fit=crop' WHERE Id = 14;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1594035910387-fea47794261f?w=600&h=600&fit=crop' WHERE Id = 15;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1608248543803-ba4f8c70ae0b?w=600&h=600&fit=crop' WHERE Id = 16;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1619451334792-150fd785ee74?w=600&h=600&fit=crop' WHERE Id = 17;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1574101656628-bfee1fccdc4e?w=600&h=600&fit=crop' WHERE Id = 18;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1598452963314-b09f397a5c48?w=600&h=600&fit=crop' WHERE Id = 20;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1527799820374-dcf8d9d4a388?w=600&h=600&fit=crop' WHERE Id = 21;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1556228852-80c3a083d6a3?w=600&h=600&fit=crop' WHERE Id = 22;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1571875257727-256c39da42af?w=600&h=600&fit=crop' WHERE Id = 23;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1620843002805-05a08cb72f57?w=600&h=600&fit=crop' WHERE Id = 24;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1582735689369-4fe89db7114c?w=600&h=600&fit=crop' WHERE Id = 25;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1592647420148-bfcc177e2117?w=600&h=600&fit=crop' WHERE Id = 26;
UPDATE [Products] SET ImageUrl = 'https://images.unsplash.com/photo-1585747860715-2ba37e788b70?w=600&h=600&fit=crop' WHERE Id = 27;

PRINT '✅ Đã update 16 products';

SELECT Id, Name, LEFT(ImageUrl, 50) AS ImageUrl FROM Products WHERE Id IN (6,7,9,14,15,16,17,18,20,21,22,23,24,25,26,27);
