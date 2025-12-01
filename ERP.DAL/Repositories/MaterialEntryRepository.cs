using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class MaterialEntryRepository
    {
        public List<MaterialEntry> GetAll(string searchTerm = null, Guid? supplierId = null)
        {
            var entries = new List<MaterialEntry>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT me.Id, me.TransactionType, me.MaterialType, me.MaterialSize, me.Size, me.Thickness,
                             me.SupplierId, me.SerialNoId, me.InvoiceNo, me.TrexPurchaseNo, me.EntryDate, me.Quantity,
                             me.CreatedDate, me.ModifiedDate, me.IsActive,
                             s.Name as SupplierName,
                             sn.SerialNumber as SerialNumber
                             FROM MaterialEntries me
                             LEFT JOIN Suppliers s ON me.SupplierId = s.Id
                             LEFT JOIN SerialNos sn ON me.SerialNoId = sn.Id
                             WHERE me.IsActive = 1";
                
                var parameters = new List<SqlParameter>();
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query += " AND (me.TransactionType LIKE @SearchTerm OR me.MaterialType LIKE @SearchTerm OR me.MaterialSize LIKE @SearchTerm OR me.InvoiceNo LIKE @SearchTerm OR me.TrexPurchaseNo LIKE @SearchTerm OR s.Name LIKE @SearchTerm)";
                    parameters.Add(new SqlParameter("@SearchTerm", $"%{searchTerm}%"));
                }
                
                if (supplierId.HasValue)
                {
                    query += " AND me.SupplierId = @SupplierId";
                    parameters.Add(new SqlParameter("@SupplierId", supplierId.Value));
                }
                
                query += " ORDER BY me.EntryDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entries.Add(MapToMaterialEntry(reader));
                        }
                    }
                }
            }
            
            return entries;
        }

        public MaterialEntry GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT me.Id, me.TransactionType, me.MaterialType, me.MaterialSize, me.Size, me.Thickness,
                             me.SupplierId, me.SerialNoId, me.InvoiceNo, me.TrexPurchaseNo, me.EntryDate, me.Quantity,
                             me.CreatedDate, me.ModifiedDate, me.IsActive,
                             s.Name as SupplierName,
                             sn.SerialNumber as SerialNumber
                             FROM MaterialEntries me
                             LEFT JOIN Suppliers s ON me.SupplierId = s.Id
                             LEFT JOIN SerialNos sn ON me.SerialNoId = sn.Id
                             WHERE me.Id = @Id AND me.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToMaterialEntry(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(MaterialEntry entry)
        {
            entry.Id = Guid.NewGuid();
            entry.CreatedDate = DateTime.Now;
            entry.IsActive = true;
            if (entry.EntryDate == default(DateTime))
            {
                entry.EntryDate = DateTime.Now;
            }

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO MaterialEntries (Id, TransactionType, MaterialType, MaterialSize, Size, Thickness,
                             SupplierId, SerialNoId, InvoiceNo, TrexPurchaseNo, EntryDate, Quantity, CreatedDate, IsActive) 
                             VALUES (@Id, @TransactionType, @MaterialType, @MaterialSize, @Size, @Thickness,
                             @SupplierId, @SerialNoId, @InvoiceNo, @TrexPurchaseNo, @EntryDate, @Quantity, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddMaterialEntryParameters(command, entry);
                    command.ExecuteNonQuery();
                }
            }
            
            return entry.Id;
        }

        public void Update(MaterialEntry entry)
        {
            entry.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE MaterialEntries SET 
                             TransactionType = @TransactionType,
                             MaterialType = @MaterialType,
                             MaterialSize = @MaterialSize,
                             Size = @Size,
                             Thickness = @Thickness,
                             SupplierId = @SupplierId,
                             SerialNoId = @SerialNoId,
                             InvoiceNo = @InvoiceNo,
                             TrexPurchaseNo = @TrexPurchaseNo,
                             EntryDate = @EntryDate,
                             Quantity = @Quantity,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddMaterialEntryParameters(command, entry);
                    command.Parameters.AddWithValue("@ModifiedDate", entry.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE MaterialEntries SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private MaterialEntry MapToMaterialEntry(SqlDataReader reader)
        {
            var entry = new MaterialEntry
            {
                Id = reader.GetGuid("Id"),
                TransactionType = reader.GetString("TransactionType"),
                MaterialType = reader.GetString("MaterialType"),
                MaterialSize = reader.GetString("MaterialSize"),
                Size = reader.GetInt32("Size"),
                Thickness = reader.GetDecimal("Thickness"),
                InvoiceNo = reader.IsDBNull("InvoiceNo") ? null : reader.GetString("InvoiceNo"),
                TrexPurchaseNo = reader.IsDBNull("TrexPurchaseNo") ? null : reader.GetString("TrexPurchaseNo"),
                EntryDate = reader.GetDateTime("EntryDate"),
                Quantity = reader.GetDecimal("Quantity"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("SupplierId"))
            {
                entry.SupplierId = reader.GetGuid("SupplierId");
                entry.Supplier = new Supplier
                {
                    Id = entry.SupplierId.Value,
                    Name = reader.IsDBNull("SupplierName") ? "" : reader.GetString("SupplierName")
                };
            }
            else
            {
                entry.SupplierId = null;
                entry.Supplier = null;
            }

            if (!reader.IsDBNull("SerialNoId"))
            {
                entry.SerialNoId = reader.GetGuid("SerialNoId");
                if (!reader.IsDBNull("SerialNumber"))
                {
                    entry.SerialNo = new SerialNo
                    {
                        Id = entry.SerialNoId.Value,
                        SerialNumber = reader.GetString("SerialNumber")
                    };
                }
            }
            else
            {
                entry.SerialNoId = null;
            }

            return entry;
        }

        private void AddMaterialEntryParameters(SqlCommand command, MaterialEntry entry)
        {
            command.Parameters.AddWithValue("@Id", entry.Id);
            command.Parameters.AddWithValue("@TransactionType", entry.TransactionType);
            command.Parameters.AddWithValue("@MaterialType", entry.MaterialType);
            command.Parameters.AddWithValue("@MaterialSize", entry.MaterialSize);
            command.Parameters.AddWithValue("@Size", entry.Size);
            command.Parameters.AddWithValue("@Thickness", entry.Thickness);
            command.Parameters.AddWithValue("@SupplierId", entry.SupplierId.HasValue ? (object)entry.SupplierId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@SerialNoId", entry.SerialNoId.HasValue ? (object)entry.SerialNoId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@InvoiceNo", (object)entry.InvoiceNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@TrexPurchaseNo", (object)entry.TrexPurchaseNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@EntryDate", entry.EntryDate);
            command.Parameters.AddWithValue("@Quantity", entry.Quantity);
            command.Parameters.AddWithValue("@CreatedDate", entry.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", entry.IsActive);
        }
    }
}

