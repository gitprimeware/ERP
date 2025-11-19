using System;
using Microsoft.Data.SqlClient;
using ERP.DAL;

namespace ERP.DAL
{
    public static class DatabaseInitializer
    {
        private const string DatabaseName = "ERPDB";

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
                    CreateOrdersTable(connection);
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
                        [LamelThickness] DECIMAL(10,2) NULL,
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
        }
    }
}

