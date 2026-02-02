-- Test script: Kiểm tra và thêm ảnh phụ cho sản phẩm

-- 1. Kiểm tra cột AdditionalImages có tồn tại không
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products' 
  AND COLUMN_NAME = 'AdditionalImages';

-- 2. Kiểm tra sản phẩm hiện có
SELECT TOP 5
    Id,
    Name,
    ImageUrl,
    AdditionalImages,
    CASE 
        WHEN AdditionalImages IS NULL THEN 'NULL'
        WHEN AdditionalImages = '' THEN 'EMPTY'
        ELSE 'HAS DATA'
    END AS Status
FROM Products
WHERE IsDeleted = 0
ORDER BY Id;

-- 3. Đếm số sản phẩm có ảnh phụ
SELECT 
    COUNT(*) AS TotalProducts,
    SUM(CASE WHEN AdditionalImages IS NOT NULL AND AdditionalImages != '' THEN 1 ELSE 0 END) AS HasAdditionalImages,
    SUM(CASE WHEN AdditionalImages IS NULL OR AdditionalImages = '' THEN 1 ELSE 0 END) AS NoAdditionalImages
FROM Products
WHERE IsDeleted = 0;

-- 4. Update một số sản phẩm test với ảnh phụ (ví dụ)
-- Thay đổi Id và URLs phù hợp với dữ liệu thực tế

/*
-- Ví dụ update sản phẩm ID = 1
UPDATE Products
SET AdditionalImages = '[
    "https://via.placeholder.com/500x500/FF6B6B/FFFFFF?text=Image+2",
    "https://via.placeholder.com/500x500/4ECDC4/FFFFFF?text=Image+3",
    "https://via.placeholder.com/500x500/45B7D1/FFFFFF?text=Image+4"
]'
WHERE Id = 1;

-- Hoặc dùng ảnh từ uploads folder
UPDATE Products
SET AdditionalImages = '[
    "/uploads/products/product1_2.jpg",
    "/uploads/products/product1_3.jpg",
    "/uploads/products/product1_4.jpg"
]'
WHERE Id = 1;
*/

-- 5. Kiểm tra lại sau khi update
SELECT 
    Id,
    Name,
    ImageUrl,
    AdditionalImages,
    LEN(AdditionalImages) AS ImageDataLength
FROM Products
WHERE AdditionalImages IS NOT NULL AND AdditionalImages != ''
  AND IsDeleted = 0;
