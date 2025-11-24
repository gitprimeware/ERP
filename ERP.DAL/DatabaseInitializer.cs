using System;
using Microsoft.Data.SqlClient;
using ERP.DAL;

namespace ERP.DAL
{
    public static class DatabaseInitializer
    {
        private const string DatabaseName = "ERPDB2";

        public static void InitializeDatabase()
        {
            try
            {
                // Önce veritabanının var olup olmadığını kontrol et ve yoksa oluştur
                EnsureDatabaseExists();

                // Sonra tabloları oluştur
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    
                    CreateCompaniesTable(connection);
                    CreateSuppliersTable(connection);
                    CreateOrdersTable(connection);
                    CreateMaterialEntriesTable(connection);
                    CreateMaterialExitsTable(connection);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı başlatma hatası: " + ex.Message);
            }
        }

        private static void EnsureDatabaseExists()
        {
            try
            {
                using (var masterConnection = DatabaseHelper.GetMasterConnection())
                {
                    masterConnection.Open();
                    
                    // Veritabanının var olup olmadığını kontrol et
                    var checkDbQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{DatabaseName}'";
                    using (var checkCommand = new SqlCommand(checkDbQuery, masterConnection))
                    {
                        var exists = (int)checkCommand.ExecuteScalar() > 0;
                        
                        if (!exists)
                        {
                            // Veritabanını oluştur
                            var createDbQuery = $"CREATE DATABASE [{DatabaseName}]";
                            using (var createCommand = new SqlCommand(createDbQuery, masterConnection))
                            {
                                createCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Veritabanı oluşturma hatası: {ex.Message}");
            }
        }

        private static void CreateCompaniesTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Companies] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [Name] NVARCHAR(200) NOT NULL,
                        [Address] NVARCHAR(500) NULL,
                        [Phone] NVARCHAR(50) NULL,
                        [Email] NVARCHAR(100) NULL,
                        [TaxNumber] NVARCHAR(50) NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateOrdersTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Orders] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [CompanyId] UNIQUEIDENTIFIER NOT NULL,
                        [CustomerOrderNo] NVARCHAR(100) NOT NULL,
                        [TrexOrderNo] NVARCHAR(100) NOT NULL,
                        [DeviceName] NVARCHAR(200) NULL,
                        [OrderDate] DATETIME NOT NULL,
                        [TermDate] DATETIME NOT NULL,
                        [ProductCode] NVARCHAR(200) NULL,
                        [BypassSize] NVARCHAR(100) NULL,
                        [BypassType] NVARCHAR(100) NULL,
                        [LamelThickness] DECIMAL(10,3) NULL,
                        [ProductType] NVARCHAR(50) NULL,
                        [Quantity] INT NOT NULL,
                        [SalesPrice] DECIMAL(18,2) NULL,
                        [TotalPrice] DECIMAL(18,2) NOT NULL,
                        [ShipmentDate] DATETIME NULL,
                        [CurrencyRate] DECIMAL(18,4) NULL,
                        [Status] NVARCHAR(50) NULL DEFAULT 'Yeni',
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([CompanyId]) REFERENCES [Companies]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

            // Mevcut tablolar için migration: LamelThickness kolonunu DECIMAL(10,3) yap
            var migrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
                BEGIN
                    -- Kolonun mevcut scale'ini kontrol et ve gerekirse güncelle
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'LamelThickness')
                    BEGIN
                        DECLARE @CurrentScale INT
                        SELECT @CurrentScale = numeric_scale FROM sys.columns 
                        WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'LamelThickness'
                        
                        -- Eğer scale 2 ise (DECIMAL(10,2)), 3'e güncelle
                        IF @CurrentScale = 2
                        BEGIN
                            BEGIN TRY
                                ALTER TABLE [dbo].[Orders]
                                ALTER COLUMN [LamelThickness] DECIMAL(10,3) NULL
                                PRINT 'LamelThickness kolonu DECIMAL(10,3) olarak güncellendi.'
                            END TRY
                            BEGIN CATCH
                                PRINT 'LamelThickness kolonu güncellenirken hata oluştu: ' + ERROR_MESSAGE()
                            END CATCH
                        END
                    END
                END";

            using (var migrationCommand = new SqlCommand(migrationQuery, connection))
            {
                try
                {
                    migrationCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Migration hatası olursa devam et (tablo zaten doğru formatta olabilir)
                    System.Diagnostics.Debug.WriteLine($"Migration hatası: {ex.Message}");
                }
            }
        }

        private static void CreateSuppliersTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Suppliers] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [Name] NVARCHAR(200) NOT NULL,
                        [Address] NVARCHAR(500) NULL,
                        [Phone] NVARCHAR(50) NULL,
                        [Email] NVARCHAR(100) NULL,
                        [TaxNumber] NVARCHAR(50) NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateMaterialEntriesTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[MaterialEntries] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [TransactionType] NVARCHAR(50) NOT NULL,
                        [MaterialType] NVARCHAR(50) NOT NULL,
                        [MaterialSize] NVARCHAR(100) NOT NULL,
                        [Size] INT NOT NULL,
                        [Thickness] DECIMAL(10,3) NOT NULL,
                        [SupplierId] UNIQUEIDENTIFIER NULL,
                        [InvoiceNo] NVARCHAR(100) NULL,
                        [TrexPurchaseNo] NVARCHAR(100) NULL,
                        [EntryDate] DATETIME NOT NULL,
                        [Quantity] DECIMAL(18,3) NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

            // Migration: Eğer tablo varsa ve Companies'a referans veriyorsa, Suppliers'a çevir
            var migrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND type in (N'U'))
                BEGIN
                    -- Foreign key'i kontrol et ve gerekirse değiştir
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND referenced_object_id = OBJECT_ID(N'[dbo].[Companies]'))
                    BEGIN
                        DECLARE @FKName NVARCHAR(128)
                        SELECT @FKName = name FROM sys.foreign_keys 
                        WHERE parent_object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') 
                        AND referenced_object_id = OBJECT_ID(N'[dbo].[Companies]')
                        
                        IF @FKName IS NOT NULL
                        BEGIN
                            EXEC('ALTER TABLE [dbo].[MaterialEntries] DROP CONSTRAINT ' + @FKName)
                            ALTER TABLE [dbo].[MaterialEntries]
                            ADD CONSTRAINT FK_MaterialEntries_Suppliers FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers]([Id])
                        END
                    END
                    ELSE IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND referenced_object_id = OBJECT_ID(N'[dbo].[Suppliers]'))
                    BEGIN
                        ALTER TABLE [dbo].[MaterialEntries]
                        ADD CONSTRAINT FK_MaterialEntries_Suppliers FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers]([Id])
                    END
                END";

            using (var migrationCommand = new SqlCommand(migrationQuery, connection))
            {
                try
                {
                    migrationCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Migration hatası: {ex.Message}");
                }
            }
        }

        private static void CreateMaterialExitsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MaterialExits]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[MaterialExits] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [TransactionType] NVARCHAR(50) NOT NULL,
                        [MaterialType] NVARCHAR(50) NOT NULL,
                        [MaterialSize] NVARCHAR(100) NOT NULL,
                        [Size] INT NOT NULL,
                        [Thickness] DECIMAL(10,3) NOT NULL,
                        [CompanyId] UNIQUEIDENTIFIER NULL,
                        [TrexInvoiceNo] NVARCHAR(100) NULL,
                        [ExitDate] DATETIME NOT NULL,
                        [Quantity] DECIMAL(18,3) NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([CompanyId]) REFERENCES [Companies]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}

