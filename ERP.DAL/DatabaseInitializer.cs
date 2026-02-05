using System;
using Microsoft.EntityFrameworkCore;
using ERP.DAL;

namespace ERP.DAL
{
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            try
            {
                // Ensure both databases are created with the latest schema
                DatabaseHelper.EnsureDatabasesCreated();
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı başlatma hatası: " + ex.Message, ex);
            }
        }

        public static void InitializeWithMigrations()
        {
            try
            {
                // Apply migrations to both databases
                DatabaseHelper.MigrateDatabases();
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı göç hatası: " + ex.Message, ex);
            }
        }
    }
}

