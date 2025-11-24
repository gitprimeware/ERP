using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class MaterialExitRepository
    {
        public List<MaterialExit> GetAll()
        {
            var exits = new List<MaterialExit>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT me.Id, me.TransactionType, me.MaterialType, me.MaterialSize, me.Size, me.Thickness,
                             me.CompanyId, me.TrexInvoiceNo, me.ExitDate, me.Quantity,
                             me.CreatedDate, me.ModifiedDate, me.IsActive,
                             c.Name as CompanyName
                             FROM MaterialExits me
                             LEFT JOIN Companies c ON me.CompanyId = c.Id
                             WHERE me.IsActive = 1
                             ORDER BY me.ExitDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            exits.Add(MapToMaterialExit(reader));
                        }
                    }
                }
            }
            
            return exits;
        }

        public MaterialExit GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT me.Id, me.TransactionType, me.MaterialType, me.MaterialSize, me.Size, me.Thickness,
                             me.CompanyId, me.TrexInvoiceNo, me.ExitDate, me.Quantity,
                             me.CreatedDate, me.ModifiedDate, me.IsActive,
                             c.Name as CompanyName
                             FROM MaterialExits me
                             LEFT JOIN Companies c ON me.CompanyId = c.Id
                             WHERE me.Id = @Id AND me.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToMaterialExit(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(MaterialExit exit)
        {
            exit.Id = Guid.NewGuid();
            exit.CreatedDate = DateTime.Now;
            exit.IsActive = true;
            if (exit.ExitDate == default(DateTime))
            {
                exit.ExitDate = DateTime.Now;
            }

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO MaterialExits (Id, TransactionType, MaterialType, MaterialSize, Size, Thickness,
                             CompanyId, TrexInvoiceNo, ExitDate, Quantity, CreatedDate, IsActive) 
                             VALUES (@Id, @TransactionType, @MaterialType, @MaterialSize, @Size, @Thickness,
                             @CompanyId, @TrexInvoiceNo, @ExitDate, @Quantity, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddMaterialExitParameters(command, exit);
                    command.ExecuteNonQuery();
                }
            }
            
            return exit.Id;
        }

        public void Update(MaterialExit exit)
        {
            exit.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE MaterialExits SET 
                             TransactionType = @TransactionType,
                             MaterialType = @MaterialType,
                             MaterialSize = @MaterialSize,
                             Size = @Size,
                             Thickness = @Thickness,
                             CompanyId = @CompanyId,
                             TrexInvoiceNo = @TrexInvoiceNo,
                             ExitDate = @ExitDate,
                             Quantity = @Quantity,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddMaterialExitParameters(command, exit);
                    command.Parameters.AddWithValue("@ModifiedDate", exit.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE MaterialExits SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private MaterialExit MapToMaterialExit(SqlDataReader reader)
        {
            var exit = new MaterialExit
            {
                Id = reader.GetGuid("Id"),
                TransactionType = reader.GetString("TransactionType"),
                MaterialType = reader.GetString("MaterialType"),
                MaterialSize = reader.GetString("MaterialSize"),
                Size = reader.GetInt32("Size"),
                Thickness = reader.GetDecimal("Thickness"),
                TrexInvoiceNo = reader.IsDBNull("TrexInvoiceNo") ? null : reader.GetString("TrexInvoiceNo"),
                ExitDate = reader.GetDateTime("ExitDate"),
                Quantity = reader.GetDecimal("Quantity"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };

            if (!reader.IsDBNull("CompanyId"))
            {
                exit.CompanyId = reader.GetGuid("CompanyId");
                if (!reader.IsDBNull("CompanyName"))
                {
                    exit.Company = new Company
                    {
                        Id = exit.CompanyId.Value,
                        Name = reader.GetString("CompanyName")
                    };
                }
            }
            else
            {
                exit.CompanyId = null;
            }

            return exit;
        }

        private void AddMaterialExitParameters(SqlCommand command, MaterialExit exit)
        {
            command.Parameters.AddWithValue("@Id", exit.Id);
            command.Parameters.AddWithValue("@TransactionType", exit.TransactionType);
            command.Parameters.AddWithValue("@MaterialType", exit.MaterialType);
            command.Parameters.AddWithValue("@MaterialSize", exit.MaterialSize);
            command.Parameters.AddWithValue("@Size", exit.Size);
            command.Parameters.AddWithValue("@Thickness", exit.Thickness);
            command.Parameters.AddWithValue("@CompanyId", exit.CompanyId.HasValue ? (object)exit.CompanyId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@TrexInvoiceNo", (object)exit.TrexInvoiceNo ?? DBNull.Value);
            command.Parameters.AddWithValue("@ExitDate", exit.ExitDate);
            command.Parameters.AddWithValue("@Quantity", exit.Quantity);
            command.Parameters.AddWithValue("@CreatedDate", exit.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", exit.IsActive);
        }
    }
}

