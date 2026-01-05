using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PackagingRepository
    {
        public List<Packaging> GetAll()
        {
            var packagings = new List<Packaging>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.Id, p.OrderId, p.AssemblyId, p.IsolationId, p.PlateThickness, p.Hatve, p.Size, p.Length,
                             p.SerialNoId, p.MachineId, p.PackagingCount, p.UsedAssemblyCount, p.EmployeeId, p.PackagingDate,
                             p.CreatedDate, p.ModifiedDate, p.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Packagings p
                             LEFT JOIN SerialNos sn ON p.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON p.MachineId = m.Id
                             LEFT JOIN Employees e ON p.EmployeeId = e.Id
                             WHERE p.IsActive = 1
                             ORDER BY p.PackagingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            packagings.Add(MapToPackaging(reader));
                        }
                    }
                }
            }
            
            return packagings;
        }

        public List<Packaging> GetByOrderId(Guid orderId)
        {
            var packagings = new List<Packaging>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.Id, p.OrderId, p.AssemblyId, p.IsolationId, p.PlateThickness, p.Hatve, p.Size, p.Length,
                             p.SerialNoId, p.MachineId, p.PackagingCount, p.UsedAssemblyCount, p.EmployeeId, p.PackagingDate,
                             p.CreatedDate, p.ModifiedDate, p.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Packagings p
                             LEFT JOIN SerialNos sn ON p.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON p.MachineId = m.Id
                             LEFT JOIN Employees e ON p.EmployeeId = e.Id
                             WHERE p.OrderId = @OrderId AND p.IsActive = 1
                             ORDER BY p.PackagingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            packagings.Add(MapToPackaging(reader));
                        }
                    }
                }
            }
            
            return packagings;
        }

        public Packaging GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.Id, p.OrderId, p.AssemblyId, p.PlateThickness, p.Hatve, p.Size, p.Length,
                             p.SerialNoId, p.MachineId, p.PackagingCount, p.UsedAssemblyCount, p.EmployeeId, p.PackagingDate,
                             p.CreatedDate, p.ModifiedDate, p.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Packagings p
                             LEFT JOIN SerialNos sn ON p.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON p.MachineId = m.Id
                             LEFT JOIN Employees e ON p.EmployeeId = e.Id
                             WHERE p.Id = @Id AND p.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPackaging(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Packaging packaging)
        {
            packaging.Id = Guid.NewGuid();
            packaging.CreatedDate = DateTime.Now;
            packaging.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Packagings (Id, OrderId, AssemblyId, IsolationId, PlateThickness, Hatve, Size, Length,
                             SerialNoId, MachineId, PackagingCount, UsedAssemblyCount, EmployeeId, PackagingDate, CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @AssemblyId, @IsolationId, @PlateThickness, @Hatve, @Size, @Length,
                             @SerialNoId, @MachineId, @PackagingCount, @UsedAssemblyCount, @EmployeeId, @PackagingDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddPackagingParameters(command, packaging);
                    command.ExecuteNonQuery();
                }
            }
            
            return packaging.Id;
        }

        public void Update(Packaging packaging)
        {
            packaging.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Packagings SET 
                             OrderId = @OrderId,
                             AssemblyId = @AssemblyId,
                             IsolationId = @IsolationId,
                             PlateThickness = @PlateThickness,
                             Hatve = @Hatve,
                             Size = @Size,
                             Length = @Length,
                             SerialNoId = @SerialNoId,
                             MachineId = @MachineId,
                             PackagingCount = @PackagingCount,
                             UsedAssemblyCount = @UsedAssemblyCount,
                             EmployeeId = @EmployeeId,
                             PackagingDate = @PackagingDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddPackagingParameters(command, packaging);
                    command.Parameters.AddWithValue("@ModifiedDate", packaging.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Packagings SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Packaging MapToPackaging(SqlDataReader reader)
        {
            var packaging = new Packaging
            {
                Id = reader.GetGuid("Id"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                Length = reader.GetDecimal("Length"),
                PackagingCount = reader.GetInt32("PackagingCount"),
                UsedAssemblyCount = reader.GetInt32("UsedAssemblyCount"),
                PackagingDate = reader.GetDateTime("PackagingDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                packaging.OrderId = reader.GetGuid("OrderId");
            }

            if (!reader.IsDBNull("AssemblyId"))
            {
                packaging.AssemblyId = reader.GetGuid("AssemblyId");
            }

            if (!reader.IsDBNull("IsolationId"))
            {
                packaging.IsolationId = reader.GetGuid("IsolationId");
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                packaging.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    packaging.SerialNo = new SerialNo
                    {
                        Id = packaging.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("MachineId"))
            {
                packaging.MachineId = reader.GetGuid("MachineId");
                if (!reader.IsDBNull("MachineName"))
                {
                    packaging.Machine = new Machine
                    {
                        Id = packaging.MachineId.Value,
                        Name = reader.GetString("MachineName")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                packaging.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    packaging.Employee = new Employee
                    {
                        Id = packaging.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            return packaging;
        }

        private void AddPackagingParameters(SqlCommand command, Packaging packaging)
        {
            command.Parameters.AddWithValue("@Id", packaging.Id);
            command.Parameters.AddWithValue("@OrderId", packaging.OrderId.HasValue ? (object)packaging.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@AssemblyId", packaging.AssemblyId.HasValue ? (object)packaging.AssemblyId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsolationId", packaging.IsolationId.HasValue ? (object)packaging.IsolationId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PlateThickness", packaging.PlateThickness);
            command.Parameters.AddWithValue("@Hatve", packaging.Hatve);
            command.Parameters.AddWithValue("@Size", packaging.Size);
            command.Parameters.AddWithValue("@Length", packaging.Length);
            command.Parameters.AddWithValue("@SerialNoId", packaging.SerialNoId.HasValue ? (object)packaging.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MachineId", packaging.MachineId.HasValue ? (object)packaging.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PackagingCount", packaging.PackagingCount);
            command.Parameters.AddWithValue("@UsedAssemblyCount", packaging.UsedAssemblyCount);
            command.Parameters.AddWithValue("@EmployeeId", packaging.EmployeeId.HasValue ? (object)packaging.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PackagingDate", packaging.PackagingDate);
            command.Parameters.AddWithValue("@CreatedDate", packaging.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", packaging.IsActive);
        }
    }
}

