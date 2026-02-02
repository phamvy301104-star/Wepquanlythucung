-- Add ListingType and AdoptionFee columns to Pets table
-- ListingType: 'Sale' (đang bán) or 'Adoption' (nhận nuôi)
-- AdoptionFee: Phí hỗ trợ cho thú cưng nhận nuôi (có thể = 0 nghĩa là miễn phí)

USE QuanLyPet;
GO

-- Check if ListingType column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Pets]') AND name = 'ListingType')
BEGIN
    ALTER TABLE [dbo].[Pets]
    ADD [ListingType] NVARCHAR(20) NOT NULL DEFAULT 'Sale';
    PRINT 'Added ListingType column';
END
ELSE
BEGIN
    PRINT 'ListingType column already exists';
END
GO

-- Check if AdoptionFee column exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Pets]') AND name = 'AdoptionFee')
BEGIN
    ALTER TABLE [dbo].[Pets]
    ADD [AdoptionFee] DECIMAL(18,2) NULL;
    PRINT 'Added AdoptionFee column';
END
ELSE
BEGIN
    PRINT 'AdoptionFee column already exists';
END
GO

-- Update existing pets to have ListingType = 'Sale' (default)
UPDATE [dbo].[Pets] 
SET [ListingType] = 'Sale' 
WHERE [ListingType] IS NULL OR [ListingType] = '';
GO

-- Optional: Add some sample adoption pets
-- INSERT INTO [dbo].[Pets] (Name, Species, Breed, Gender, AgeInMonths, Price, ListingType, AdoptionFee, IsForSale, Status, ...)
-- VALUES ('Bé Miu', 'Meo', 'Mèo ta', 'Cái', 6, 0, 'Adoption', 0, 1, 'Available', ...)

PRINT 'Database updated successfully!';
SELECT 'Pets with ListingType' as Info, ListingType, COUNT(*) as Count 
FROM [dbo].[Pets] 
GROUP BY ListingType;
