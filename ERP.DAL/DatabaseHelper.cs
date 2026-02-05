using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ERP.DAL
{
    public class DatabaseHelper
    {
        private static string? _connectionString;
        private const string DatabaseFileName = "ERPDB.db";

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
            // Get application directory
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var databasePath = Path.Combine(appDirectory, DatabaseFileName);
            
            _connectionString = $"Data Source={databasePath}";
        }

        public static string GetDatabasePath()
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDirectory, DatabaseFileName);
        }

        public static bool TestConnection()
        {
            try
            {
                using (var context = new ErpDbContext())
                {
                    return context.Database.CanConnect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı bağlantısı başarısız: " + ex.Message);
            }
        }

        public static void EnsureDatabaseCreated()
        {
            using (var context = new ErpDbContext())
            {
                context.Database.EnsureCreated();
            }
        }

        public static void MigrateDatabase()
        {
            using (var context = new ErpDbContext())
            {
                context.Database.Migrate();
            }
        }
    }
}

