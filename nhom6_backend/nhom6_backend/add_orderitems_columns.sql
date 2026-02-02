-- Script to add missing columns to OrderItems table
-- Run this in SQL Server Management Studio against the UmeAPI database

USE UmeAPI;
GO

-- Check if columns exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OrderItems]') AND name = 'ProductVariantId')
BEGIN
    ALTER TABLE [dbo].[OrderItems] ADD [ProductVariantId] INT NULL;
    PRINT 'Added ProductVariantId column';
END
ELSE
BEGIN
    PRINT 'ProductVariantId column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OrderItems]') AND name = 'SKU')
BEGIN
    ALTER TABLE [dbo].[OrderItems] ADD [SKU] NVARCHAR(50) NULL;
    PRINT 'Added SKU column';
END
ELSE
BEGIN
    PRINT 'SKU column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[OrderItems]') AND name = 'VariantName')
BEGIN
    ALTER TABLE [dbo].[OrderItems] ADD [VariantName] NVARCHAR(100) NULL;
    PRINT 'Added VariantName column';
END
ELSE
BEGIN
    PRINT 'VariantName column already exists';
END
GO

-- Add foreign key constraint for ProductVariantId if ProductVariants table exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductVariants')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrderItems_ProductVariants_ProductVariantId')
    BEGIN
        ALTER TABLE [dbo].[OrderItems] 
        ADD CONSTRAINT [FK_OrderItems_ProductVariants_ProductVariantId] 
        FOREIGN KEY ([ProductVariantId]) REFERENCES [dbo].[ProductVariants]([Id]);
        PRINT 'Added FK_OrderItems_ProductVariants_ProductVariantId constraint';
    END
END
GO

PRINT 'Migration completed successfully!';
