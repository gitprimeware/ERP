using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PressingRequestRepository
    {
        public List<PressingRequest> GetAll()
        {
            var requests = new List<PressingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.Hatve, pr.Size, pr.PlateThickness, pr.SerialNoId, pr.CuttingId,
                             pr.RequestedPressCount, pr.ActualPressCount, pr.ResultedPressCount, pr.PressNo, pr.Pressure, pr.WasteAmount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PressingRequests pr
                             LEFT JOIN SerialNos sn ON pr.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON pr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON pr.OrderId = o.Id
                             WHERE pr.IsActive = 1
                             ORDER BY pr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToPressingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<PressingRequest> GetByOrderId(Guid orderId)
        {
            var requests = new List<PressingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.Hatve, pr.Size, pr.PlateThickness, pr.SerialNoId, pr.CuttingId,
                             pr.RequestedPressCount, pr.ActualPressCount, pr.ResultedPressCount, pr.PressNo, pr.Pressure, pr.WasteAmount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PressingRequests pr
                             LEFT JOIN SerialNos sn ON pr.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON pr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON pr.OrderId = o.Id
                             WHERE pr.OrderId = @OrderId AND pr.IsActive = 1
                             ORDER BY pr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToPressingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<PressingRequest> GetPendingRequests()
        {
            var requests = new List<PressingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.Hatve, pr.Size, pr.PlateThickness, pr.SerialNoId, pr.CuttingId,
                             pr.RequestedPressCount, pr.ActualPressCount, pr.ResultedPressCount, pr.PressNo, pr.Pressure, pr.WasteAmount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PressingRequests pr
                             LEFT JOIN SerialNos sn ON pr.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON pr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON pr.OrderId = o.Id
                             WHERE pr.Status IN ('Beklemede', 'Presde') AND pr.IsActive = 1
                             ORDER BY pr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToPressingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public PressingRequest GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.Hatve, pr.Size, pr.PlateThickness, pr.SerialNoId, pr.CuttingId,
                             pr.RequestedPressCount, pr.ActualPressCount, pr.ResultedPressCount, pr.PressNo, pr.Pressure, pr.WasteAmount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PressingRequests pr
                             LEFT JOIN SerialNos sn ON pr.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON pr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON pr.OrderId = o.Id
                             WHERE pr.Id = @Id AND pr.IsActive = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToPressingRequest(reader);
                        }
                    }
                }
            }
            return null;
        }

        public Guid Insert(PressingRequest request)
        {
            request.Id = Guid.NewGuid();
            request.CreatedDate = DateTime.Now;
            request.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO PressingRequests (Id, OrderId, Hatve, Size, PlateThickness, SerialNoId, CuttingId,
                             RequestedPressCount, ActualPressCount, ResultedPressCount, PressNo, Pressure, WasteAmount,
                             EmployeeId, Status, RequestDate, CompletionDate, CreatedDate, IsActive)
                             VALUES (@Id, @OrderId, @Hatve, @Size, @PlateThickness, @SerialNoId, @CuttingId,
                             @RequestedPressCount, @ActualPressCount, @ResultedPressCount, @PressNo, @Pressure, @WasteAmount,
                             @EmployeeId, @Status, @RequestDate, @CompletionDate, @CreatedDate, @IsActive)";

                using (var command = new SqlCommand(query, connection))
                {
                    AddPressingRequestParameters(command, request);
                    command.ExecuteNonQuery();
                }
            }
            return request.Id;
        }

        public void Update(PressingRequest request)
        {
            request.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE PressingRequests SET
                             OrderId = @OrderId,
                             Hatve = @Hatve,
                             Size = @Size,
                             PlateThickness = @PlateThickness,
                             SerialNoId = @SerialNoId,
                             CuttingId = @CuttingId,
                             RequestedPressCount = @RequestedPressCount,
                             ActualPressCount = @ActualPressCount,
                             ResultedPressCount = @ResultedPressCount,
                             PressNo = @PressNo,
                             Pressure = @Pressure,
                             WasteAmount = @WasteAmount,
                             EmployeeId = @EmployeeId,
                             Status = @Status,
                             RequestDate = @RequestDate,
                             CompletionDate = @CompletionDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    AddPressingRequestParameters(command, request);
                    command.Parameters.AddWithValue("@ModifiedDate", request.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE PressingRequests SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private PressingRequest MapToPressingRequest(SqlDataReader reader)
        {
            var request = new PressingRequest
            {
                Id = reader.GetGuid("Id"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                RequestedPressCount = reader.GetInt32("RequestedPressCount"),
                ActualPressCount = reader.IsDBNull("ActualPressCount") ? (int?)null : reader.GetInt32("ActualPressCount"),
                ResultedPressCount = reader.IsDBNull("ResultedPressCount") ? (int?)null : reader.GetInt32("ResultedPressCount"),
                PressNo = reader.IsDBNull("PressNo") ? "" : reader.GetString("PressNo"),
                Pressure = reader.GetDecimal("Pressure"),
                WasteAmount = reader.GetDecimal("WasteAmount"),
                RequestDate = reader.GetDateTime("RequestDate"),
                CompletionDate = reader.IsDBNull("CompletionDate") ? (DateTime?)null : reader.GetDateTime("CompletionDate"),
                Status = reader.GetString("Status"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                request.OrderId = reader.GetGuid("OrderId");
                request.Order = new Order { Id = request.OrderId, TrexOrderNo = reader.GetString("TrexOrderNo") };
            }
            if (!reader.IsDBNull("SerialNoId"))
            {
                request.SerialNoId = reader.GetGuid("SerialNoId");
                request.SerialNo = new SerialNo { Id = request.SerialNoId.Value, SerialNumber = reader.GetString("SerialNumber") };
            }
            if (!reader.IsDBNull("CuttingId"))
            {
                request.CuttingId = reader.GetGuid("CuttingId");
            }
            if (!reader.IsDBNull("EmployeeId"))
            {
                request.EmployeeId = reader.GetGuid("EmployeeId");
                request.Employee = new Employee { Id = request.EmployeeId.Value, FirstName = reader.GetString("EmployeeFirstName"), LastName = reader.GetString("EmployeeLastName") };
            }

            return request;
        }

        private void AddPressingRequestParameters(SqlCommand command, PressingRequest request)
        {
            command.Parameters.AddWithValue("@Id", request.Id);
            command.Parameters.AddWithValue("@OrderId", request.OrderId);
            command.Parameters.AddWithValue("@Hatve", request.Hatve);
            command.Parameters.AddWithValue("@Size", request.Size);
            command.Parameters.AddWithValue("@PlateThickness", request.PlateThickness);
            command.Parameters.AddWithValue("@SerialNoId", request.SerialNoId.HasValue ? (object)request.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CuttingId", request.CuttingId.HasValue ? (object)request.CuttingId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RequestedPressCount", request.RequestedPressCount);
            command.Parameters.AddWithValue("@ActualPressCount", request.ActualPressCount.HasValue ? (object)request.ActualPressCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ResultedPressCount", request.ResultedPressCount.HasValue ? (object)request.ResultedPressCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@PressNo", (object)request.PressNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@Pressure", request.Pressure);
            command.Parameters.AddWithValue("@WasteAmount", request.WasteAmount);
            command.Parameters.AddWithValue("@EmployeeId", request.EmployeeId.HasValue ? (object)request.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
            command.Parameters.AddWithValue("@CompletionDate", request.CompletionDate.HasValue ? (object)request.CompletionDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
        }
    }
}

