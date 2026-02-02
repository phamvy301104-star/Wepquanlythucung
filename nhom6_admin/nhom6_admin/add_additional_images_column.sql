-- Migration script to add AdditionalImages column to Products table
-- Run this if the column doesn't exist in your database

-- Check if column exists, if not add it
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Products]') 
    AND name = 'AdditionalImages'
)
BEGIN
    ALTER TABLE [dbo].[Products]
    ADD [AdditionalImages] NVARCHAR(MAX) NULL;
    
    PRINT 'Column AdditionalImages added successfully to Products table';
END
ELSE
BEGIN
    PRINT 'Column AdditionalImages already exists in Products table';
END
GO

-- Verify the column exists
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'AdditionalImages';
GO

PRINT 'Migration completed successfully!';
