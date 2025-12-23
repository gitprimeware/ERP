-- Migration: Add WasteCount column to Cuttings table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'WasteCount')
BEGIN
    ALTER TABLE [dbo].[Cuttings]
    ADD [WasteCount] INT NULL;

    PRINT 'WasteCount column added to Cuttings table';
END
ELSE
BEGIN
    PRINT 'WasteCount column already exists in Cuttings table';
END
GO

