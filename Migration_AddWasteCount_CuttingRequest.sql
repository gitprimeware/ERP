-- Migration: Add WasteCount column to CuttingRequests table
-- Date: 2025-01-XX

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CuttingRequests]') AND name = 'WasteCount')
BEGIN
    ALTER TABLE [dbo].[CuttingRequests]
    ADD [WasteCount] INT NULL;
    
    PRINT 'WasteCount column added to CuttingRequests table';
END
ELSE
BEGIN
    PRINT 'WasteCount column already exists in CuttingRequests table';
END
GO

