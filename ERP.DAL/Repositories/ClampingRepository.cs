using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class ClampingRepository
    {
        public List<Clamping> GetAll()
        {
            var clampings = new List<Clamping>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT c.Id, c.OrderId, c.PressingId, c.PlateThickness, c.Hatve, c.Size, c.Length,
                             c.SerialNoId, c.MachineId, c.ClampCount, c.UsedPlateCount, c.EmployeeId, c.ClampingDate,
                             c.CreatedDate, c.ModifiedDate, c.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Clampings c
                             LEFT JOIN SerialNos sn ON c.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON c.MachineId = m.Id
                             LEFT JOIN Employees e ON c.EmployeeId = e.Id
                             WHERE c.IsActive = 1
                             ORDER BY c.ClampingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clampings.Add(MapToClamping(reader));
                        }
                    }
                }
            }
            
            return clampings;
        }

        public List<Clamping> GetByOrderId(Guid orderId)
        {
            var clampings = new List<Clamping>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT c.Id, c.OrderId, c.PressingId, c.PlateThickness, c.Hatve, c.Size, c.Length,
                             c.SerialNoId, c.MachineId, c.ClampCount, c.UsedPlateCount, c.EmployeeId, c.ClampingDate,
                             c.CreatedDate, c.ModifiedDate, c.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Clampings c
                             LEFT JOIN SerialNos sn ON c.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON c.MachineId = m.Id
                             LEFT JOIN Employees e ON c.EmployeeId = e.Id
                             WHERE c.OrderId = @OrderId AND c.IsActive = 1
                             ORDER BY c.ClampingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clampings.Add(MapToClamping(reader));
                        }
                    }
                }
            }
            
            return clampings;
        }

        public Clamping GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT c.Id, c.OrderId, c.PressingId, c.PlateThickness, c.Hatve, c.Size, c.Length,
                             c.SerialNoId, c.MachineId, c.ClampCount, c.UsedPlateCount, c.EmployeeId, c.ClampingDate,
                             c.CreatedDate, c.ModifiedDate, c.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Clampings c
                             LEFT JOIN SerialNos sn ON c.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON c.MachineId = m.Id
                             LEFT JOIN Employees e ON c.EmployeeId = e.Id
                             WHERE c.Id = @Id AND c.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToClamping(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Clamping clamping)
        {
            clamping.Id = Guid.NewGuid();
            clamping.CreatedDate = DateTime.Now;
            clamping.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Clampings (Id, OrderId, PressingId, PlateThickness, Hatve, Size, Length,
                             SerialNoId, MachineId, ClampCount, UsedPlateCount, EmployeeId, ClampingDate, CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @PressingId, @PlateThickness, @Hatve, @Size, @Length,
                             @SerialNoId, @MachineId, @ClampCount, @UsedPlateCount, @EmployeeId, @ClampingDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddClampingParameters(command, clamping);
                    command.ExecuteNonQuery();
                }
            }
            
            return clamping.Id;
        }

        public void Update(Clamping clamping)
        {
            clamping.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Clampings SET 
                             OrderId = @OrderId,
                             PressingId = @PressingId,
                             PlateThickness = @PlateThickness,
                             Hatve = @Hatve,
                             Size = @Size,
                             Length = @Length,
                             SerialNoId = @SerialNoId,
                             MachineId = @MachineId,
                             ClampCount = @ClampCount,
                             UsedPlateCount = @UsedPlateCount,
                             EmployeeId = @EmployeeId,
                             ClampingDate = @ClampingDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddClampingParameters(command, clamping);
                    command.Parameters.AddWithValue("@ModifiedDate", clamping.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Clampings SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Clamping MapToClamping(SqlDataReader reader)
        {
            var clamping = new Clamping
            {
                Id = reader.GetGuid("Id"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                Length = reader.GetDecimal("Length"),
                ClampCount = reader.GetInt32("ClampCount"),
                UsedPlateCount = reader.GetInt32("UsedPlateCount"),
                ClampingDate = reader.GetDateTime("ClampingDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                clamping.OrderId = reader.GetGuid("OrderId");
            }

            if (!reader.IsDBNull("PressingId"))
            {
                clamping.PressingId = reader.GetGuid("PressingId");
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                clamping.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    clamping.SerialNo = new SerialNo
                    {
                        Id = clamping.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("MachineId"))
            {
                clamping.MachineId = reader.GetGuid("MachineId");
                if (!reader.IsDBNull("MachineName"))
                {
                    clamping.Machine = new Machine
                    {
                        Id = clamping.MachineId.Value,
                        Name = reader.GetString("MachineName")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                clamping.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    clamping.Employee = new Employee
                    {
                        Id = clamping.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            return clamping;
        }

        private void AddClampingParameters(SqlCommand command, Clamping clamping)
        {
            command.Parameters.AddWithValue("@Id", clamping.Id);
            command.Parameters.AddWithValue("@OrderId", clamping.OrderId.HasValue ? (object)clamping.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PressingId", clamping.PressingId.HasValue ? (object)clamping.PressingId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PlateThickness", clamping.PlateThickness);
            command.Parameters.AddWithValue("@Hatve", clamping.Hatve);
            command.Parameters.AddWithValue("@Size", clamping.Size);
            command.Parameters.AddWithValue("@Length", clamping.Length);
            command.Parameters.AddWithValue("@SerialNoId", clamping.SerialNoId.HasValue ? (object)clamping.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MachineId", clamping.MachineId.HasValue ? (object)clamping.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ClampCount", clamping.ClampCount);
            command.Parameters.AddWithValue("@UsedPlateCount", clamping.UsedPlateCount);
            command.Parameters.AddWithValue("@EmployeeId", clamping.EmployeeId.HasValue ? (object)clamping.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ClampingDate", clamping.ClampingDate);
            command.Parameters.AddWithValue("@CreatedDate", clamping.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", clamping.IsActive);
        }
    }
}

