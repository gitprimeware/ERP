using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class SupplierRepository
    {
        public List<Supplier> GetAll()
        {
            var suppliers = new List<Supplier>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Name, Address, Phone, Email, TaxNumber, CreatedDate, ModifiedDate, IsActive FROM Suppliers WHERE IsActive = 1 ORDER BY Name";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            suppliers.Add(MapToSupplier(reader));
                        }
                    }
                }
            }
            
            return suppliers;
        }

        public Supplier GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Name, Address, Phone, Email, TaxNumber, CreatedDate, ModifiedDate, IsActive FROM Suppliers WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSupplier(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Supplier supplier)
        {
            supplier.Id = Guid.NewGuid();
            supplier.CreatedDate = DateTime.Now;
            supplier.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Suppliers (Id, Name, Address, Phone, Email, TaxNumber, CreatedDate, IsActive) 
                             VALUES (@Id, @Name, @Address, @Phone, @Email, @TaxNumber, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddSupplierParameters(command, supplier);
                    command.ExecuteNonQuery();
                }
            }
            
            return supplier.Id;
        }

        public void Update(Supplier supplier)
        {
            supplier.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Suppliers SET 
                             Name = @Name,
                             Address = @Address,
                             Phone = @Phone,
                             Email = @Email,
                             TaxNumber = @TaxNumber,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddSupplierParameters(command, supplier);
                    command.Parameters.AddWithValue("@ModifiedDate", supplier.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Suppliers SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Supplier MapToSupplier(SqlDataReader reader)
        {
            return new Supplier
            {
                Id = reader.GetGuid("Id"),
                Name = reader.GetString("Name"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                TaxNumber = reader.IsDBNull("TaxNumber") ? null : reader.GetString("TaxNumber"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }

        private void AddSupplierParameters(SqlCommand command, Supplier supplier)
        {
            command.Parameters.AddWithValue("@Id", supplier.Id);
            command.Parameters.AddWithValue("@Name", supplier.Name);
            command.Parameters.AddWithValue("@Address", (object)supplier.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("@Phone", (object)supplier.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)supplier.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@TaxNumber", (object)supplier.TaxNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", supplier.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", supplier.IsActive);
        }
    }
}

