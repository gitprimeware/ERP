-- Migration Script: PlakaAdedi ve CuttingId kolonlarını ekle
-- Bu script'i SQL Server Management Studio'da veya SQL komut satırında çalıştırabilirsiniz

USE ERPDB2;
GO

-- 1. Cuttings tablosuna PlakaAdedi kolonu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'PlakaAdedi')
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[Cuttings]
        ADD [PlakaAdedi] INT NOT NULL DEFAULT 0;
        PRINT 'Cuttings tablosuna PlakaAdedi kolonu eklendi.';
    END TRY
    BEGIN CATCH
        PRINT 'Cuttings tablosuna PlakaAdedi kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'Cuttings tablosunda PlakaAdedi kolonu zaten mevcut.';
END
GO

-- 2. Pressings tablosuna CuttingId kolonu ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Pressings]') AND name = 'CuttingId')
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[Pressings]
        ADD [CuttingId] UNIQUEIDENTIFIER NULL;
        PRINT 'Pressings tablosuna CuttingId kolonu eklendi.';
    END TRY
    BEGIN CATCH
        PRINT 'Pressings tablosuna CuttingId kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'Pressings tablosunda CuttingId kolonu zaten mevcut.';
END
GO

-- 3. Pressings tablosuna Foreign Key ekle
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND type in (N'U'))
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Pressings]') AND name = 'FK_Pressings_Cuttings')
    BEGIN
        BEGIN TRY
            ALTER TABLE [dbo].[Pressings]
            ADD CONSTRAINT FK_Pressings_Cuttings FOREIGN KEY ([CuttingId]) REFERENCES [Cuttings]([Id]);
            PRINT 'Pressings tablosuna FK_Pressings_Cuttings foreign key eklendi.';
        END TRY
        BEGIN CATCH
            PRINT 'Pressings tablosuna foreign key eklenirken hata oluştu: ' + ERROR_MESSAGE();
        END CATCH
    END
    ELSE
    BEGIN
        PRINT 'Pressings tablosunda FK_Pressings_Cuttings foreign key zaten mevcut.';
    END
END
GO

-- 4. Orders tablosuna IsStockOrder kolonu ekle (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'IsStockOrder')
BEGIN
    BEGIN TRY
        ALTER TABLE [dbo].[Orders]
        ADD [IsStockOrder] BIT NOT NULL DEFAULT 0;
        PRINT 'Orders tablosuna IsStockOrder kolonu eklendi.';
    END TRY
    BEGIN CATCH
        PRINT 'Orders tablosuna IsStockOrder kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT 'Orders tablosunda IsStockOrder kolonu zaten mevcut.';
END
GO

PRINT 'Migration tamamlandı!';
GO

