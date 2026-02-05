using ERP.Core.Encryption;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ERP.DAL.Configuration
{
    internal class ConnectionStringManager
    {
        private static DatabaseConfig? _databaseConfigs;
        internal static DatabaseConfig DatabaseConfigs =>
            _databaseConfigs ??= LoadConnectionStrings();
        internal class DatabaseConfig
        {
            internal string? UserDbConnectionString { get; set; }
            internal string? ErpDbConnectionString { get; set; }
        }


        internal static DatabaseConfig LoadConnectionStrings()
        {
            var flowErpDbConnection = StringEncrypter.Decrypt("Z2PQZvP4TOtExwXzy4xryAJlEZEP6nNBsSA2J8JLwn9qwSoM14+quhBe+wbcJjfF0hl2/Cw/T2Ngp+mQOdRAIj1gUEN7s1Jr+nZksEm4akVtMn2e5QR8riTpsjnVJym7wz4TZZRFPxxeg1knsK4txZbk/75OEie70fDqTzvoaxz35L3aTCJppw==");

            var userDbConnection = StringEncrypter.Decrypt("Z2PQZvP4TOtExwXzy4xryAJlEZEP6nNBsSA2J8JLwn9qwSoM14+quhBe+wbcJjfF2igzkRBjDyFNURIowZP0vw8ce4cpME4dDQdTzp3gf0MHRyAUtFa19XwjL82CI6MOay0h2GblfBriVTtYKaqlPzR43BFx3bNcZFY3MxK8f3q0CtKb76PObSTEPsVSR4w8");

            return new DatabaseConfig
            {
                ErpDbConnectionString = flowErpDbConnection,
                UserDbConnectionString = userDbConnection
            };
        }
        internal static string GetConnectionString(string dbName)
        {
            return dbName switch
            {
                "UserDbConnection" => DatabaseConfigs.UserDbConnectionString!,
                "ErpDbConnection" => DatabaseConfigs.ErpDbConnectionString!,
                _ => throw new ArgumentException("Invalid database name", nameof(dbName)),
            };
        }
    }
}
