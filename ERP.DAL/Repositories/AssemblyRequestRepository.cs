using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class AssemblyRequestRepository
    {
        public List<AssemblyRequest> GetAll()
        {
            var requests = new List<AssemblyRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT ar.Id, ar.OrderId, ar.Hatve, ar.Size, ar.PlateThickness, ar.Length, ar.SerialNoId, ar.ClampingId,
                             ar.MachineId, ar.RequestedAssemblyCount, ar.ActualClampCount, ar.ResultedAssemblyCount,
                             ar.EmployeeId, ar.Status, ar.RequestDate, ar.CompletionDate,
                             ar.CreatedDate, ar.ModifiedDate, ar.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM AssemblyRequests ar
                             LEFT JOIN SerialNos sn ON ar.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON ar.EmployeeId = e.Id
                             LEFT JOIN Orders o ON ar.OrderId = o.Id
                             WHERE ar.IsActive = 1
                             ORDER BY ar.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToAssemblyRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<AssemblyRequest> GetByOrderId(Guid orderId)
        {
            var requests = new List<AssemblyRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT ar.Id, ar.OrderId, ar.Hatve, ar.Size, ar.PlateThickness, ar.Length, ar.SerialNoId, ar.ClampingId,
                             ar.MachineId, ar.RequestedAssemblyCount, ar.ActualClampCount, ar.ResultedAssemblyCount,
                             ar.EmployeeId, ar.Status, ar.RequestDate, ar.CompletionDate,
                             ar.CreatedDate, ar.ModifiedDate, ar.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM AssemblyRequests ar
                             LEFT JOIN SerialNos sn ON ar.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON ar.EmployeeId = e.Id
                             LEFT JOIN Orders o ON ar.OrderId = o.Id
                             WHERE ar.OrderId = @OrderId AND ar.IsActive = 1
                             ORDER BY ar.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToAssemblyRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public List<AssemblyRequest> GetPendingRequests()
        {
            var requests = new List<AssemblyRequest>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT ar.Id, ar.OrderId, ar.Hatve, ar.Size, ar.PlateThickness, ar.Length, ar.SerialNoId, ar.ClampingId,
                             ar.MachineId, ar.RequestedAssemblyCount, ar.ActualClampCount, ar.ResultedAssemblyCount,
                             ar.EmployeeId, ar.Status, ar.RequestDate, ar.CompletionDate,
                             ar.CreatedDate, ar.ModifiedDate, ar.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM AssemblyRequests ar
                             LEFT JOIN SerialNos sn ON ar.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON ar.EmployeeId = e.Id
                             LEFT JOIN Orders o ON ar.OrderId = o.Id
                             WHERE ar.Status IN ('Beklemede', 'Montajda') AND ar.IsActive = 1
                             ORDER BY ar.RequestDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requests.Add(MapToAssemblyRequest(reader));
                        }
                    }
                }
            }
            
            return requests;
        }

        public AssemblyRequest GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT ar.Id, ar.OrderId, ar.Hatve, ar.Size, ar.PlateThickness, ar.Length, ar.SerialNoId, ar.ClampingId,
                             ar.MachineId, ar.RequestedAssemblyCount, ar.ActualClampCount, ar.ResultedAssemblyCount,
                             ar.EmployeeId, ar.Status, ar.RequestDate, ar.CompletionDate,
                             ar.CreatedDate, ar.ModifiedDate, ar.IsActive,
                             sn.SerialNumber as SerialNumber,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName,
                             o.TrexOrderNo, o.ProductCode
                             FROM AssemblyRequests ar
                             LEFT JOIN SerialNos sn ON ar.SerialNoId = sn.Id
                             LEFT JOIN Employees e ON ar.EmployeeId = e.Id
                             LEFT JOIN Orders o ON ar.OrderId = o.Id
                             WHERE ar.Id = @Id AND ar.IsActive = 1";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToAssemblyRequest(reader);
                        }
                    }
                }
            }
            return null;
        }

        public Guid Insert(AssemblyRequest request)
        {
            request.Id = Guid.NewGuid();
            request.CreatedDate = DateTime.Now;
            request.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO AssemblyRequests (Id, OrderId, Hatve, Size, PlateThickness, Length, SerialNoId, ClampingId,
                             MachineId, RequestedAssemblyCount, ActualClampCount, ResultedAssemblyCount,
                             EmployeeId, Status, RequestDate, CompletionDate, CreatedDate, IsActive)
                             VALUES (@Id, @OrderId, @Hatve, @Size, @PlateThickness, @Length, @SerialNoId, @ClampingId,
                             @MachineId, @RequestedAssemblyCount, @ActualClampCount, @ResultedAssemblyCount,
                             @EmployeeId, @Status, @RequestDate, @CompletionDate, @CreatedDate, @IsActive)";

                using (var command = new SqlCommand(query, connection))
                {
                    AddAssemblyRequestParameters(command, request);
                    command.ExecuteNonQuery();
                }
            }
            return request.Id;
        }

        public void Update(AssemblyRequest request)
        {
            request.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE AssemblyRequests SET
                             OrderId = @OrderId,
                             Hatve = @Hatve,
                             Size = @Size,
                             PlateThickness = @PlateThickness,
                             Length = @Length,
                             SerialNoId = @SerialNoId,
                             ClampingId = @ClampingId,
                             MachineId = @MachineId,
                             RequestedAssemblyCount = @RequestedAssemblyCount,
                             ActualClampCount = @ActualClampCount,
                             ResultedAssemblyCount = @ResultedAssemblyCount,
                             EmployeeId = @EmployeeId,
                             Status = @Status,
                             RequestDate = @RequestDate,
                             CompletionDate = @CompletionDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    AddAssemblyRequestParameters(command, request);
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
                var query = "UPDATE AssemblyRequests SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private AssemblyRequest MapToAssemblyRequest(SqlDataReader reader)
        {
            var request = new AssemblyRequest
            {
                Id = reader.GetGuid("Id"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                PlateThickness = reader.GetDecimal("PlateThickness"),
                Length = reader.GetDecimal("Length"),
                RequestedAssemblyCount = reader.GetInt32("RequestedAssemblyCount"),
                ActualClampCount = reader.IsDBNull("ActualClampCount") ? (int?)null : reader.GetInt32("ActualClampCount"),
                ResultedAssemblyCount = reader.IsDBNull("ResultedAssemblyCount") ? (int?)null : reader.GetInt32("ResultedAssemblyCount"),
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
            if (!reader.IsDBNull("SerialNoId"))
            {
                request.SerialNoId = reader.GetGuid("SerialNoId");
                request.SerialNo = new SerialNo { Id = request.SerialNoId.Value, SerialNumber = reader.GetString("SerialNumber") };
            }
            if (!reader.IsDBNull("ClampingId"))
            {
                request.ClampingId = reader.GetGuid("ClampingId");
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

        private void AddAssemblyRequestParameters(SqlCommand command, AssemblyRequest request)
        {
            command.Parameters.AddWithValue("@Id", request.Id);
            command.Parameters.AddWithValue("@OrderId", request.OrderId.HasValue ? (object)request.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Hatve", request.Hatve);
            command.Parameters.AddWithValue("@Size", request.Size);
            command.Parameters.AddWithValue("@PlateThickness", request.PlateThickness);
            command.Parameters.AddWithValue("@Length", request.Length);
            command.Parameters.AddWithValue("@SerialNoId", request.SerialNoId.HasValue ? (object)request.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ClampingId", request.ClampingId.HasValue ? (object)request.ClampingId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@MachineId", request.MachineId.HasValue ? (object)request.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@RequestedAssemblyCount", request.RequestedAssemblyCount);
            command.Parameters.AddWithValue("@ActualClampCount", request.ActualClampCount.HasValue ? (object)request.ActualClampCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ResultedAssemblyCount", request.ResultedAssemblyCount.HasValue ? (object)request.ResultedAssemblyCount.Value : DBNull.Value);
            command.Parameters.AddWithValue("@EmployeeId", request.EmployeeId.HasValue ? (object)request.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@RequestDate", request.RequestDate);
            command.Parameters.AddWithValue("@CompletionDate", request.CompletionDate.HasValue ? (object)request.CompletionDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", request.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
        }
    }
}

