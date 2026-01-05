using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class UserPermissionRepository
    {
        public List<UserPermission> GetByUserId(Guid userId)
        {
            var permissions = new List<UserPermission>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, UserId, PermissionKey, CreatedDate, ModifiedDate, IsActive FROM UserPermissions WHERE UserId = @UserId AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(MapToUserPermission(reader));
                        }
                    }
                }
            }
            
            return permissions;
        }

        public List<string> GetPermissionKeysByUserId(Guid userId)
        {
            var permissionKeys = new List<string>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT PermissionKey FROM UserPermissions WHERE UserId = @UserId AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissionKeys.Add(reader.GetString(reader.GetOrdinal("PermissionKey")));
                        }
                    }
                }
            }
            
            return permissionKeys;
        }

        public bool HasPermission(Guid userId, string permissionKey)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM UserPermissions WHERE UserId = @UserId AND PermissionKey = @PermissionKey AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@PermissionKey", permissionKey);
                    
                    var count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public void Insert(UserPermission permission)
        {
            permission.Id = Guid.NewGuid();
            permission.CreatedDate = DateTime.Now;
            permission.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO UserPermissions (Id, UserId, PermissionKey, CreatedDate, IsActive) 
                             VALUES (@Id, @UserId, @PermissionKey, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", permission.Id);
                    command.Parameters.AddWithValue("@UserId", permission.UserId);
                    command.Parameters.AddWithValue("@PermissionKey", permission.PermissionKey);
                    command.Parameters.AddWithValue("@CreatedDate", permission.CreatedDate);
                    command.Parameters.AddWithValue("@IsActive", permission.IsActive);
                    
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        // Unique constraint hatası - zaten var, sessizce geç
                        if (ex.Number != 2627) // Unique constraint violation
                            throw;
                    }
                }
            }
        }

        public void DeleteByUserId(Guid userId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE UserPermissions SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE UserId = @UserId";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteByUserIdAndPermissionKey(Guid userId, string permissionKey)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE UserPermissions SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE UserId = @UserId AND PermissionKey = @PermissionKey";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@PermissionKey", permissionKey);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SetUserPermissions(Guid userId, List<string> permissionKeys)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Önce tüm izinleri IsActive = 0 yap
                        var deleteQuery = "UPDATE UserPermissions SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE UserId = @UserId";
                        using (var deleteCommand = new SqlCommand(deleteQuery, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@UserId", userId);
                            deleteCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                            deleteCommand.ExecuteNonQuery();
                        }
                        
                        // Yeni izinleri ekle veya güncelle
                        foreach (var permissionKey in permissionKeys)
                        {
                            // Önce mevcut kaydı kontrol et
                            var checkQuery = "SELECT Id FROM UserPermissions WHERE UserId = @UserId AND PermissionKey = @PermissionKey";
                            Guid? existingId = null;
                            
                            using (var checkCommand = new SqlCommand(checkQuery, connection, transaction))
                            {
                                checkCommand.Parameters.AddWithValue("@UserId", userId);
                                checkCommand.Parameters.AddWithValue("@PermissionKey", permissionKey);
                                
                                var result = checkCommand.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    existingId = (Guid)result;
                                }
                            }
                            
                            if (existingId.HasValue)
                            {
                                // Mevcut kaydı güncelle (IsActive = 1 yap)
                                var updateQuery = @"UPDATE UserPermissions 
                                                   SET IsActive = 1, ModifiedDate = @ModifiedDate 
                                                   WHERE Id = @Id";
                                
                                using (var updateCommand = new SqlCommand(updateQuery, connection, transaction))
                                {
                                    updateCommand.Parameters.AddWithValue("@Id", existingId.Value);
                                    updateCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                                    updateCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Yeni kayıt ekle
                                var permissionId = Guid.NewGuid();
                                var insertQuery = @"INSERT INTO UserPermissions (Id, UserId, PermissionKey, CreatedDate, IsActive) 
                                                 VALUES (@Id, @UserId, @PermissionKey, @CreatedDate, @IsActive)";
                                
                                using (var insertCommand = new SqlCommand(insertQuery, connection, transaction))
                                {
                                    insertCommand.Parameters.AddWithValue("@Id", permissionId);
                                    insertCommand.Parameters.AddWithValue("@UserId", userId);
                                    insertCommand.Parameters.AddWithValue("@PermissionKey", permissionKey);
                                    insertCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                    insertCommand.Parameters.AddWithValue("@IsActive", true);
                                    insertCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private UserPermission MapToUserPermission(SqlDataReader reader)
        {
            return new UserPermission
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                PermissionKey = reader.GetString(reader.GetOrdinal("PermissionKey")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}

