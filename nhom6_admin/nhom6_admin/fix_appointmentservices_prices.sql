-- Migration: Add UnitPrice and TotalPrice columns to AppointmentServices table
-- Created: 2026-01-13
-- Description: Fix missing columns for Dashboard to work properly

-- Check if columns exist before adding
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'AppointmentServices' AND COLUMN_NAME = 'UnitPrice')
BEGIN
    ALTER TABLE AppointmentServices ADD UnitPrice decimal(18,2) NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'AppointmentServices' AND COLUMN_NAME = 'TotalPrice')
BEGIN
    ALTER TABLE AppointmentServices ADD TotalPrice decimal(18,2) NOT NULL DEFAULT 0;
END

-- Update existing records to have correct values based on Price column
UPDATE AppointmentServices 
SET UnitPrice = ISNULL(Price, 0), 
    TotalPrice = ISNULL(Price, 0) * ISNULL(Quantity, 1) 
WHERE UnitPrice = 0;

PRINT 'Migration completed successfully!'
