using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class SerialNoRepository
    {
        public List<SerialNo> GetAll()
        {
            var serialNos = new List<SerialNo>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, SerialNumber, Description, CreatedDate, ModifiedDate, IsActive FROM SerialNos WHERE IsActive = 1 ORDER BY SerialNumber";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            serialNos.Add(MapToSerialNo(reader));
                        }
                    }
                }
            }
            
            return serialNos;
        }

        public SerialNo GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, SerialNumber, Description, CreatedDate, ModifiedDate, IsActive FROM SerialNos WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSerialNo(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(SerialNo serialNo)
        {
            serialNo.Id = Guid.NewGuid();
            serialNo.CreatedDate = DateTime.Now;
            serialNo.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO SerialNos (Id, SerialNumber, Description, CreatedDate, IsActive) 
                             VALUES (@Id, @SerialNumber, @Description, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddSerialNoParameters(command, serialNo);
                    command.ExecuteNonQuery();
                }
            }
            
            return serialNo.Id;
        }

        public void Update(SerialNo serialNo)
        {
            serialNo.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE SerialNos SET 
                             SerialNumber = @SerialNumber,
                             Description = @Description,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddSerialNoParameters(command, serialNo);
                    command.Parameters.AddWithValue("@ModifiedDate", serialNo.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE SerialNos SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private SerialNo MapToSerialNo(SqlDataReader reader)
        {
            return new SerialNo
            {
                Id = reader.GetGuid("Id"),
                SerialNumber = reader.GetString("SerialNumber"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }

        private void AddSerialNoParameters(SqlCommand command, SerialNo serialNo)
        {
            command.Parameters.AddWithValue("@Id", serialNo.Id);
            command.Parameters.AddWithValue("@SerialNumber", serialNo.SerialNumber);
            command.Parameters.AddWithValue("@Description", (object)serialNo.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", serialNo.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", serialNo.IsActive);
        }
    }
}

