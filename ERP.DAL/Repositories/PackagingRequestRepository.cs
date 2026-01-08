using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PackagingRequestRepository
    {
        public List<PackagingRequest> GetAll()
        {
            var requests = new List<PackagingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.IsolationId, pr.Hatve, pr.Size, pr.PlateThickness, pr.Length, pr.SerialNoId,
                             pr.MachineId, pr.RequestedPackagingCount, pr.ActualPackagingCount, pr.UsedIsolationCount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PackagingRequests pr
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
                            requests.Add(MapToPackagingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<PackagingRequest> GetByOrderId(Guid orderId)
        {
            var requests = new List<PackagingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.IsolationId, pr.Hatve, pr.Size, pr.PlateThickness, pr.Length, pr.SerialNoId,
                             pr.MachineId, pr.RequestedPackagingCount, pr.ActualPackagingCount, pr.UsedIsolationCount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PackagingRequests pr
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
                            requests.Add(MapToPackagingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<PackagingRequest> GetPendingRequests()
        {
            var requests = new List<PackagingRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.IsolationId, pr.Hatve, pr.Size, pr.PlateThickness, pr.Length, pr.SerialNoId,
                             pr.MachineId, pr.RequestedPackagingCount, pr.ActualPackagingCount, pr.UsedIsolationCount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PackagingRequests pr
                             LEFT JOIN SerialNos sn ON pr.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON pr.EmployeeId = e.Id
                             LEFT JOIN Orders o ON pr.OrderId = o.Id
                             WHERE pr.Status IN ('Beklemede', 'Paketlemede') AND pr.IsActive = 1
                             ORDER BY pr.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToPackagingRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public PackagingRequest GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT pr.Id, pr.OrderId, pr.IsolationId, pr.Hatve, pr.Size, pr.PlateThickness, pr.Length, pr.SerialNoId,
                             pr.MachineId, pr.RequestedPackagingCount, pr.ActualPackagingCount, pr.UsedIsolationCount,
                             pr.EmployeeId, pr.Status, pr.RequestDate, pr.CompletionDate,
                             pr.CreatedDate, pr.ModifiedDate, pr.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM PackagingRequests pr
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
                            return MapToPackagingRequest(reader);
                        }
                    }
                }
            }
            return null;
        }

        public Guid Insert(PackagingRequest request)
        {
            request.Id = Guid.NewGuid();
            request.CreatedDate = DateTime.Now;
            request.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO PackagingRequests (Id, OrderId, IsolationId, Hatve, Size, PlateThickness, Length, SerialNoId,
                             MachineId, RequestedPackagingCount, ActualPackagingCount, UsedIsolationCount,
                             EmployeeId, Status, RequestDate, CompletionDate, CreatedDate, IsActive)
                             VALUES (@Id, @OrderId, @IsolationId, @Hatve, @Size, @PlateThickness, @Length, @SerialNoId,
                             @MachineId, @RequestedPackagingCount, @ActualPackagingCount, @UsedIsolationCount,
                             @EmployeeId, @Status, @RequestDate, @CompletionDate, @CreatedDate, @IsActive)";

                using (var command = new SqlCommand(query, connection))
                {
                    AddPackagingRequestParameters(command, request);
                    command.ExecuteNonQuery();
                }
            }
            return request.Id;
        }

        public void Update(PackagingRequest request)
        {
            request.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE PackagingRequests SET
                             OrderId = @OrderId,
                             IsolationId = @IsolationId,
                             Hatve = @Hatve,
                             Size = @Size,
                             PlateThickness = @PlateThickness,
                             Length = @Length,
                             SerialNoId = @SerialNoId,
                             MachineId = @MachineId,
                             RequestedPackagingCount = @RequestedPackagingCount,
                             ActualPackagingCount = @ActualPackagingCount,
                             UsedIsolationCount = @UsedIsolationCount,
                             EmployeeId = @EmployeeId,
                             Status = @Status,
                             RequestDate = @RequestDate,
                             CompletionDate = @CompletionDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    AddPackagingRequestParameters(command, request);
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
                var query = "UPDATE PackagingRequests SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private PackagingRequest MapToPackagingRequest(SqlDataReader reader)
        {
            var request = new PackagingRequest
            {
                Id = reader.GetGuid("Id"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Length = reader.GetDecimal("Length"),
                RequestedPackagingCount = reader.GetInt32("RequestedPackagingCount"),
                ActualPackagingCount = reader.IsDBNull("ActualPackagingCount") ? (int?)null : reader.GetInt32("ActualPackagingCount"),
                UsedIsolationCount = reader.IsDBNull("UsedIsolationCount") ? (int?)null : reader.GetInt32("UsedIsolationCount"),
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
            if (!reader.IsDBNull("IsolationId"))
            {
                request.IsolationId = reader.GetGuid("IsolationId");
            }
            if (!reader.IsDBNull("SerialNoId"))
            {
                request.SerialNoId = reader.GetGuid("SerialNoId");
                request.SerialNo = new SerialNo { Id = request.SerialNoId.Value, SerialNumber = reader.GetString("SerialNumber") };
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

        private void AddPackagingRequestParameters(SqlCommand command, PackagingRequest request)
        {
            command.Parameters.AddWithValue("@Id", request.Id);
            command.Parameters.AddWithValue("@OrderId", request.OrderId.HasValue ? (object)request.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@IsolationId", request.IsolationId.HasValue ? (object)request.IsolationId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Hatve", request.Hatve);
            command.Parameters.AddWithValue("@Size", request.Size);
            command.Parameters.AddWithValue("@PlateThickness", request.PlateThickness);
            command.Parameters.AddWithValue("@Length", request.Length);
            command.Parameters.AddWithValue("@SerialNoId", request.SerialNoId.HasValue ? (object)request.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MachineId", request.MachineId.HasValue ? (object)request.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RequestedPackagingCount", request.RequestedPackagingCount);
            command.Parameters.AddWithValue("@ActualPackagingCount", request.ActualPackagingCount.HasValue ? (object)request.ActualPackagingCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@UsedIsolationCount", request.UsedIsolationCount.HasValue ? (object)request.UsedIsolationCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeId", request.EmployeeId.HasValue ? (object)request.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
            command.Parameters.AddWithValue("@CompletionDate", request.CompletionDate.HasValue ? (object)request.CompletionDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
        }
    }
}

