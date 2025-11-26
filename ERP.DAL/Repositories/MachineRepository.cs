using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class MachineRepository
    {
        public List<Machine> GetAll()
        {
            var machines = new List<Machine>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Name, Code, Description, CreatedDate, ModifiedDate, IsActive FROM Machines WHERE IsActive = 1 ORDER BY Name";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            machines.Add(MapToMachine(reader));
                        }
                    }
                }
            }
            
            return machines;
        }

        public Machine GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, Name, Code, Description, CreatedDate, ModifiedDate, IsActive FROM Machines WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToMachine(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Machine machine)
        {
            machine.Id = Guid.NewGuid();
            machine.CreatedDate = DateTime.Now;
            machine.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Machines (Id, Name, Code, Description, CreatedDate, IsActive) 
                             VALUES (@Id, @Name, @Code, @Description, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddMachineParameters(command, machine);
                    command.ExecuteNonQuery();
                }
            }
            
            return machine.Id;
        }

        public void Update(Machine machine)
        {
            machine.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Machines SET 
                             Name = @Name,
                             Code = @Code,
                             Description = @Description,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddMachineParameters(command, machine);
                    command.Parameters.AddWithValue("@ModifiedDate", machine.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Machines SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Machine MapToMachine(SqlDataReader reader)
        {
            return new Machine
            {
                Id = reader.GetGuid("Id"),
                Name = reader.GetString("Name"),
                Code = reader.IsDBNull("Code") ? null : reader.GetString("Code"),
                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }

        private void AddMachineParameters(SqlCommand command, Machine machine)
        {
            command.Parameters.AddWithValue("@Id", machine.Id);
            command.Parameters.AddWithValue("@Name", machine.Name);
            command.Parameters.AddWithValue("@Code", (object)machine.Code ?? DBNull.Value);
            command.Parameters.AddWithValue("@Description", (object)machine.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", machine.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", machine.IsActive);
        }
    }
}

