using System;
using Microsoft.EntityFrameworkCore;
using ERP.DAL.Configuration;

namespace ERP.DAL
{
    public class DatabaseHelper
    {

        public static bool TestUserDbConnection()
        {
            try
            {
                using (var context = new UserDbContext())
                {
                    return context.Database.CanConnect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("User database connection failed: " + ex.Message, ex);
            }
        }

        public static bool TestErpDbConnection()
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
                throw new Exception("ERP database connection failed: " + ex.Message, ex);
            }
        }

        public static bool TestConnections()
        {
            bool userDbOk = TestUserDbConnection();
            bool erpDbOk = TestErpDbConnection();
            return userDbOk && erpDbOk;
        }

        public static void EnsureDatabasesCreated()
        {
            using (var userContext = new UserDbContext())
            {
                userContext.Database.EnsureCreated();
            }

            using (var erpContext = new ErpDbContext())
            {
                erpContext.Database.EnsureCreated();
            }
        }

        public static void MigrateDatabases()
        {
            using (var userContext = new UserDbContext())
            {
                userContext.Database.Migrate();
            }

            using (var erpContext = new ErpDbContext())
            {
                erpContext.Database.Migrate();
            }
        }
    }
}

