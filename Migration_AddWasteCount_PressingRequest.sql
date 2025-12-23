-- Migration: Add WasteCount column to PressingRequests table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PressingRequests]') AND name = 'WasteCount')
BEGIN
    ALTER TABLE [dbo].[PressingRequests]
    ADD [WasteCount] INT NULL;

    PRINT 'WasteCount column added to PressingRequests table';
END
ELSE
BEGIN
    PRINT 'WasteCount column already exists in PressingRequests table';
END
GO

