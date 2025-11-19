using System;
using Microsoft.Data.SqlClient;

namespace ERP.DAL
{
    public class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=ERPDB;Integrated Security=true;TrustServerCertificate=true;";

        public static string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
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

