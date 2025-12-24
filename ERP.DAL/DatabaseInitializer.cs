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
                    CreatePressingsTable(connection);
                    CreateClampingsTable(connection);
                    CreateAssembliesTable(connection);
                    CreateCuttingRequestsTable(connection);
                    CreatePressingRequestsTable(connection);
                    CreateClampingRequestsTable(connection);
                    CreateAssemblyRequestsTable(connection);
                    CreateClamping2RequestsTable(connection);
                    CreatePackagingsTable(connection);
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
                        [IsStockOrder] BIT NOT NULL DEFAULT 0,
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

            // Mevcut tablolar için migration: IsStockOrder kolonunu ekle
            var isStockOrderMigrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'IsStockOrder')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[Orders]
                            ADD [IsStockOrder] BIT NOT NULL DEFAULT 0
                            PRINT 'IsStockOrder kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'IsStockOrder kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                END";

            using (var isStockOrderMigrationCommand = new SqlCommand(isStockOrderMigrationQuery, connection))
            {
                try
                {
                    isStockOrderMigrationCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Orders migration başarılı: IsStockOrder kolonu kontrol edildi.");
                }
                catch (Exception ex)
                {
                    // Migration hatası olursa devam et
                    System.Diagnostics.Debug.WriteLine($"IsStockOrder migration hatası: {ex.Message}");
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
                        [WasteCount] INT NULL,
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
                    
                    -- WasteCount kolonu ekle
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'WasteCount')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[Cuttings] ADD [WasteCount] INT NULL
                            PRINT 'Cuttings tablosuna WasteCount kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'Cuttings tablosuna WasteCount kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                    
                    -- PlakaAdedi kolonu ekle
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND name = 'PlakaAdedi')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[Cuttings] ADD [PlakaAdedi] INT NOT NULL DEFAULT 0
                            PRINT 'Cuttings tablosuna PlakaAdedi kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'Cuttings tablosuna PlakaAdedi kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                END";

            using (var migrationCommand = new SqlCommand(migrationQuery, connection))
            {
                try
                {
                    migrationCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Cuttings migration başarılı: PlakaAdedi kolonu kontrol edildi.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Cuttings migration hatası: {ex.Message}");
                    // Hata durumunda da devam et, çünkü kolon zaten var olabilir
                }
            }
        }

        private static void CreatePressingsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pressings]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Pressings] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [CuttingId] UNIQUEIDENTIFIER NULL,
                        [PressNo] NVARCHAR(50) NULL,
                        [Pressure] DECIMAL(18,3) NOT NULL,
                        [PressCount] INT NOT NULL,
                        [WasteAmount] DECIMAL(18,3) NOT NULL DEFAULT 0,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [PressingDate] DATETIME NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id]),
                        FOREIGN KEY ([CuttingId]) REFERENCES [Cuttings]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

            // Mevcut tablolar için migration: CuttingId kolonunu ekle
            var pressingMigrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pressings]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Pressings]') AND name = 'CuttingId')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[Pressings]
                            ADD [CuttingId] UNIQUEIDENTIFIER NULL
                            
                            -- Foreign key ekle
                            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cuttings]') AND type in (N'U'))
                            BEGIN
                                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'[dbo].[Pressings]') AND name = 'FK_Pressings_Cuttings')
                                BEGIN
                                    ALTER TABLE [dbo].[Pressings]
                                    ADD CONSTRAINT FK_Pressings_Cuttings FOREIGN KEY ([CuttingId]) REFERENCES [Cuttings]([Id])
                                END
                            END
                            
                            PRINT 'Pressings tablosuna CuttingId kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'Pressings tablosuna CuttingId kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                END";

            using (var pressingMigrationCommand = new SqlCommand(pressingMigrationQuery, connection))
            {
                try
                {
                    pressingMigrationCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Pressings migration başarılı: CuttingId kolonu kontrol edildi.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Pressings migration hatası: {ex.Message}");
                    // Hata durumunda da devam et, çünkü kolon zaten var olabilir
                }
            }
        }

        private static void CreateClampingsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clampings]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Clampings] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [PressingId] UNIQUEIDENTIFIER NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [Length] DECIMAL(10,2) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [ClampCount] INT NOT NULL,
                        [UsedPlateCount] INT NOT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [ClampingDate] DATETIME NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([PressingId]) REFERENCES [Pressings]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateAssembliesTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assemblies]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Assemblies] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [ClampingId] UNIQUEIDENTIFIER NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [Length] DECIMAL(10,2) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [AssemblyCount] INT NOT NULL,
                        [UsedClampCount] INT NOT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [AssemblyDate] DATETIME NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([ClampingId]) REFERENCES [Clampings]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreatePackagingsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Packagings]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Packagings] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [AssemblyId] UNIQUEIDENTIFIER NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [Length] DECIMAL(10,2) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [PackagingCount] INT NOT NULL,
                        [UsedAssemblyCount] INT NOT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [PackagingDate] DATETIME NOT NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([AssemblyId]) REFERENCES [Assemblies]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
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

        private static void CreateCuttingRequestsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CuttingRequests]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[CuttingRequests] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [RequestedPlateCount] INT NOT NULL,
                        [OnePlateWeight] DECIMAL(18,3) NOT NULL,
                        [TotalRequiredPlateWeight] DECIMAL(18,3) NOT NULL,
                        [RemainingKg] DECIMAL(18,3) NOT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [ActualCutCount] INT NULL,
                        [WasteCount] INT NULL,
                        [IsRollFinished] BIT NOT NULL DEFAULT 0,
                        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Beklemede',
                        [RequestDate] DATETIME NOT NULL,
                        [CompletionDate] DATETIME NULL,
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
        }

        private static void CreatePressingRequestsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PressingRequests]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[PressingRequests] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [CuttingId] UNIQUEIDENTIFIER NULL,
                        [RequestedPressCount] INT NOT NULL,
                        [ActualPressCount] INT NULL,
                        [ResultedPressCount] INT NULL,
                        [WasteCount] INT NULL,
                        [PressNo] NVARCHAR(50) NULL,
                        [Pressure] DECIMAL(10,2) NOT NULL,
                        [WasteAmount] DECIMAL(10,2) NOT NULL DEFAULT 0,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Beklemede',
                        [RequestDate] DATETIME NOT NULL,
                        [CompletionDate] DATETIME NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([CuttingId]) REFERENCES [Cuttings]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }

            // Mevcut tablolar için migration: ResultedPressCount ve WasteCount kolonlarını ekle
            var migrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PressingRequests]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PressingRequests]') AND name = 'ResultedPressCount')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[PressingRequests]
                            ADD [ResultedPressCount] INT NULL
                            
                            PRINT 'PressingRequests tablosuna ResultedPressCount kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'PressingRequests tablosuna ResultedPressCount kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                    
                    -- WasteCount kolonu ekle
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PressingRequests]') AND name = 'WasteCount')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[PressingRequests]
                            ADD [WasteCount] INT NULL
                            
                            PRINT 'PressingRequests tablosuna WasteCount kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'PressingRequests tablosuna WasteCount kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                END";

            using (var migrationCommand = new SqlCommand(migrationQuery, connection))
            {
                migrationCommand.ExecuteNonQuery();
            }
        }

        private static void CreateClampingRequestsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClampingRequests]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[ClampingRequests] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NOT NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [Length] DECIMAL(10,2) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [PressingId] UNIQUEIDENTIFIER NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [RequestedClampCount] INT NOT NULL,
                        [ActualClampCount] INT NULL,
                        [ResultedClampCount] INT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Beklemede',
                        [RequestDate] DATETIME NOT NULL,
                        [CompletionDate] DATETIME NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([PressingId]) REFERENCES [Pressings]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
            
            // Migration: ActualClampCount ve ResultedClampCount kolonlarını ekle (eğer yoksa)
            var migrationQuery = @"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClampingRequests]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ClampingRequests]') AND name = 'ActualClampCount')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[ClampingRequests]
                            ADD [ActualClampCount] INT NULL
                            
                            PRINT 'ClampingRequests tablosuna ActualClampCount kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'ClampingRequests tablosuna ActualClampCount kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ClampingRequests]') AND name = 'ResultedClampCount')
                    BEGIN
                        BEGIN TRY
                            ALTER TABLE [dbo].[ClampingRequests]
                            ADD [ResultedClampCount] INT NULL
                            
                            PRINT 'ClampingRequests tablosuna ResultedClampCount kolonu eklendi.'
                        END TRY
                        BEGIN CATCH
                            PRINT 'ClampingRequests tablosuna ResultedClampCount kolonu eklenirken hata oluştu: ' + ERROR_MESSAGE()
                        END CATCH
                    END
                END";
            
            using (var migrationCommand = new SqlCommand(migrationQuery, connection))
            {
                migrationCommand.ExecuteNonQuery();
            }
        }

        private static void CreateAssemblyRequestsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AssemblyRequests]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[AssemblyRequests] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [Size] DECIMAL(10,2) NOT NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [Length] DECIMAL(10,2) NOT NULL,
                        [SerialNoId] UNIQUEIDENTIFIER NULL,
                        [ClampingId] UNIQUEIDENTIFIER NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [RequestedAssemblyCount] INT NOT NULL,
                        [ActualClampCount] INT NULL,
                        [ResultedAssemblyCount] INT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Beklemede',
                        [RequestDate] DATETIME NOT NULL,
                        [CompletionDate] DATETIME NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([SerialNoId]) REFERENCES [SerialNos]([Id]),
                        FOREIGN KEY ([ClampingId]) REFERENCES [Clampings]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void CreateClamping2RequestsTable(SqlConnection connection)
        {
            var query = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clamping2Requests]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Clamping2Requests] (
                        [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                        [OrderId] UNIQUEIDENTIFIER NULL,
                        [Hatve] DECIMAL(10,2) NOT NULL,
                        [PlateThickness] DECIMAL(10,3) NOT NULL,
                        [FirstClampingId] UNIQUEIDENTIFIER NULL,
                        [SecondClampingId] UNIQUEIDENTIFIER NULL,
                        [ResultedSize] DECIMAL(10,2) NOT NULL,
                        [ResultedLength] DECIMAL(10,2) NOT NULL,
                        [MachineId] UNIQUEIDENTIFIER NULL,
                        [RequestedCount] INT NOT NULL,
                        [ActualCount] INT NULL,
                        [ResultedCount] INT NULL,
                        [EmployeeId] UNIQUEIDENTIFIER NULL,
                        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Beklemede',
                        [RequestDate] DATETIME NOT NULL,
                        [CompletionDate] DATETIME NULL,
                        [CreatedDate] DATETIME NOT NULL,
                        [ModifiedDate] DATETIME NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1,
                        FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
                        FOREIGN KEY ([FirstClampingId]) REFERENCES [Clampings]([Id]),
                        FOREIGN KEY ([SecondClampingId]) REFERENCES [Clampings]([Id]),
                        FOREIGN KEY ([MachineId]) REFERENCES [Machines]([Id]),
                        FOREIGN KEY ([EmployeeId]) REFERENCES [Employees]([Id])
                    )
                END";

            using (var command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}

