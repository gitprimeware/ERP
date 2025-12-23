using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class CuttingRequestRepository
    {
        public List<CuttingRequest> GetAll()
        {
            var requests = new List<CuttingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr.Id, cr.OrderId, cr.Hatve, cr.Size, cr.PlateThickness, cr.MachineId, cr.SerialNoId,
                             cr.RequestedPlateCount, cr.OnePlateWeight, cr.TotalRequiredPlateWeight, cr.RemainingKg,
                             cr.EmployeeId, cr.ActualCutCount, cr.WasteCount, cr.IsRollFinished, cr.Status, cr.RequestDate, cr.CompletionDate,
                             cr.CreatedDate, cr.ModifiedDate, cr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM CuttingRequests cr
                             LEFT JOIN SerialNos sn ON cr.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON cr.MachineId = m.Id
                             LEFT JOIN Employees e ON cr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr.OrderId = o.Id
                             WHERE cr.IsActive = 1
                             ORDER BY cr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToCuttingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<CuttingRequest> GetByOrderId(Guid orderId)
        {
            var requests = new List<CuttingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr.Id, cr.OrderId, cr.Hatve, cr.Size, cr.PlateThickness, cr.MachineId, cr.SerialNoId,
                             cr.RequestedPlateCount, cr.OnePlateWeight, cr.TotalRequiredPlateWeight, cr.RemainingKg,
                             cr.EmployeeId, cr.ActualCutCount, cr.WasteCount, cr.IsRollFinished, cr.Status, cr.RequestDate, cr.CompletionDate,
                             cr.CreatedDate, cr.ModifiedDate, cr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM CuttingRequests cr
                             LEFT JOIN SerialNos sn ON cr.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON cr.MachineId = m.Id
                             LEFT JOIN Employees e ON cr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr.OrderId = o.Id
                             WHERE cr.OrderId = @OrderId AND cr.IsActive = 1
                             ORDER BY cr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToCuttingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<CuttingRequest> GetPendingRequests()
        {
            var requests = new List<CuttingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr.Id, cr.OrderId, cr.Hatve, cr.Size, cr.PlateThickness, cr.MachineId, cr.SerialNoId,
                             cr.RequestedPlateCount, cr.OnePlateWeight, cr.TotalRequiredPlateWeight, cr.RemainingKg,
                             cr.EmployeeId, cr.ActualCutCount, cr.WasteCount, cr.IsRollFinished, cr.Status, cr.RequestDate, cr.CompletionDate,
                             cr.CreatedDate, cr.ModifiedDate, cr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM CuttingRequests cr
                             LEFT JOIN SerialNos sn ON cr.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON cr.MachineId = m.Id
                             LEFT JOIN Employees e ON cr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr.OrderId = o.Id
                             WHERE cr.Status IN ('Beklemede', 'Kesimde') AND cr.IsActive = 1
                             ORDER BY cr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToCuttingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public CuttingRequest GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr.Id, cr.OrderId, cr.Hatve, cr.Size, cr.PlateThickness, cr.MachineId, cr.SerialNoId,
                             cr.RequestedPlateCount, cr.OnePlateWeight, cr.TotalRequiredPlateWeight, cr.RemainingKg,
                             cr.EmployeeId, cr.ActualCutCount, cr.WasteCount, cr.IsRollFinished, cr.Status, cr.RequestDate, cr.CompletionDate,
                             cr.CreatedDate, cr.ModifiedDate, cr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM CuttingRequests cr
                             LEFT JOIN SerialNos sn ON cr.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON cr.MachineId = m.Id
                             LEFT JOIN Employees e ON cr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr.OrderId = o.Id
                             WHERE cr.Id = @Id AND cr.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToCuttingRequest(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(CuttingRequest request)
        {
            request.Id = Guid.NewGuid();
            request.CreatedDate = DateTime.Now;
            request.IsActive = true;
            request.RequestDate = DateTime.Now;
            if (string.IsNullOrEmpty(request.Status))
                request.Status = "Beklemede";

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO CuttingRequests (Id, OrderId, Hatve, Size, PlateThickness, MachineId, SerialNoId,
                             RequestedPlateCount, OnePlateWeight, TotalRequiredPlateWeight, RemainingKg,
                             EmployeeId, ActualCutCount, WasteCount, IsRollFinished, Status, RequestDate, CompletionDate,
                             CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @Hatve, @Size, @PlateThickness, @MachineId, @SerialNoId,
                             @RequestedPlateCount, @OnePlateWeight, @TotalRequiredPlateWeight, @RemainingKg,
                             @EmployeeId, @ActualCutCount, @WasteCount, @IsRollFinished, @Status, @RequestDate, @CompletionDate,
                             @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddCuttingRequestParameters(command, request);
                    command.ExecuteNonQuery();
                }
            }
            
            return request.Id;
        }

        public void Update(CuttingRequest request)
        {
            request.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE CuttingRequests SET 
                             OrderId = @OrderId,
                             Hatve = @Hatve,
                             Size = @Size,
                             PlateThickness = @PlateThickness,
                             MachineId = @MachineId,
                             SerialNoId = @SerialNoId,
                             RequestedPlateCount = @RequestedPlateCount,
                             OnePlateWeight = @OnePlateWeight,
                             TotalRequiredPlateWeight = @TotalRequiredPlateWeight,
                             RemainingKg = @RemainingKg,
                             EmployeeId = @EmployeeId,
                             ActualCutCount = @ActualCutCount,
                             WasteCount = @WasteCount,
                             IsRollFinished = @IsRollFinished,
                             Status = @Status,
                             RequestDate = @RequestDate,
                             CompletionDate = @CompletionDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddCuttingRequestParameters(command, request);
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
                var query = "UPDATE CuttingRequests SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private CuttingRequest MapToCuttingRequest(SqlDataReader reader)
        {
            var request = new CuttingRequest
            {
                Id = reader.GetGuid("Id"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                RequestedPlateCount = reader.GetInt32("RequestedPlateCount"),
                OnePlateWeight = reader.GetDecimal("OnePlateWeight"),
                TotalRequiredPlateWeight = reader.GetDecimal("TotalRequiredPlateWeight"),
                RemainingKg = reader.GetDecimal("RemainingKg"),
                IsRollFinished = reader.GetBoolean("IsRollFinished"),
                Status = reader.GetString("Status"),
                RequestDate = reader.GetDateTime("RequestDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                request.OrderId = reader.GetGuid("OrderId");
                if (!reader.IsDBNull("TrexOrderNo"))
                {
                    request.Order = new Order
                    {
                        Id = request.OrderId,
                        TrexOrderNo = reader.GetString("TrexOrderNo"),
                        ProductCode = reader.IsDBNull("ProductCode") ? null : reader.GetString("ProductCode")
                    };
                }
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                request.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    request.SerialNo = new SerialNo
                    {
                        Id = request.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("MachineId"))
            {
                request.MachineId = reader.GetGuid("MachineId");
                if (!reader.IsDBNull("MachineName"))
                {
                    request.Machine = new Machine
                    {
                        Id = request.MachineId.Value,
                        Name = reader.GetString("MachineName")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                request.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    request.Employee = new Employee
                    {
                        Id = request.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            if (!reader.IsDBNull("ActualCutCount"))
            {
                request.ActualCutCount = reader.GetInt32("ActualCutCount");
            }

            if (!reader.IsDBNull("WasteCount"))
            {
                request.WasteCount = reader.GetInt32("WasteCount");
            }

            if (!reader.IsDBNull("CompletionDate"))
            {
                request.CompletionDate = reader.GetDateTime("CompletionDate");
            }

            return request;
        }

        private void AddCuttingRequestParameters(SqlCommand command, CuttingRequest request)
        {
            command.Parameters.AddWithValue("@Id", request.Id);
            command.Parameters.AddWithValue("@OrderId", request.OrderId);
            command.Parameters.AddWithValue("@Hatve", request.Hatve);
            command.Parameters.AddWithValue("@Size", request.Size);
            command.Parameters.AddWithValue("@PlateThickness", request.PlateThickness);
            command.Parameters.AddWithValue("@MachineId", request.MachineId.HasValue ? (object)request.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SerialNoId", request.SerialNoId.HasValue ? (object)request.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RequestedPlateCount", request.RequestedPlateCount);
            command.Parameters.AddWithValue("@OnePlateWeight", request.OnePlateWeight);
            command.Parameters.AddWithValue("@TotalRequiredPlateWeight", request.TotalRequiredPlateWeight);
            command.Parameters.AddWithValue("@RemainingKg", request.RemainingKg);
            command.Parameters.AddWithValue("@EmployeeId", request.EmployeeId.HasValue ? (object)request.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ActualCutCount", request.ActualCutCount.HasValue ? (object)request.ActualCutCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@WasteCount", request.WasteCount.HasValue ? (object)request.WasteCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsRollFinished", request.IsRollFinished);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
            command.Parameters.AddWithValue("@CompletionDate", request.CompletionDate.HasValue ? (object)request.CompletionDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
        }
    }
}

