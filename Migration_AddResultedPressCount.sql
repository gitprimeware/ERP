-- Migration: Add ResultedPressCount column to PressingRequests table
-- This column stores the number of pressed plates that resulted from the pressing operation

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PressingRequests]') AND name = 'ResultedPressCount')
BEGIN
    ALTER TABLE [dbo].[PressingRequests]
    ADD [ResultedPressCount] INT NULL;
    
    PRINT 'ResultedPressCount column added to PressingRequests table';
END
ELSE
BEGIN
    PRINT 'ResultedPressCount column already exists in PressingRequests table';
END
GO

