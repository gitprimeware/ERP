using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class UserRepository
    {
        public List<User> GetAll()
        {
            var users = new List<User>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Username, PasswordHash, FullName, IsAdmin, CreatedDate, ModifiedDate, IsActive FROM Users WHERE IsActive = 1 ORDER BY Username";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(MapToUser(reader));
                        }
                    }
                }
            }
            
            return users;
        }

        public User GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Username, PasswordHash, FullName, IsAdmin, CreatedDate, ModifiedDate, IsActive FROM Users WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToUser(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public User GetByUsername(string username)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Username, PasswordHash, FullName, IsAdmin, CreatedDate, ModifiedDate, IsActive FROM Users WHERE Username = @Username AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToUser(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public User Authenticate(string username, string password)
        {
            var user = GetByUsername(username);
            if (user == null)
                return null;

            var passwordHash = HashPassword(password);
            if (user.PasswordHash == passwordHash)
                return user;

            return null;
        }

        public Guid Insert(User user)
        {
            user.Id = Guid.NewGuid();
            user.CreatedDate = DateTime.Now;
            user.IsActive = true;
            user.PasswordHash = HashPassword(user.PasswordHash); // PasswordHash alanı aslında plain password olarak geliyor

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Users (Id, Username, PasswordHash, FullName, IsAdmin, CreatedDate, IsActive) 
                             VALUES (@Id, @Username, @PasswordHash, @FullName, @IsAdmin, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@FullName", user.FullName);
                    command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);
                    command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);
                    command.Parameters.AddWithValue("@IsActive", user.IsActive);
                    
                    command.ExecuteNonQuery();
                }
            }
            
            return user.Id;
        }

        public void Update(User user)
        {
            user.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Users SET Username = @Username, FullName = @FullName, IsAdmin = @IsAdmin, ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@FullName", user.FullName);
                    command.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);
                    command.Parameters.AddWithValue("@ModifiedDate", user.ModifiedDate);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdatePassword(Guid userId, string newPassword)
        {
            var passwordHash = HashPassword(newPassword);

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Users SET PasswordHash = @PasswordHash, ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
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
                var query = "UPDATE Users SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        private User MapToUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                IsAdmin = reader.GetBoolean(reader.GetOrdinal("IsAdmin")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}

