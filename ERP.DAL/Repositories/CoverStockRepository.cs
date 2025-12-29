using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class CoverStockRepository
    {
        public List<CoverStock> GetAll()
        {
            var stocks = new List<CoverStock>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, ProfileType, Size, CoverLength, Quantity, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM CoverStocks
                             WHERE IsActive = 1
                             ORDER BY ProfileType, Size, CoverLength";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stocks.Add(MapToCoverStock(reader));
                        }
                    }
                }
            }
            
            return stocks;
        }

        public CoverStock GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, ProfileType, Size, CoverLength, Quantity, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM CoverStocks
                             WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToCoverStock(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public CoverStock GetByProperties(string profileType, int size, int coverLength)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, ProfileType, Size, CoverLength, Quantity, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM CoverStocks
                             WHERE ProfileType = @ProfileType AND Size = @Size AND CoverLength = @CoverLength AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProfileType", profileType);
                    command.Parameters.AddWithValue("@Size", size);
                    command.Parameters.AddWithValue("@CoverLength", coverLength);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToCoverStock(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid InsertOrUpdate(CoverStock stock)
        {
            // Önce mevcut kaydı kontrol et
            var existing = GetByProperties(stock.ProfileType, stock.Size, stock.CoverLength);
            
            if (existing != null)
            {
                // Mevcut kayıt varsa quantity'yi artır
                existing.Quantity += stock.Quantity;
                existing.EntryDate = DateTime.Now;
                existing.ModifiedDate = DateTime.Now;
                Update(existing);
                return existing.Id;
            }
            else
            {
                // Yeni kayıt oluştur
                return Insert(stock);
            }
        }

        public Guid Insert(CoverStock stock)
        {
            stock.Id = Guid.NewGuid();
            stock.CreatedDate = DateTime.Now;
            stock.EntryDate = DateTime.Now;
            stock.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO CoverStocks (Id, ProfileType, Size, CoverLength, Quantity, EntryDate, CreatedDate, IsActive) 
                             VALUES (@Id, @ProfileType, @Size, @CoverLength, @Quantity, @EntryDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddCoverStockParameters(command, stock);
                    command.ExecuteNonQuery();
                }
            }
            
            return stock.Id;
        }

        public void Update(CoverStock stock)
        {
            stock.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE CoverStocks SET 
                             Quantity = @Quantity, 
                             EntryDate = @EntryDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", stock.Id);
                    command.Parameters.AddWithValue("@Quantity", stock.Quantity);
                    command.Parameters.AddWithValue("@EntryDate", stock.EntryDate);
                    command.Parameters.AddWithValue("@ModifiedDate", stock.ModifiedDate ?? (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE CoverStocks SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddCoverStockParameters(SqlCommand command, CoverStock stock)
        {
            command.Parameters.AddWithValue("@Id", stock.Id);
            command.Parameters.AddWithValue("@ProfileType", stock.ProfileType);
            command.Parameters.AddWithValue("@Size", stock.Size);
            command.Parameters.AddWithValue("@CoverLength", stock.CoverLength);
            command.Parameters.AddWithValue("@Quantity", stock.Quantity);
            command.Parameters.AddWithValue("@EntryDate", stock.EntryDate);
            command.Parameters.AddWithValue("@CreatedDate", stock.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", stock.IsActive);
        }

        private CoverStock MapToCoverStock(SqlDataReader reader)
        {
            return new CoverStock
            {
                Id = reader.GetGuid("Id"),
                ProfileType = reader.GetString("ProfileType"),
                Size = reader.GetInt32("Size"),
                CoverLength = reader.GetInt32("CoverLength"),
                Quantity = reader.GetInt32("Quantity"),
                EntryDate = reader.GetDateTime("EntryDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}

