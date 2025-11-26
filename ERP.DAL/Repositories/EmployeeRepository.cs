using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class EmployeeRepository
    {
        public List<Employee> GetAll()
        {
            var employees = new List<Employee>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, FirstName, LastName, Phone, Email, Department, CreatedDate, ModifiedDate, IsActive FROM Employees WHERE IsActive = 1 ORDER BY FirstName, LastName";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(MapToEmployee(reader));
                        }
                    }
                }
            }
            
            return employees;
        }

        public Employee GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT Id, FirstName, LastName, Phone, Email, Department, CreatedDate, ModifiedDate, IsActive FROM Employees WHERE Id = @Id AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToEmployee(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Employee employee)
        {
            employee.Id = Guid.NewGuid();
            employee.CreatedDate = DateTime.Now;
            employee.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Employees (Id, FirstName, LastName, Phone, Email, Department, CreatedDate, IsActive) 
                             VALUES (@Id, @FirstName, @LastName, @Phone, @Email, @Department, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddEmployeeParameters(command, employee);
                    command.ExecuteNonQuery();
                }
            }
            
            return employee.Id;
        }

        public void Update(Employee employee)
        {
            employee.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Employees SET 
                             FirstName = @FirstName,
                             LastName = @LastName,
                             Phone = @Phone,
                             Email = @Email,
                             Department = @Department,
                             ModifiedDate = @ModifiedDate
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddEmployeeParameters(command, employee);
                    command.Parameters.AddWithValue("@ModifiedDate", employee.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Employees SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Employee MapToEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                Id = reader.GetGuid("Id"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? (DateTime?)null : reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }

        private void AddEmployeeParameters(SqlCommand command, Employee employee)
        {
            command.Parameters.AddWithValue("@Id", employee.Id);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Phone", (object)employee.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object)employee.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@Department", (object)employee.Department ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", employee.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", employee.IsActive);
        }
    }
}

