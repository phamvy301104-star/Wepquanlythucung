-- Fix missing columns in database
-- Run this script to add all missing columns

-- 1. Add missing columns to OrderItems table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'ProductSKU')
    ALTER TABLE OrderItems ADD ProductSKU NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'OrderItems' AND COLUMN_NAME = 'VariantOptions')
    ALTER TABLE OrderItems ADD VariantOptions NVARCHAR(500) NULL;

-- 2. Add missing columns to Appointments table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Appointments' AND COLUMN_NAME = 'CheckedInAt')
    ALTER TABLE Appointments ADD CheckedInAt DATETIME2 NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Appointments' AND COLUMN_NAME = 'CompletedAt')
    ALTER TABLE Appointments ADD CompletedAt DATETIME2 NULL;

PRINT 'All missing columns added successfully!';

-- 3. Update Pets table with better sample data (Vietnamese without special chars)
UPDATE Pets SET 
    Species = 'Cho',
    Breed = 'Corgi',
    Gender = 'Duc',
    Color = 'Vang trang'
WHERE PetCode = 'PET260202001';

UPDATE Pets SET 
    Species = 'Meo',
    Breed = 'British Shorthair',
    Gender = 'Cai',
    Color = 'Xam xanh'
WHERE PetCode = 'PET260202002';

UPDATE Pets SET 
    Species = 'Cho',
    Breed = 'Poodle',
    Gender = 'Duc',
    Color = 'Trang'
WHERE PetCode = 'PET260202003';

UPDATE Pets SET 
    Species = 'Cho',
    Breed = 'Samoyed',
    Gender = 'Duc',
    Color = 'Trang'
WHERE PetCode = 'PET260202004';

UPDATE Pets SET 
    Species = 'Meo',
    Breed = 'Scottish Fold',
    Gender = 'Cai',
    Color = 'Xam'
WHERE PetCode = 'PET260202005';

UPDATE Pets SET 
    Species = 'Cho',
    Breed = 'Shiba Inu',
    Gender = 'Duc',
    Color = 'Vang'
WHERE PetCode = 'PET260202006';

PRINT 'Pets data updated!';

-- 4. Verify
SELECT 'Pets updated:' AS Info;
SELECT PetCode, Name, Species, Breed, Gender, Color, Price, Status FROM Pets;
