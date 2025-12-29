using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class SideProfileStockRepository
    {
        public List<SideProfileStock> GetAll()
        {
            var stocks = new List<SideProfileStock>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, Length, InitialQuantity, UsedLength, WastedLength, RemainingLength, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM SideProfileStocks
                             WHERE IsActive = 1
                             ORDER BY EntryDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stocks.Add(MapToSideProfileStock(reader));
                        }
                    }
                }
            }
            
            return stocks;
        }

        public SideProfileStock GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, Length, InitialQuantity, UsedLength, WastedLength, RemainingLength, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM SideProfileStocks
                             WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSideProfileStock(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public SideProfileStock GetByLength(decimal length)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, Length, InitialQuantity, UsedLength, WastedLength, RemainingLength, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM SideProfileStocks
                             WHERE Length = @Length AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Length", length);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSideProfileStock(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid InsertOrUpdate(SideProfileStock stock)
        {
            // Önce mevcut kaydı kontrol et
            var existing = GetByLength(stock.Length);
            
            if (existing != null)
            {
                // Mevcut kayıt varsa adedi artır
                existing.InitialQuantity += stock.InitialQuantity;
                existing.RemainingLength = (existing.Length * existing.InitialQuantity) - existing.UsedLength - existing.WastedLength;
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

        public Guid Insert(SideProfileStock stock)
        {
            stock.Id = Guid.NewGuid();
            stock.CreatedDate = DateTime.Now;
            stock.EntryDate = DateTime.Now;
            stock.RemainingLength = stock.Length * stock.InitialQuantity;
            stock.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO SideProfileStocks (Id, Length, InitialQuantity, UsedLength, WastedLength, RemainingLength, EntryDate, CreatedDate, IsActive) 
                             VALUES (@Id, @Length, @InitialQuantity, @UsedLength, @WastedLength, @RemainingLength, @EntryDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddSideProfileStockParameters(command, stock);
                    command.ExecuteNonQuery();
                }
            }
            
            return stock.Id;
        }

        public void Update(SideProfileStock stock)
        {
            stock.ModifiedDate = DateTime.Now;
            stock.RemainingLength = (stock.Length * stock.InitialQuantity) - stock.UsedLength - stock.WastedLength;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE SideProfileStocks SET 
                             InitialQuantity = @InitialQuantity,
                             UsedLength = @UsedLength,
                             WastedLength = @WastedLength,
                             RemainingLength = @RemainingLength,
                             EntryDate = @EntryDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", stock.Id);
                    command.Parameters.AddWithValue("@InitialQuantity", stock.InitialQuantity);
                    command.Parameters.AddWithValue("@UsedLength", stock.UsedLength);
                    command.Parameters.AddWithValue("@WastedLength", stock.WastedLength);
                    command.Parameters.AddWithValue("@RemainingLength", stock.RemainingLength);
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
                var query = @"UPDATE SideProfileStocks SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddSideProfileStockParameters(SqlCommand command, SideProfileStock stock)
        {
            command.Parameters.AddWithValue("@Id", stock.Id);
            command.Parameters.AddWithValue("@Length", stock.Length);
            command.Parameters.AddWithValue("@InitialQuantity", stock.InitialQuantity);
            command.Parameters.AddWithValue("@UsedLength", stock.UsedLength);
            command.Parameters.AddWithValue("@WastedLength", stock.WastedLength);
            command.Parameters.AddWithValue("@RemainingLength", stock.RemainingLength);
            command.Parameters.AddWithValue("@EntryDate", stock.EntryDate);
            command.Parameters.AddWithValue("@CreatedDate", stock.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", stock.IsActive);
        }

        private SideProfileStock MapToSideProfileStock(SqlDataReader reader)
        {
            return new SideProfileStock
            {
                Id = reader.GetGuid("Id"),
                Length = reader.GetDecimal("Length"),
                InitialQuantity = reader.GetInt32("InitialQuantity"),
                UsedLength = reader.GetDecimal("UsedLength"),
                WastedLength = reader.GetDecimal("WastedLength"),
                RemainingLength = reader.GetDecimal("RemainingLength"),
                EntryDate = reader.GetDateTime("EntryDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}

