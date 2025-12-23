using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class CuttingRepository
    {
        public List<Cutting> GetAll()
        {
            var cuttings = new List<Cutting>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // PlakaAdedi ve WasteCount kolonlarının varlığını kontrol et
                bool hasPlakaAdediColumn = ColumnExists(connection, "Cuttings", "PlakaAdedi");
                bool hasWasteCountColumn = ColumnExists(connection, "Cuttings", "WasteCount");
                
                string plakaAdediColumn = hasPlakaAdediColumn ? "c.PlakaAdedi," : "";
                string wasteCountColumn = hasWasteCountColumn ? "c.WasteCount," : "";
                
                var query = $@"SELECT c.Id, c.OrderId, c.Hatve, c.Size, c.MachineId, c.SerialNoId,
                             c.TotalKg, c.CutKg, c.CuttingCount, {plakaAdediColumn} {wasteCountColumn} c.WasteKg, c.RemainingKg, c.EmployeeId, c.CuttingDate,
                             c.CreatedDate, c.ModifiedDate, c.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Cuttings c
                             LEFT JOIN SerialNos sn ON c.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON c.MachineId = m.Id
                             LEFT JOIN Employees e ON c.EmployeeId = e.Id
                             WHERE c.IsActive = 1
                             ORDER BY c.CuttingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cuttings.Add(MapToCutting(reader, hasPlakaAdediColumn, hasWasteCountColumn));
                        }
                    }
                }
            }
            
            return cuttings;
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

        public List<Cutting> GetByOrderId(Guid orderId)
        {
            var cuttings = new List<Cutting>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // PlakaAdedi ve WasteCount kolonlarının varlığını kontrol et
                bool hasPlakaAdediColumn = ColumnExists(connection, "Cuttings", "PlakaAdedi");
                bool hasWasteCountColumn = ColumnExists(connection, "Cuttings", "WasteCount");
                
                string plakaAdediColumn = hasPlakaAdediColumn ? "c.PlakaAdedi," : "";
                string wasteCountColumn = hasWasteCountColumn ? "c.WasteCount," : "";
                
                var query = $@"SELECT c.Id, c.OrderId, c.Hatve, c.Size, c.MachineId, c.SerialNoId,
                             c.TotalKg, c.CutKg, c.CuttingCount, {plakaAdediColumn} {wasteCountColumn} c.WasteKg, c.RemainingKg, c.EmployeeId, c.CuttingDate,
                             c.CreatedDate, c.ModifiedDate, c.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Cuttings c
                             LEFT JOIN SerialNos sn ON c.SerialNoId = sn.Id
                             LEFT JOIN Machines m ON c.MachineId = m.Id
                             LEFT JOIN Employees e ON c.EmployeeId = e.Id
                             WHERE c.OrderId = @OrderId AND c.IsActive = 1
                             ORDER BY c.CuttingDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cuttings.Add(MapToCutting(reader, hasPlakaAdediColumn, hasWasteCountColumn));
                        }
                    }
                }
            }
            
            return cuttings;
        }

        public Guid Insert(Cutting cutting)
        {
            cutting.Id = Guid.NewGuid();
            cutting.CreatedDate = DateTime.Now;
            cutting.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // PlakaAdedi ve WasteCount kolonlarının varlığını kontrol et
                bool hasPlakaAdediColumn = ColumnExists(connection, "Cuttings", "PlakaAdedi");
                bool hasWasteCountColumn = ColumnExists(connection, "Cuttings", "WasteCount");
                
                string plakaAdediInsert = hasPlakaAdediColumn ? "PlakaAdedi," : "";
                string plakaAdediValue = hasPlakaAdediColumn ? "@PlakaAdedi," : "";
                string wasteCountInsert = hasWasteCountColumn ? "WasteCount," : "";
                string wasteCountValue = hasWasteCountColumn ? "@WasteCount," : "";
                
                var query = $@"INSERT INTO Cuttings (Id, OrderId, Hatve, Size, MachineId, SerialNoId,
                             TotalKg, CutKg, CuttingCount, {plakaAdediInsert} {wasteCountInsert} WasteKg, RemainingKg, EmployeeId, CuttingDate, CreatedDate, IsActive) 
                             VALUES (@Id, @OrderId, @Hatve, @Size, @MachineId, @SerialNoId,
                             @TotalKg, @CutKg, @CuttingCount, {plakaAdediValue} {wasteCountValue} @WasteKg, @RemainingKg, @EmployeeId, @CuttingDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddCuttingParameters(command, cutting, hasPlakaAdediColumn, hasWasteCountColumn);
                    command.ExecuteNonQuery();
                }
            }
            
            return cutting.Id;
        }

        public void Update(Cutting cutting)
        {
            cutting.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // PlakaAdedi ve WasteCount kolonlarının varlığını kontrol et
                bool hasPlakaAdediColumn = ColumnExists(connection, "Cuttings", "PlakaAdedi");
                bool hasWasteCountColumn = ColumnExists(connection, "Cuttings", "WasteCount");
                
                string plakaAdediUpdate = hasPlakaAdediColumn ? "PlakaAdedi = @PlakaAdedi," : "";
                string wasteCountUpdate = hasWasteCountColumn ? "WasteCount = @WasteCount," : "";
                
                var query = $@"UPDATE Cuttings SET 
                             OrderId = @OrderId,
                             Hatve = @Hatve,
                             Size = @Size,
                             MachineId = @MachineId,
                             SerialNoId = @SerialNoId,
                             TotalKg = @TotalKg,
                             CutKg = @CutKg,
                             CuttingCount = @CuttingCount,
                             {plakaAdediUpdate}
                             {wasteCountUpdate}
                             WasteKg = @WasteKg,
                             RemainingKg = @RemainingKg,
                             EmployeeId = @EmployeeId,
                             CuttingDate = @CuttingDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddCuttingParameters(command, cutting, hasPlakaAdediColumn, hasWasteCountColumn);
                    command.Parameters.AddWithValue("@ModifiedDate", cutting.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Cutting GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // PlakaAdedi ve WasteCount kolonlarının varlığını kontrol et
                bool hasPlakaAdediColumn = ColumnExists(connection, "Cuttings", "PlakaAdedi");
                bool hasWasteCountColumn = ColumnExists(connection, "Cuttings", "WasteCount");
                
                string plakaAdediColumn = hasPlakaAdediColumn ? "c.PlakaAdedi," : "";
                string wasteCountColumn = hasWasteCountColumn ? "c.WasteCount," : "";
                
                var query = $@"SELECT c.Id, c.OrderId, c.Hatve, c.Size, c.MachineId, c.SerialNoId,
                             c.TotalKg, c.CutKg, c.CuttingCount, {plakaAdediColumn} {wasteCountColumn} c.WasteKg, c.RemainingKg, c.EmployeeId, c.CuttingDate,
                             c.CreatedDate, c.ModifiedDate, c.IsActive,
                             sn.SerialNumber as SerialNumber,
                             m.Name as MachineName,
                             e.FirstName as EmployeeFirstName, e.LastName as EmployeeLastName
                             FROM Cuttings c
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
                            return MapToCutting(reader, hasPlakaAdediColumn, hasWasteCountColumn);
                        }
                    }
                }
            }
            
            return null;
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Cuttings SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Cutting MapToCutting(SqlDataReader reader, bool hasPlakaAdediColumn = true, bool hasWasteCountColumn = true)
        {
            var cutting = new Cutting
            {
                Id = reader.GetGuid("Id"),
                Hatve = reader.GetDecimal("Hatve"),
                Size = reader.GetDecimal("Size"),
                TotalKg = reader.GetDecimal("TotalKg"),
                CutKg = reader.GetDecimal("CutKg"),
                CuttingCount = reader.GetInt32("CuttingCount"),
                PlakaAdedi = hasPlakaAdediColumn && !reader.IsDBNull("PlakaAdedi") ? reader.GetInt32("PlakaAdedi") : 0,
                WasteCount = GetWasteCountValue(reader, hasWasteCountColumn),
                WasteKg = reader.GetDecimal("WasteKg"),
                RemainingKg = reader.GetDecimal("RemainingKg"),
                CuttingDate = reader.GetDateTime("CuttingDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("OrderId"))
            {
                cutting.OrderId = reader.GetGuid("OrderId");
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                cutting.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    cutting.SerialNo = new SerialNo
                    {
                        Id = cutting.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }

            if (!reader.IsDBNull("MachineId"))
            {
                cutting.MachineId = reader.GetGuid("MachineId");
                if (!reader.IsDBNull("MachineName"))
                {
                    cutting.Machine = new Machine
                    {
                        Id = cutting.MachineId.Value,
                        Name = reader.GetString("MachineName")
                    };
                }
            }

            if (!reader.IsDBNull("EmployeeId"))
            {
                cutting.EmployeeId = reader.GetGuid("EmployeeId");
                if (!reader.IsDBNull("EmployeeFirstName") && !reader.IsDBNull("EmployeeLastName"))
                {
                    cutting.Employee = new Employee
                    {
                        Id = cutting.EmployeeId.Value,
                        FirstName = reader.GetString("EmployeeFirstName"),
                        LastName = reader.GetString("EmployeeLastName")
                    };
                }
            }

            return cutting;
        }

        private int? GetWasteCountValue(SqlDataReader reader, bool hasWasteCountColumn)
        {
            if (!hasWasteCountColumn)
                return null;

            try
            {
                int ordinal = reader.GetOrdinal("WasteCount");
                if (reader.IsDBNull(ordinal))
                    return null;
                return reader.GetInt32(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private void AddCuttingParameters(SqlCommand command, Cutting cutting, bool includePlakaAdedi = true, bool includeWasteCount = true)
        {
            command.Parameters.AddWithValue("@Id", cutting.Id);
            command.Parameters.AddWithValue("@OrderId", cutting.OrderId.HasValue ? (object)cutting.OrderId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Hatve", cutting.Hatve);
            command.Parameters.AddWithValue("@Size", cutting.Size);
            command.Parameters.AddWithValue("@MachineId", cutting.MachineId.HasValue ? (object)cutting.MachineId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SerialNoId", cutting.SerialNoId.HasValue ? (object)cutting.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@TotalKg", cutting.TotalKg);
            command.Parameters.AddWithValue("@CutKg", cutting.CutKg);
            command.Parameters.AddWithValue("@CuttingCount", cutting.CuttingCount);
            
            if (includePlakaAdedi)
            {
                command.Parameters.AddWithValue("@PlakaAdedi", cutting.PlakaAdedi);
            }
            
            if (includeWasteCount)
            {
                command.Parameters.AddWithValue("@WasteCount", cutting.WasteCount.HasValue ? (object)cutting.WasteCount.Value : DBNull.Value);
            }
            command.Parameters.AddWithValue("@WasteKg", cutting.WasteKg);
            command.Parameters.AddWithValue("@RemainingKg", cutting.RemainingKg);
            command.Parameters.AddWithValue("@EmployeeId", cutting.EmployeeId.HasValue ? (object)cutting.EmployeeId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@CuttingDate", cutting.CuttingDate);
            command.Parameters.AddWithValue("@CreatedDate", cutting.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", cutting.IsActive);
        }
    }
}

