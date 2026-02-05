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
                // Ensure the database is created with the latest schema
                DatabaseHelper.EnsureDatabaseCreated();
            }
            catch (Exception ex)
            {
                throw new Exception("Veritabanı başlatma hatası: " + ex.Message, ex);
            }
        }
    }
}

