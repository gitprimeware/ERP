-- Migration Script for Cuttings and MaterialEntries Tables
-- Bu script'i SQL Server Management Studio'da çalıştırın

USE ERPDB2;
GO

-- 1. MaterialEntries tablosuna SerialNoId kolonu ekle
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND type in (N'U'))
BEGIN
    -- SerialNoId kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND name = 'SerialNoId')
    BEGIN
        ALTER TABLE [dbo].[MaterialEntries]
        ADD [SerialNoId] UNIQUEIDENTIFIER NULL
        PRINT 'SerialNoId kolonu MaterialEntries tablosuna eklendi.'
    END
    
    -- Foreign key ekle (eğer yoksa)
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND referenced_object_id = OBJECT_ID(N'[dbo].[SerialNos]'))
    BEGIN
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SerialNos]') AND type in (N'U'))
        BEGIN
            ALTER TABLE [dbo].[MaterialEntries]
            ADD CONSTRAINT FK_MaterialEntries_SerialNos FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id])
            PRINT 'FK_MaterialEntries_SerialNos foreign key eklendi.'
        END
    END
END
GO

-- 2. Cuttings tablosuna yeni kolonları ekle
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND type in (N'U'))
BEGIN
    -- Hatve kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'Hatve')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] ADD [Hatve] DECIMAL(10,2) NOT NULL DEFAULT 0
        PRINT 'Hatve kolonu Cuttings tablosuna eklendi.'
    END
    
    -- SerialNoId kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'SerialNoId')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] ADD [SerialNoId] UNIQUEIDENTIFIER NULL
        PRINT 'SerialNoId kolonu Cuttings tablosuna eklendi.'
    END
    
    -- MachineId kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'MachineId')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] ADD [MachineId] UNIQUEIDENTIFIER NULL
        PRINT 'MachineId kolonu Cuttings tablosuna eklendi.'
    END
    
    -- CuttingCount kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'CuttingCount')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] ADD [CuttingCount] INT NOT NULL DEFAULT 0
        PRINT 'CuttingCount kolonu Cuttings tablosuna eklendi.'
    END
    
    -- WasteKg kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'WasteKg')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] ADD [WasteKg] DECIMAL(18,3) NOT NULL DEFAULT 0
        PRINT 'WasteKg kolonu Cuttings tablosuna eklendi.'
    END
    
    -- EmployeeId kolonu yoksa ekle
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'EmployeeId')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] ADD [EmployeeId] UNIQUEIDENTIFIER NULL
        PRINT 'EmployeeId kolonu Cuttings tablosuna eklendi.'
    END
    
    -- Eski Thickness kolonu varsa kaldır (artık kullanılmıyor)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'Thickness')
    BEGIN
        ALTER TABLE [dbo].[Cuttings] DROP COLUMN [Thickness]
        PRINT 'Eski Thickness kolonu Cuttings tablosundan kaldırıldı.'
    END
    
    -- Foreign key'leri ekle (eğer yoksa)
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND referenced_object_id = OBJECT_ID(N'[dbo].[SerialNos]'))
    BEGIN
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SerialNos]') AND type in (N'U'))
        BEGIN
            ALTER TABLE [dbo].[Cuttings]
            ADD CONSTRAINT FK_Cuttings_SerialNos FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id])
            PRINT 'FK_Cuttings_SerialNos foreign key eklendi.'
        END
    END
    
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND referenced_object_id = OBJECT_ID(N'[dbo].[Machines]'))
    BEGIN
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Machines]') AND type in (N'U'))
        BEGIN
            ALTER TABLE [dbo].[Cuttings]
            ADD CONSTRAINT FK_Cuttings_Machines FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id])
            PRINT 'FK_Cuttings_Machines foreign key eklendi.'
        END
    END
    
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND referenced_object_id = OBJECT_ID(N'[dbo].[Employees]'))
    BEGIN
        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
        BEGIN
            ALTER TABLE [dbo].[Cuttings]
            ADD CONSTRAINT FK_Cuttings_Employees FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
            PRINT 'FK_Cuttings_Employees foreign key eklendi.'
        END
    END
END
GO

PRINT 'Migration tamamlandı!'
GO

