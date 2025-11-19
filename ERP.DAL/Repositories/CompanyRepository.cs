using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class CompanyRepository
    {
        public List<Company> GetAll()
        {
            var companies = new List<Company>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Name, Address, Phone, Email, TaxNumber, CreatedDate, ModifiedDate, IsActive FROM Companies WHERE IsActive = 1 ORDER BY Name";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            companies.Add(MapToCompany(reader));
                        }
                    }
                }
            }
            
            return companies;
        }

        public Company GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Name, Address, Phone, Email, TaxNumber, CreatedDate, ModifiedDate, IsActive FROM Companies WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToCompany(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Company company)
        {
            company.Id = Guid.NewGuid();
            company.CreatedDate = DateTime.Now;
            company.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Companies (Id, Name, Address, Phone, Email, TaxNumber, CreatedDate, IsActive) 
                             VALUES (@Id, @Name, @Address, @Phone, @Email, @TaxNumber, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", company.Id);
                    command.Parameters.AddWithValue("@Name", company.Name);
                    command.Parameters.AddWithValue("@Address", (object)company.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object)company.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)company.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TaxNumber", (object)company.TaxNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedDate", company.CreatedDate);
                    command.Parameters.AddWithValue("@IsActive", company.IsActive);
                    
                    command.ExecuteNonQuery();
                }
            }
            
            return company.Id;
        }

        public void Update(Company company)
        {
            company.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Companies SET Name = @Name, Address = @Address, Phone = @Phone, 
                             Email = @Email, TaxNumber = @TaxNumber, ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", company.Id);
                    command.Parameters.AddWithValue("@Name", company.Name);
                    command.Parameters.AddWithValue("@Address", (object)company.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object)company.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object)company.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TaxNumber", (object)company.TaxNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ModifiedDate", company.ModifiedDate);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Companies SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        private Company MapToCompany(SqlDataReader reader)
        {
            return new Company
            {
                Id = reader.GetGuid("Id"),
                Name = reader.GetString("Name"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                TaxNumber = reader.IsDBNull("TaxNumber") ? null : reader.GetString("TaxNumber"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? null : (DateTime?)reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}

