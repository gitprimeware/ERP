using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class SideProfileRemnantRepository
    {
        public List<SideProfileRemnant> GetAll(bool includeWaste = true)
        {
            var remnants = new List<SideProfileRemnant>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, ProfileType, Length, Quantity, IsWaste, CreatedDate, WasteDate, 
                             ModifiedDate, IsActive
                             FROM SideProfileRemnants
                             WHERE IsActive = 1";
                
                if (!includeWaste)
                {
                    query += " AND IsWaste = 0";
                }
                
                query += " ORDER BY Length DESC, Quantity DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            remnants.Add(MapToSideProfileRemnant(reader));
                        }
                    }
                }
            }
            
            return remnants;
        }

        public SideProfileRemnant GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, Length, Quantity, IsWaste, CreatedDate, WasteDate, 
                             ModifiedDate, IsActive
                             FROM SideProfileRemnants
                             WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSideProfileRemnant(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public SideProfileRemnant GetByLengthAndProfileType(decimal length, string profileType, bool includeWaste = false)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT Id, ProfileType, Length, Quantity, IsWaste, CreatedDate, WasteDate, 
                             ModifiedDate, IsActive
                             FROM SideProfileRemnants
                             WHERE Length = @Length AND ProfileType = @ProfileType AND IsActive = 1";
                
                if (!includeWaste)
                {
                    query += " AND IsWaste = 0";
                }
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Length", length);
                    command.Parameters.AddWithValue("@ProfileType", profileType);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSideProfileRemnant(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public SideProfileRemnant GetByLength(decimal length, bool includeWaste = false)
        {
            // Backward compatibility - returns first match (deprecated, use GetByLengthAndProfileType instead)
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT TOP 1 Id, ProfileType, Length, Quantity, IsWaste, CreatedDate, WasteDate, 
                             ModifiedDate, IsActive
                             FROM SideProfileRemnants
                             WHERE Length = @Length AND IsActive = 1";
                
                if (!includeWaste)
                {
                    query += " AND IsWaste = 0";
                }
                
                query += " ORDER BY CreatedDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Length", length);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToSideProfileRemnant(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid InsertOrMerge(SideProfileRemnant remnant)
        {
            // Aynı uzunlukta ve profil tipinde (ve hurda değilse) remnant var mı kontrol et
            var existing = GetByLengthAndProfileType(remnant.Length, remnant.ProfileType, includeWaste: false);
            
            if (existing != null)
            {
                // Mevcut kayıt varsa quantity'yi artır
                existing.Quantity += remnant.Quantity;
                existing.ModifiedDate = DateTime.Now;
                Update(existing);
                return existing.Id;
            }
            else
            {
                // Yeni kayıt oluştur
                return Insert(remnant);
            }
        }

        public Guid Insert(SideProfileRemnant remnant)
        {
            remnant.Id = Guid.NewGuid();
            remnant.CreatedDate = DateTime.Now;
            remnant.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO SideProfileRemnants (Id, ProfileType, Length, Quantity, IsWaste, CreatedDate, WasteDate, IsActive) 
                             VALUES (@Id, @ProfileType, @Length, @Quantity, @IsWaste, @CreatedDate, @WasteDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddSideProfileRemnantParameters(command, remnant);
                    command.ExecuteNonQuery();
                }
            }
            
            return remnant.Id;
        }

        public void Update(SideProfileRemnant remnant)
        {
            remnant.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE SideProfileRemnants SET 
                             Quantity = @Quantity,
                             IsWaste = @IsWaste,
                             WasteDate = @WasteDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", remnant.Id);
                    command.Parameters.AddWithValue("@Quantity", remnant.Quantity);
                    command.Parameters.AddWithValue("@IsWaste", remnant.IsWaste);
                    command.Parameters.AddWithValue("@WasteDate", remnant.WasteDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ModifiedDate", remnant.ModifiedDate ?? (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public void MarkAsWaste(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE SideProfileRemnants SET 
                             IsWaste = 1,
                             WasteDate = @WasteDate,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@WasteDate", DateTime.Now);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE SideProfileRemnants SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddSideProfileRemnantParameters(SqlCommand command, SideProfileRemnant remnant)
        {
            command.Parameters.AddWithValue("@Id", remnant.Id);
            command.Parameters.AddWithValue("@ProfileType", remnant.ProfileType);
            command.Parameters.AddWithValue("@Length", remnant.Length);
            command.Parameters.AddWithValue("@Quantity", remnant.Quantity);
            command.Parameters.AddWithValue("@IsWaste", remnant.IsWaste);
            command.Parameters.AddWithValue("@CreatedDate", remnant.CreatedDate);
            command.Parameters.AddWithValue("@WasteDate", remnant.WasteDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", remnant.IsActive);
        }

        private SideProfileRemnant MapToSideProfileRemnant(SqlDataReader reader)
        {
            return new SideProfileRemnant
            {
                Id = reader.GetGuid("Id"),
                ProfileType = reader.IsDBNull("ProfileType") ? "Standart" : reader.GetString("ProfileType"),
                Length = reader.GetDecimal("Length"),
                Quantity = reader.GetInt32("Quantity"),
                IsWaste = reader.GetBoolean("IsWaste"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                WasteDate = reader.IsDBNull("WasteDate") ? (DateTime?)null : reader.GetDateTime("WasteDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
    }
}

