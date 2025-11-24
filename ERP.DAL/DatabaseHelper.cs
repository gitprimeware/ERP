using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace ERP.DAL
{
    public class DatabaseHelper
    {
        private static string? _connectionString;
        private const string DatabaseName = "ERPDB2";

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    LoadConnectionString();
                }
                return _connectionString ?? string.Empty;
            }
            set { _connectionString = value; }
        }

        private static void LoadConnectionString()
        {
            try
            {
                // app.config'den connection string'i oku
                var configConnectionString = ConfigurationManager.ConnectionStrings["ERPConnection"]?.ConnectionString;
                
                if (string.IsNullOrEmpty(configConnectionString))
                {
                    // Eğer app.config'de yoksa, varsayılan connection string kullan
                    _connectionString = $"Server=localhost\\SQLEXPRESS;Database={DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
                }
                else
                {
                    // app.config'deki connection string'i kullan, ancak veritabanı adını ekle
                    var builder = new SqlConnectionStringBuilder(configConnectionString);
                    if (string.IsNullOrEmpty(builder.InitialCatalog) || builder.InitialCatalog == "master")
                    {
                        builder.InitialCatalog = DatabaseName;
                    }
                    _connectionString = builder.ConnectionString;
                }
            }
            catch
            {
                // Hata durumunda varsayılan connection string kullan
                _connectionString = $"Server=localhost\\SQLEXPRESS;Database={DatabaseName};Integrated Security=True;TrustServerCertificate=True;";
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static SqlConnection GetMasterConnection()
        {
            // Master veritabanına bağlanmak için connection string
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            builder.InitialCatalog = "master";
            return new SqlConnection(builder.ConnectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı bağlantısı başarısız: " + ex.Message);
            }
        }
    }
}

