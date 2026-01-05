using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class IsolationRepository
    {
        public List<Isolation> GetAll()
        {
            var isolations = new List<Isolation>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // IsolationMethod kolonunun varlığını kontrol et
                bool hasIsolationMethodColumn = ColumnExists(connection, "Isolations", "IsolationMethod");
                string isolationMethodColumn = hasIsolationMethodColumn ? "i.IsolationMethod, " : "";
                
                var query = $@"SELECT i.Id, i.OrderId, i.AssemblyId, i.PlateThickness, i.Hatve, i.Size, i.Length,
                             i.SerialNoId, i.MachineId, i.IsolationCount, i.UsedAssemblyCount, i.IsolationLiquidAmount, 
                             {isolationMethodColumn}i.EmployeeId, i.IsolationDate,
                             i.CreatedDate, i.ModifiedDate, i.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Isolations i
                             LEFT JOIN SerialNos sn ON i.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON i.MachineId = m.Id
                             LEFT JOIN Employees e ON i.EmployeeId = e.Id
                             WHERE i.IsActive = 1
                             ORDER BY i.IsolationDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            isolations.Add(MapToIsolation(reader));
                        }
                    }
                }
            }
            
            return isolations;
        }

        public List<Isolation> GetByOrderId(Guid orderId)
        {
            var isolations = new List<Isolation>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // IsolationMethod kolonunun varlığını kontrol et
                bool hasIsolationMethodColumn = ColumnExists(connection, "Isolations", "IsolationMethod");
                string isolationMethodColumn = hasIsolationMethodColumn ? "i.IsolationMethod, " : "";
                
                var query = $@"SELECT i.Id, i.OrderId, i.AssemblyId, i.PlateThickness, i.Hatve, i.Size, i.Length,
                             i.SerialNoId, i.MachineId, i.IsolationCount, i.UsedAssemblyCount, i.IsolationLiquidAmount, 
                             {isolationMethodColumn}i.EmployeeId, i.IsolationDate,
                             i.CreatedDate, i.ModifiedDate, i.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Isolations i
                             LEFT JOIN SerialNos sn ON i.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON i.MachineId = m.Id
                             LEFT JOIN Employees e ON i.EmployeeId = e.Id
                             WHERE i.OrderId = @OrderId AND i.IsActive = 1
                             ORDER BY i.IsolationDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            isolations.Add(MapToIsolation(reader));
                        }
                    }
                }
            }
            
            return isolations;
        }

        public Isolation GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // IsolationMethod kolonunun varlığını kontrol et
                bool hasIsolationMethodColumn = ColumnExists(connection, "Isolations", "IsolationMethod");
                string isolationMethodColumn = hasIsolationMethodColumn ? "i.IsolationMethod, " : "";
                
                var query = $@"SELECT i.Id, i.OrderId, i.AssemblyId, i.PlateThickness, i.Hatve, i.Size, i.Length,
                             i.SerialNoId, i.MachineId, i.IsolationCount, i.UsedAssemblyCount, i.IsolationLiquidAmount, 
                             {isolationMethodColumn}i.EmployeeId, i.IsolationDate,
                             i.CreatedDate, i.ModifiedDate, i.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Isolations i
                             LEFT JOIN SerialNos sn ON i.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON i.MachineId = m.Id
                             LEFT JOIN Employees e ON i.EmployeeId = e.Id
                             WHERE i.Id = @Id AND i.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToIsolation(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Isolation isolation)
        {
            isolation.Id = Guid.NewGuid();
            isolation.CreatedDate = DateTime.Now;
            isolation.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Isolations (Id, OrderId, AssemblyId, PlateThickness, Hatve, Size, Length,
                             SerialNoId, MachineId, IsolationCount, UsedAssemblyCount, IsolationLiquidAmount, 
                             IsolationMethod, EmployeeId, IsolationDate, CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @AssemblyId, @PlateThickness, @Hatve, @Size, @Length,
                             @SerialNoId, @MachineId, @IsolationCount, @UsedAssemblyCount, @IsolationLiquidAmount, 
                             @IsolationMethod, @EmployeeId, @IsolationDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddIsolationParameters(command, isolation);
                    command.ExecuteNonQuery();
                }
            }
            
            return isolation.Id;
        }

        public void Update(Isolation isolation)
        {
            isolation.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Isolations SET 
                             OrderId = @OrderId,
                             AssemblyId = @AssemblyId,
                             PlateThickness = @PlateThickness,
                             Hatve = @Hatve,
                             Size = @Size,
                             Length = @Length,
                             SerialNoId = @SerialNoId,
                             MachineId = @MachineId,
                             IsolationCount = @IsolationCount,
                             UsedAssemblyCount = @UsedAssemblyCount,
                             IsolationLiquidAmount = @IsolationLiquidAmount,
                             IsolationMethod = @IsolationMethod,
                             EmployeeId = @EmployeeId,
                             IsolationDate = @IsolationDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddIsolationParameters(command, isolation);
                    command.Parameters.AddWithValue("@ModifiedDate", isolation.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Isolations SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Isolation MapToIsolation(SqlDataReader reader)
        {
            var isolation = new Isolation
            {
                Id = reader.GetGuid("Id"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                Length = reader.GetDecimal("Length"),
                IsolationCount = reader.GetInt32("IsolationCount"),
                UsedAssemblyCount = reader.GetInt32("UsedAssemblyCount"),
                IsolationLiquidAmount = reader.GetDecimal("IsolationLiquidAmount"),
                IsolationMethod = GetIsolationMethod(reader),
                IsolationDate = reader.GetDateTime("IsolationDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                isolation.OrderId = reader.GetGuid("OrderId");
            }

            if (!reader.IsDBNull("AssemblyId"))
            {
                isolation.AssemblyId = reader.GetGuid("AssemblyId");
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                isolation.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    isolation.SerialNo = new SerialNo
                    {
                        Id = isolation.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("MachineId"))
            {
                isolation.MachineId = reader.GetGuid("MachineId");
                if (!reader.IsDBNull("MachineName"))
                {
                    isolation.Machine = new Machine
                    {
                        Id = isolation.MachineId.Value,
                        Name = reader.GetString("MachineName")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                isolation.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    isolation.Employee = new Employee
                    {
                        Id = isolation.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            return isolation;
        }

        private bool ColumnExists(SqlConnection connection, string tableName, string columnName)
        {
            try
            {
                var query = @"SELECT COUNT(*) FROM sys.columns 
                             WHERE object_id = OBJECT_ID(@TableName) AND name = @ColumnName";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", $"[dbo].[{tableName}]");
                    command.Parameters.AddWithValue("@ColumnName", columnName);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetIsolationMethod(SqlDataReader reader)
        {
            try
            {
                int ordinal = reader.GetOrdinal("IsolationMethod");
                if (reader.IsDBNull(ordinal))
                    return "İzosiyanat+Poliol";
                return reader.GetString(ordinal);
            }
            catch
            {
                // Kolon yoksa varsayılan değer döndür
                return "İzosiyanat+Poliol";
            }
        }

        private void AddIsolationParameters(SqlCommand command, Isolation isolation)
        {
            command.Parameters.AddWithValue("@Id", isolation.Id);
            command.Parameters.AddWithValue("@OrderId", isolation.OrderId.HasValue ? (object)isolation.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@AssemblyId", isolation.AssemblyId.HasValue ? (object)isolation.AssemblyId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PlateThickness", isolation.PlateThickness);
            command.Parameters.AddWithValue("@Hatve", isolation.Hatve);
            command.Parameters.AddWithValue("@Size", isolation.Size);
            command.Parameters.AddWithValue("@Length", isolation.Length);
            command.Parameters.AddWithValue("@SerialNoId", isolation.SerialNoId.HasValue ? (object)isolation.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MachineId", isolation.MachineId.HasValue ? (object)isolation.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsolationCount", isolation.IsolationCount);
            command.Parameters.AddWithValue("@UsedAssemblyCount", isolation.UsedAssemblyCount);
            command.Parameters.AddWithValue("@IsolationLiquidAmount", isolation.IsolationLiquidAmount);
            command.Parameters.AddWithValue("@IsolationMethod", string.IsNullOrEmpty(isolation.IsolationMethod) ? "İzosiyanat+Poliol" : isolation.IsolationMethod);
            command.Parameters.AddWithValue("@EmployeeId", isolation.EmployeeId.HasValue ? (object)isolation.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsolationDate", isolation.IsolationDate);
            command.Parameters.AddWithValue("@CreatedDate", isolation.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", isolation.IsActive);
        }
    }
}


