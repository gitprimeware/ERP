using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class AssemblyRepository
    {
        public List<Assembly> GetAll()
        {
            var assemblies = new List<Assembly>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT a.Id, a.OrderId, a.ClampingId, a.PlateThickness, a.Hatve, a.Size, a.Length,
                             a.SerialNoId, a.MachineId, a.AssemblyCount, a.UsedClampCount, a.EmployeeId, a.AssemblyDate,
                             a.CreatedDate, a.ModifiedDate, a.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Assemblies a
                             LEFT JOIN SerialNos sn ON a.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON a.MachineId = m.Id
                             LEFT JOIN Employees e ON a.EmployeeId = e.Id
                             WHERE a.IsActive = 1
                             ORDER BY a.AssemblyDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            assemblies.Add(MapToAssembly(reader));
                        }
                    }
                }
            }
            
            return assemblies;
        }

        public List<Assembly> GetByOrderId(Guid orderId)
        {
            var assemblies = new List<Assembly>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT a.Id, a.OrderId, a.ClampingId, a.PlateThickness, a.Hatve, a.Size, a.Length,
                             a.SerialNoId, a.MachineId, a.AssemblyCount, a.UsedClampCount, a.EmployeeId, a.AssemblyDate,
                             a.CreatedDate, a.ModifiedDate, a.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Assemblies a
                             LEFT JOIN SerialNos sn ON a.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON a.MachineId = m.Id
                             LEFT JOIN Employees e ON a.EmployeeId = e.Id
                             WHERE a.OrderId = @OrderId AND a.IsActive = 1
                             ORDER BY a.AssemblyDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            assemblies.Add(MapToAssembly(reader));
                        }
                    }
                }
            }
            
            return assemblies;
        }

        public Assembly GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT a.Id, a.OrderId, a.ClampingId, a.PlateThickness, a.Hatve, a.Size, a.Length,
                             a.SerialNoId, a.MachineId, a.AssemblyCount, a.UsedClampCount, a.EmployeeId, a.AssemblyDate,
                             a.CreatedDate, a.ModifiedDate, a.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Assemblies a
                             LEFT JOIN SerialNos sn ON a.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON a.MachineId = m.Id
                             LEFT JOIN Employees e ON a.EmployeeId = e.Id
                             WHERE a.Id = @Id AND a.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToAssembly(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Assembly assembly)
        {
            assembly.Id = Guid.NewGuid();
            assembly.CreatedDate = DateTime.Now;
            assembly.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Assemblies (Id, OrderId, ClampingId, PlateThickness, Hatve, Size, Length,
                             SerialNoId, MachineId, AssemblyCount, UsedClampCount, EmployeeId, AssemblyDate, CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @ClampingId, @PlateThickness, @Hatve, @Size, @Length,
                             @SerialNoId, @MachineId, @AssemblyCount, @UsedClampCount, @EmployeeId, @AssemblyDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddAssemblyParameters(command, assembly);
                    command.ExecuteNonQuery();
                }
            }
            
            return assembly.Id;
        }

        public void Update(Assembly assembly)
        {
            assembly.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Assemblies SET 
                             OrderId = @OrderId,
                             ClampingId = @ClampingId,
                             PlateThickness = @PlateThickness,
                             Hatve = @Hatve,
                             Size = @Size,
                             Length = @Length,
                             SerialNoId = @SerialNoId,
                             MachineId = @MachineId,
                             AssemblyCount = @AssemblyCount,
                             UsedClampCount = @UsedClampCount,
                             EmployeeId = @EmployeeId,
                             AssemblyDate = @AssemblyDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddAssemblyParameters(command, assembly);
                    command.Parameters.AddWithValue("@ModifiedDate", assembly.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Assemblies SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Assembly MapToAssembly(SqlDataReader reader)
        {
            var assembly = new Assembly
            {
                Id = reader.GetGuid("Id"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                Length = reader.GetDecimal("Length"),
                AssemblyCount = reader.GetInt32("AssemblyCount"),
                UsedClampCount = reader.GetInt32("UsedClampCount"),
                AssemblyDate = reader.GetDateTime("AssemblyDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                assembly.OrderId = reader.GetGuid("OrderId");
            }

            if (!reader.IsDBNull("ClampingId"))
            {
                assembly.ClampingId = reader.GetGuid("ClampingId");
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                assembly.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    assembly.SerialNo = new SerialNo
                    {
                        Id = assembly.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("MachineId"))
            {
                assembly.MachineId = reader.GetGuid("MachineId");
                if (!reader.IsDBNull("MachineName"))
                {
                    assembly.Machine = new Machine
                    {
                        Id = assembly.MachineId.Value,
                        Name = reader.GetString("MachineName")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                assembly.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    assembly.Employee = new Employee
                    {
                        Id = assembly.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            return assembly;
        }

        private void AddAssemblyParameters(SqlCommand command, Assembly assembly)
        {
            command.Parameters.AddWithValue("@Id", assembly.Id);
            command.Parameters.AddWithValue("@OrderId", assembly.OrderId.HasValue ? (object)assembly.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ClampingId", assembly.ClampingId.HasValue ? (object)assembly.ClampingId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PlateThickness", assembly.PlateThickness);
            command.Parameters.AddWithValue("@Hatve", assembly.Hatve);
            command.Parameters.AddWithValue("@Size", assembly.Size);
            command.Parameters.AddWithValue("@Length", assembly.Length);
            command.Parameters.AddWithValue("@SerialNoId", assembly.SerialNoId.HasValue ? (object)assembly.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MachineId", assembly.MachineId.HasValue ? (object)assembly.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@AssemblyCount", assembly.AssemblyCount);
            command.Parameters.AddWithValue("@UsedClampCount", assembly.UsedClampCount);
            command.Parameters.AddWithValue("@EmployeeId", assembly.EmployeeId.HasValue ? (object)assembly.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@AssemblyDate", assembly.AssemblyDate);
            command.Parameters.AddWithValue("@CreatedDate", assembly.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", assembly.IsActive);
        }
    }
}

