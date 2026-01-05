using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class IsolationStockRepository
    {
        public List<IsolationStock> GetAll()
        {
            var stocks = new List<IsolationStock>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, LiquidType, Kilogram, Quantity, Liter, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM IsolationStocks
                             WHERE IsActive = 1
                             ORDER BY EntryDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stocks.Add(MapToIsolationStock(reader));
                        }
                    }
                }
            }
            
            return stocks;
        }

        public IsolationStock GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, LiquidType, Kilogram, Quantity, Liter, EntryDate, 
                             CreatedDate, ModifiedDate, IsActive
                             FROM IsolationStocks
                             WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToIsolationStock(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(IsolationStock stock)
        {
            stock.Id = Guid.NewGuid();
            stock.CreatedDate = DateTime.Now;
            stock.EntryDate = DateTime.Now;
            stock.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO IsolationStocks (Id, LiquidType, Kilogram, Quantity, Liter, EntryDate, CreatedDate, IsActive) 
                             VALUES (@Id, @LiquidType, @Kilogram, @Quantity, @Liter, @EntryDate, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddIsolationStockParameters(command, stock);
                    command.ExecuteNonQuery();
                }
            }
            
            return stock.Id;
        }

        public void Update(IsolationStock stock)
        {
            stock.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE IsolationStocks SET 
                             LiquidType = @LiquidType,
                             Kilogram = @Kilogram,
                             Quantity = @Quantity,
                             Liter = @Liter,
                             EntryDate = @EntryDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", stock.Id);
                    command.Parameters.AddWithValue("@LiquidType", stock.LiquidType);
                    command.Parameters.AddWithValue("@Kilogram", stock.Kilogram);
                    command.Parameters.AddWithValue("@Quantity", stock.Quantity);
                    command.Parameters.AddWithValue("@Liter", stock.Liter);
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
                var query = @"UPDATE IsolationStocks SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Toplam stok hesaplamaları için yardımcı metodlar
        public (decimal TotalIsosiyanat, decimal TotalPoliol, decimal TotalLiter) GetTotalStocks()
        {
            decimal totalIsosiyanat = 0;
            decimal totalPoliol = 0;
            decimal totalLiter = 0;

            var stocks = GetAll();
            foreach (var stock in stocks)
            {
                if (stock.LiquidType == "İzosiyanat")
                {
                    totalIsosiyanat += stock.Liter;
                    totalLiter += stock.Liter;
                }
                else if (stock.LiquidType == "Poliol")
                {
                    totalPoliol += stock.Liter;
                    totalLiter += stock.Liter;
                }
                else if (stock.LiquidType == "İzolasyon")
                {
                    // İzolasyon karışımı genellikle 1:1 oranında (50% İzosiyanat, 50% Poliol)
                    decimal halfLiter = stock.Liter / 2.0m;
                    totalIsosiyanat += halfLiter;
                    totalPoliol += halfLiter;
                    totalLiter += stock.Liter;
                }
            }

            return (totalIsosiyanat, totalPoliol, totalLiter);
        }

        private void AddIsolationStockParameters(SqlCommand command, IsolationStock stock)
        {
            command.Parameters.AddWithValue("@Id", stock.Id);
            command.Parameters.AddWithValue("@LiquidType", stock.LiquidType);
            command.Parameters.AddWithValue("@Kilogram", stock.Kilogram);
            command.Parameters.AddWithValue("@Quantity", stock.Quantity);
            command.Parameters.AddWithValue("@Liter", stock.Liter);
            command.Parameters.AddWithValue("@EntryDate", stock.EntryDate);
            command.Parameters.AddWithValue("@CreatedDate", stock.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", stock.IsActive);
        }

        private IsolationStock MapToIsolationStock(SqlDataReader reader)
        {
            var stock = new IsolationStock
            {
                Id = reader.GetGuid("Id"),
                LiquidType = reader.GetString("LiquidType"),
                EntryDate = reader.GetDateTime("EntryDate"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
            
            // Kilogram kolonu varsa onu kullan, yoksa Quantity'den hesapla (geriye uyumluluk)
            if (HasColumn(reader, "Kilogram"))
            {
                stock.Kilogram = reader.IsDBNull("Kilogram") ? 0 : reader.GetDecimal("Kilogram");
            }
            else
            {
                // Eski veriler için Quantity'den kg'a çevir (geriye uyumluluk)
                int quantity = reader.IsDBNull("Quantity") ? 0 : reader.GetInt32("Quantity");
                if (stock.LiquidType == "İzosiyanat")
                    stock.Kilogram = quantity * 250m; // 1 adet = 250 kg
                else if (stock.LiquidType == "Poliol")
                    stock.Kilogram = quantity * 25m; // 1 adet = 25 kg
                else if (stock.LiquidType == "MS Silikon")
                    stock.Kilogram = quantity * 0.95m; // 1 adet = 0.95 kg (950 gr)
                else
                    stock.Kilogram = 0;
            }
            
            stock.Quantity = reader.IsDBNull("Quantity") ? 0 : reader.GetInt32("Quantity");
            stock.Liter = reader.IsDBNull("Liter") ? 0 : reader.GetDecimal("Liter");
            
            return stock;
        }
        
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}

