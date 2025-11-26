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
                    CreateSerialNosTable(connection);
                    CreateMachinesTable(connection);
                    CreateEmployeesTable(connection);
                    CreateOrdersTable(connection);
                    CreateMaterialEntriesTable(connection);
                    CreateMaterialExitsTable(connection);
                    CreateCuttingsTable(connection);
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

        private static void CreateSerialNosTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SerialNos]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[SerialNos] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [SerialNumber] NVARCHAR(100) NOT NULL,
                        [Description] NVARCHAR(500) NULL,
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

        private static void CreateMachinesTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Machines]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Machines] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [Name] NVARCHAR(200) NOT NULL,
                        [Code] NVARCHAR(50) NULL,
                        [Description] NVARCHAR(500) NULL,
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

        private static void CreateEmployeesTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Employees] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [FirstName] NVARCHAR(100) NOT NULL,
                        [LastName] NVARCHAR(100) NOT NULL,
                        [Phone] NVARCHAR(50) NULL,
                        [Email] NVARCHAR(100) NULL,
                        [Department] NVARCHAR(100) NULL,
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
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [InvoiceNo] NVARCHAR(100) NULL,
                        [TrexPurchaseNo] NVARCHAR(100) NULL,
                        [EntryDate] DATETIME NOT NULL,
                        [Quantity] DECIMAL(18,3) NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id])
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

            // Migration: SerialNoId kolonu ekle ve eski SerialNo kolonunu kaldır
            var serialNoMigrationQuery = @"
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
                    
                    -- Eski SerialNo kolonu varsa kaldır (opsiyonel - veri kaybı olabilir)
                    -- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[MaterialEntries]') AND name = 'SerialNo')
                    -- BEGIN
                    --     ALTER TABLE [dbo].[MaterialEntries]
                    --     DROP COLUMN [SerialNo]
                    --     PRINT 'Eski SerialNo kolonu kaldırıldı.'
                    -- END
                END";

            using (var serialNoMigrationCommand = new SqlCommand(serialNoMigrationQuery, connection))
            {
                try
                {
                    serialNoMigrationCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SerialNoId migration hatası: {ex.Message}");
                }
            }
        }

        private static void CreateCuttingsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Cuttings] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [TotalKg] DECIMAL(18,3) NOT NULL,
                        [CutKg] DECIMAL(18,3) NOT NULL,
                        [CuttingCount] INT NOT NULL DEFAULT 0,
                        [WasteKg] DECIMAL(18,3) NOT NULL DEFAULT 0,
                        [RemainingKg] DECIMAL(18,3) NOT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [CuttingDate] DATETIME NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

            // Migration: Eski kolonları kaldır ve yeni kolonları ekle
            var migrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND type in (N'U'))
                BEGIN
                    -- Eski kolonları kaldır (eğer varsa)
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'CompanyId')
                    BEGIN
                        -- Foreign key'i kaldır
                        DECLARE @FKName NVARCHAR(128)
                        SELECT @FKName = name FROM sys.foreign_keys 
                        WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') 
                        AND referenced_object_id = OBJECT_ID(N'[dbo].[Companies]')
                        AND COL_NAME(parent_object_id, parent_column_id) = 'CompanyId'
                        
                        IF @FKName IS NOT NULL
                            EXEC('ALTER TABLE [dbo].[Cuttings] DROP CONSTRAINT ' + @FKName)
                        
                        ALTER TABLE [dbo].[Cuttings] DROP COLUMN [CompanyId]
                    END
                    
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'Thickness')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] DROP COLUMN [Thickness]
                    END
                    
                    -- Yeni kolonları ekle (eğer yoksa)
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'Hatve')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] ADD [Hatve] DECIMAL(10,2) NOT NULL DEFAULT 0
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'SerialNoId')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] ADD [SerialNoId] UNIQUEIDENTIFIER NULL
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'MachineId')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] ADD [MachineId] UNIQUEIDENTIFIER NULL
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'CuttingCount')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] ADD [CuttingCount] INT NOT NULL DEFAULT 0
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'WasteKg')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] ADD [WasteKg] DECIMAL(18,3) NOT NULL DEFAULT 0
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'EmployeeId')
                    BEGIN
                        ALTER TABLE [dbo].[Cuttings] ADD [EmployeeId] UNIQUEIDENTIFIER NULL
                    END
                    
                    -- Foreign key'leri ekle (eğer yoksa)
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND referenced_object_id = OBJECT_ID(N'[dbo].[SerialNos]'))
                    BEGIN
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SerialNos]') AND type in (N'U'))
                        BEGIN
                            ALTER TABLE [dbo].[Cuttings]
                            ADD CONSTRAINT FK_Cuttings_SerialNos FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id])
                        END
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND referenced_object_id = OBJECT_ID(N'[dbo].[Machines]'))
                    BEGIN
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Machines]') AND type in (N'U'))
                        BEGIN
                            ALTER TABLE [dbo].[Cuttings]
                            ADD CONSTRAINT FK_Cuttings_Machines FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id])
                        END
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND referenced_object_id = OBJECT_ID(N'[dbo].[Employees]'))
                    BEGIN
                        IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type in (N'U'))
                        BEGIN
                            ALTER TABLE [dbo].[Cuttings]
                            ADD CONSTRAINT FK_Cuttings_Employees FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
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
                    System.Diagnostics.Debug.WriteLine($"Cuttings migration hatası: {ex.Message}");
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

