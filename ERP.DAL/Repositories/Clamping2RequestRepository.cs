using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class Clamping2RequestRepository
    {
        public List<Clamping2Request> GetAll()
        {
            var requests = new List<Clamping2Request>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr2.Id, cr2.OrderId, cr2.Hatve, cr2.PlateThickness, cr2.FirstClampingId, cr2.SecondClampingId,
                             cr2.ResultedSize, cr2.ResultedLength, cr2.MachineId, cr2.RequestedCount, cr2.ActualCount, cr2.ResultedCount,
                             cr2.EmployeeId, cr2.Status, cr2.RequestDate, cr2.CompletionDate,
                             cr2.CreatedDate, cr2.ModifiedDate, cr2.IsActive,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode,
                             c1.Size as FirstSize, c1.Length as FirstLength,
                             c2.Size as SecondSize, c2.Length as SecondLength
                             FROM Clamping2Requests cr2
                             LEFT JOIN Employees e ON cr2.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr2.OrderId = o.Id
                             LEFT JOIN Clampings c1 ON cr2.FirstClampingId = c1.Id
                             LEFT JOIN Clampings c2 ON cr2.SecondClampingId = c2.Id
                             WHERE cr2.IsActive = 1
                             ORDER BY cr2.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToClamping2Request(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<Clamping2Request> GetByOrderId(Guid orderId)
        {
            var requests = new List<Clamping2Request>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr2.Id, cr2.OrderId, cr2.Hatve, cr2.PlateThickness, cr2.FirstClampingId, cr2.SecondClampingId,
                             cr2.ResultedSize, cr2.ResultedLength, cr2.MachineId, cr2.RequestedCount, cr2.ActualCount, cr2.ResultedCount,
                             cr2.EmployeeId, cr2.Status, cr2.RequestDate, cr2.CompletionDate,
                             cr2.CreatedDate, cr2.ModifiedDate, cr2.IsActive,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode,
                             c1.Size as FirstSize, c1.Length as FirstLength,
                             c2.Size as SecondSize, c2.Length as SecondLength
                             FROM Clamping2Requests cr2
                             LEFT JOIN Employees e ON cr2.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr2.OrderId = o.Id
                             LEFT JOIN Clampings c1 ON cr2.FirstClampingId = c1.Id
                             LEFT JOIN Clampings c2 ON cr2.SecondClampingId = c2.Id
                             WHERE cr2.OrderId = @OrderId AND cr2.IsActive = 1
                             ORDER BY cr2.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToClamping2Request(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<Clamping2Request> GetPendingRequests()
        {
            var requests = new List<Clamping2Request>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr2.Id, cr2.OrderId, cr2.Hatve, cr2.PlateThickness, cr2.FirstClampingId, cr2.SecondClampingId,
                             cr2.ResultedSize, cr2.ResultedLength, cr2.MachineId, cr2.RequestedCount, cr2.ActualCount, cr2.ResultedCount,
                             cr2.EmployeeId, cr2.Status, cr2.RequestDate, cr2.CompletionDate,
                             cr2.CreatedDate, cr2.ModifiedDate, cr2.IsActive,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode,
                             c1.Size as FirstSize, c1.Length as FirstLength,
                             c2.Size as SecondSize, c2.Length as SecondLength
                             FROM Clamping2Requests cr2
                             LEFT JOIN Employees e ON cr2.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr2.OrderId = o.Id
                             LEFT JOIN Clampings c1 ON cr2.FirstClampingId = c1.Id
                             LEFT JOIN Clampings c2 ON cr2.SecondClampingId = c2.Id
                             WHERE cr2.Status IN ('Beklemede', 'Kenetmede') AND cr2.IsActive = 1
                             ORDER BY cr2.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToClamping2Request(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public Clamping2Request GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cr2.Id, cr2.OrderId, cr2.Hatve, cr2.PlateThickness, cr2.FirstClampingId, cr2.SecondClampingId,
                             cr2.ResultedSize, cr2.ResultedLength, cr2.MachineId, cr2.RequestedCount, cr2.ActualCount, cr2.ResultedCount,
                             cr2.EmployeeId, cr2.Status, cr2.RequestDate, cr2.CompletionDate,
                             cr2.CreatedDate, cr2.ModifiedDate, cr2.IsActive,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode,
                             c1.Size as FirstSize, c1.Length as FirstLength,
                             c2.Size as SecondSize, c2.Length as SecondLength
                             FROM Clamping2Requests cr2
                             LEFT JOIN Employees e ON cr2.EmployeeId = e.Id
                             LEFT JOIN Orders o ON cr2.OrderId = o.Id
                             LEFT JOIN Clampings c1 ON cr2.FirstClampingId = c1.Id
                             LEFT JOIN Clampings c2 ON cr2.SecondClampingId = c2.Id
                             WHERE cr2.Id = @Id AND cr2.IsActive = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToClamping2Request(reader);
                        }
                    }
                }
            }
            return null;
        }

        public Guid Insert(Clamping2Request request)
        {
            request.Id = Guid.NewGuid();
            request.CreatedDate = DateTime.Now;
            request.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Clamping2Requests (Id, OrderId, Hatve, PlateThickness, FirstClampingId, SecondClampingId,
                             ResultedSize, ResultedLength, MachineId, RequestedCount, ActualCount, ResultedCount,
                             EmployeeId, Status, RequestDate, CompletionDate, CreatedDate, IsActive)
                             VALUES (@Id, @OrderId, @Hatve, @PlateThickness, @FirstClampingId, @SecondClampingId,
                             @ResultedSize, @ResultedLength, @MachineId, @RequestedCount, @ActualCount, @ResultedCount,
                             @EmployeeId, @Status, @RequestDate, @CompletionDate, @CreatedDate, @IsActive)";

                using (var command = new SqlCommand(query, connection))
                {
                    AddClamping2RequestParameters(command, request);
                    command.ExecuteNonQuery();
                }
            }
            return request.Id;
        }

        public void Update(Clamping2Request request)
        {
            request.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Clamping2Requests SET
                             OrderId = @OrderId,
                             Hatve = @Hatve,
                             PlateThickness = @PlateThickness,
                             FirstClampingId = @FirstClampingId,
                             SecondClampingId = @SecondClampingId,
                             ResultedSize = @ResultedSize,
                             ResultedLength = @ResultedLength,
                             MachineId = @MachineId,
                             RequestedCount = @RequestedCount,
                             ActualCount = @ActualCount,
                             ResultedCount = @ResultedCount,
                             EmployeeId = @EmployeeId,
                             Status = @Status,
                             RequestDate = @RequestDate,
                             CompletionDate = @CompletionDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    AddClamping2RequestParameters(command, request);
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
                var query = "UPDATE Clamping2Requests SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Clamping2Request MapToClamping2Request(SqlDataReader reader)
        {
            var request = new Clamping2Request
            {
                Id = reader.GetGuid("Id"),
                Hatve = reader.GetDecimal("Hatve"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                ResultedSize = reader.GetDecimal("ResultedSize"),
                ResultedLength = reader.GetDecimal("ResultedLength"),
                RequestedCount = reader.GetInt32("RequestedCount"),
                ActualCount = reader.IsDBNull("ActualCount") ? (int?)null : reader.GetInt32("ActualCount"),
                ResultedCount = reader.IsDBNull("ResultedCount") ? (int?)null : reader.GetInt32("ResultedCount"),
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
                request.Order = new Order { Id = request.OrderId.Value, TrexOrderNo = reader.GetString("TrexOrderNo") };
            }
            if (!reader.IsDBNull("FirstClampingId"))
            {
                request.FirstClampingId = reader.GetGuid("FirstClampingId");
                request.FirstClamping = new Clamping 
                { 
                    Id = request.FirstClampingId.Value, 
                    Size = reader.GetDecimal("FirstSize"),
                    Length = reader.GetDecimal("FirstLength")
                };
            }
            if (!reader.IsDBNull("SecondClampingId"))
            {
                request.SecondClampingId = reader.GetGuid("SecondClampingId");
                request.SecondClamping = new Clamping 
                { 
                    Id = request.SecondClampingId.Value, 
                    Size = reader.GetDecimal("SecondSize"),
                    Length = reader.GetDecimal("SecondLength")
                };
            }
            if (!reader.IsDBNull("MachineId"))
            {
                request.MachineId = reader.GetGuid("MachineId");
            }
            if (!reader.IsDBNull("EmployeeId"))
            {
                request.EmployeeId = reader.GetGuid("EmployeeId");
                request.Employee = new Employee { Id = request.EmployeeId.Value, FirstName = reader.GetString("EmployeeFirstName"), LastName = reader.GetString("EmployeeLastName") };
            }

            return request;
        }

        private void AddClamping2RequestParameters(SqlCommand command, Clamping2Request request)
        {
            command.Parameters.AddWithValue("@Id", request.Id);
            command.Parameters.AddWithValue("@OrderId", request.OrderId.HasValue ? (object)request.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Hatve", request.Hatve);
            command.Parameters.AddWithValue("@PlateThickness", request.PlateThickness);
            command.Parameters.AddWithValue("@FirstClampingId", request.FirstClampingId.HasValue ? (object)request.FirstClampingId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SecondClampingId", request.SecondClampingId.HasValue ? (object)request.SecondClampingId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ResultedSize", request.ResultedSize);
            command.Parameters.AddWithValue("@ResultedLength", request.ResultedLength);
            command.Parameters.AddWithValue("@MachineId", request.MachineId.HasValue ? (object)request.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RequestedCount", request.RequestedCount);
            command.Parameters.AddWithValue("@ActualCount", request.ActualCount.HasValue ? (object)request.ActualCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ResultedCount", request.ResultedCount.HasValue ? (object)request.ResultedCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeId", request.EmployeeId.HasValue ? (object)request.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
            command.Parameters.AddWithValue("@CompletionDate", request.CompletionDate.HasValue ? (object)request.CompletionDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
        }
    }
}

