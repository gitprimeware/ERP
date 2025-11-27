using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PressingRepository
    {
        public List<Pressing> GetAll()
        {
            var pressings = new List<Pressing>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.Id, p.OrderId, p.PlateThickness, p.Hatve, p.Size, p.SerialNoId,
                             p.PressNo, p.Pressure, p.PressCount, p.WasteAmount, p.EmployeeId, p.PressingDate,
                             p.CreatedDate, p.ModifiedDate, p.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Pressings p
                             LEFT JOIN SerialNos sn ON p.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON p.EmployeeId = e.Id
                             WHERE p.IsActive = 1
                             ORDER BY p.PressingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pressings.Add(MapToPressing(reader));
                        }
                    }
                }
            }
            
            return pressings;
        }

        public List<Pressing> GetByOrderId(Guid orderId)
        {
            var pressings = new List<Pressing>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT p.Id, p.OrderId, p.PlateThickness, p.Hatve, p.Size, p.SerialNoId,
                             p.PressNo, p.Pressure, p.PressCount, p.WasteAmount, p.EmployeeId, p.PressingDate,
                             p.CreatedDate, p.ModifiedDate, p.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Pressings p
                             LEFT JOIN SerialNos sn ON p.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON p.EmployeeId = e.Id
                             WHERE p.OrderId = @OrderId AND p.IsActive = 1
                             ORDER BY p.PressingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pressings.Add(MapToPressing(reader));
                        }
                    }
                }
            }
            
            return pressings;
        }

        public Guid Insert(Pressing pressing)
        {
            pressing.Id = Guid.NewGuid();
            pressing.CreatedDate = DateTime.Now;
            pressing.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Pressings (Id, OrderId, PlateThickness, Hatve, Size, SerialNoId,
                             PressNo, Pressure, PressCount, WasteAmount, EmployeeId, PressingDate, CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @PlateThickness, @Hatve, @Size, @SerialNoId,
                             @PressNo, @Pressure, @PressCount, @WasteAmount, @EmployeeId, @PressingDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddPressingParameters(command, pressing);
                    command.ExecuteNonQuery();
                }
            }
            
            return pressing.Id;
        }

        public void Update(Pressing pressing)
        {
            pressing.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Pressings SET 
                             OrderId = @OrderId,
                             PlateThickness = @PlateThickness,
                             Hatve = @Hatve,
                             Size = @Size,
                             SerialNoId = @SerialNoId,
                             PressNo = @PressNo,
                             Pressure = @Pressure,
                             PressCount = @PressCount,
                             WasteAmount = @WasteAmount,
                             EmployeeId = @EmployeeId,
                             PressingDate = @PressingDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddPressingParameters(command, pressing);
                    command.Parameters.AddWithValue("@ModifiedDate", pressing.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Pressings SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Pressing MapToPressing(SqlDataReader reader)
        {
            var pressing = new Pressing
            {
                Id = reader.GetGuid("Id"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                PressNo = reader.IsDBNull("PressNo") ? "" : reader.GetString("PressNo"),
                Pressure = reader.GetDecimal("Pressure"),
                PressCount = reader.GetInt32("PressCount"),
                WasteAmount = reader.GetDecimal("WasteAmount"),
                PressingDate = reader.GetDateTime("PressingDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                pressing.OrderId = reader.GetGuid("OrderId");
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                pressing.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    pressing.SerialNo = new SerialNo
                    {
                        Id = pressing.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                pressing.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    pressing.Employee = new Employee
                    {
                        Id = pressing.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            return pressing;
        }

        private void AddPressingParameters(SqlCommand command, Pressing pressing)
        {
            command.Parameters.AddWithValue("@Id", pressing.Id);
            command.Parameters.AddWithValue("@OrderId", pressing.OrderId.HasValue ? (object)pressing.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PlateThickness", pressing.PlateThickness);
            command.Parameters.AddWithValue("@Hatve", pressing.Hatve);
            command.Parameters.AddWithValue("@Size", pressing.Size);
            command.Parameters.AddWithValue("@SerialNoId", pressing.SerialNoId.HasValue ? (object)pressing.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PressNo", (object)pressing.PressNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Pressure", pressing.Pressure);
            command.Parameters.AddWithValue("@PressCount", pressing.PressCount);
            command.Parameters.AddWithValue("@WasteAmount", pressing.WasteAmount);
            command.Parameters.AddWithValue("@EmployeeId", pressing.EmployeeId.HasValue ? (object)pressing.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PressingDate", pressing.PressingDate);
            command.Parameters.AddWithValue("@CreatedDate", pressing.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", pressing.IsActive);
        }
    }
}

