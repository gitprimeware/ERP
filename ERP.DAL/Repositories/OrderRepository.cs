using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class OrderRepository
    {
        public List<Order> GetAll(string searchTerm = null, Guid? companyId = null)
        {
            var orders = new List<Order>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT o.Id, o.CompanyId, o.CustomerOrderNo, o.TrexOrderNo, o.DeviceName, 
                             o.OrderDate, o.TermDate, o.ProductCode, o.BypassSize, o.BypassType, 
                             o.LamelThickness, o.ProductType, o.Quantity, o.SalesPrice, o.TotalPrice, 
                             o.ShipmentDate, o.CurrencyRate, o.Status, o.CreatedDate, o.ModifiedDate, o.IsActive,
                             c.Name as CompanyName
                             FROM Orders o
                             LEFT JOIN Companies c ON o.CompanyId = c.Id
                             WHERE o.IsActive = 1";
                
                var parameters = new List<SqlParameter>();
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query += " AND (o.CustomerOrderNo LIKE @SearchTerm OR o.TrexOrderNo LIKE @SearchTerm OR o.DeviceName LIKE @SearchTerm OR c.Name LIKE @SearchTerm)";
                    parameters.Add(new SqlParameter("@SearchTerm", $"%{searchTerm}%"));
                }
                
                if (companyId.HasValue)
                {
                    query += " AND o.CompanyId = @CompanyId";
                    parameters.Add(new SqlParameter("@CompanyId", companyId.Value));
                }
                
                query += " ORDER BY o.OrderDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(MapToOrder(reader));
                        }
                    }
                }
            }
            
            return orders;
        }

        public Order GetById(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT o.Id, o.CompanyId, o.CustomerOrderNo, o.TrexOrderNo, o.DeviceName, 
                             o.OrderDate, o.TermDate, o.ProductCode, o.BypassSize, o.BypassType, 
                             o.LamelThickness, o.ProductType, o.Quantity, o.SalesPrice, o.TotalPrice, 
                             o.ShipmentDate, o.CurrencyRate, o.Status, o.CreatedDate, o.ModifiedDate, o.IsActive,
                             c.Name as CompanyName
                             FROM Orders o
                             LEFT JOIN Companies c ON o.CompanyId = c.Id
                             WHERE o.Id = @Id AND o.IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToOrder(reader);
                        }
                    }
                }
            }
            
            return null;
        }

        public Guid Insert(Order order)
        {
            order.Id = Guid.NewGuid();
            order.CreatedDate = DateTime.Now;
            order.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Orders (Id, CompanyId, CustomerOrderNo, TrexOrderNo, DeviceName, 
                             OrderDate, TermDate, ProductCode, BypassSize, BypassType, LamelThickness, 
                             ProductType, Quantity, SalesPrice, TotalPrice, ShipmentDate, CurrencyRate, 
                             Status, CreatedDate, IsActive) 
                             VALUES (@Id, @CompanyId, @CustomerOrderNo, @TrexOrderNo, @DeviceName, 
                             @OrderDate, @TermDate, @ProductCode, @BypassSize, @BypassType, @LamelThickness, 
                             @ProductType, @Quantity, @SalesPrice, @TotalPrice, @ShipmentDate, @CurrencyRate, 
                             @Status, @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddOrderParameters(command, order);
                    command.ExecuteNonQuery();
                }
            }
            
            return order.Id;
        }

        public void Update(Order order)
        {
            order.ModifiedDate = DateTime.Now;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Orders SET CompanyId = @CompanyId, CustomerOrderNo = @CustomerOrderNo, 
                             TrexOrderNo = @TrexOrderNo, DeviceName = @DeviceName, OrderDate = @OrderDate, 
                             TermDate = @TermDate, ProductCode = @ProductCode, BypassSize = @BypassSize, 
                             BypassType = @BypassType, LamelThickness = @LamelThickness, ProductType = @ProductType, 
                             Quantity = @Quantity, SalesPrice = @SalesPrice, TotalPrice = @TotalPrice, 
                             ShipmentDate = @ShipmentDate, CurrencyRate = @CurrencyRate, Status = @Status, 
                             ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddOrderParameters(command, order);
                    command.Parameters.AddWithValue("@ModifiedDate", order.ModifiedDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "UPDATE Orders SET IsActive = 0, ModifiedDate = @ModifiedDate WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetNextOrderNumber(int year)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = "SELECT COUNT(*) + 1 FROM Orders WHERE YEAR(OrderDate) = @Year AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Year", year);
                    var result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 1;
                }
            }
        }

        private void AddOrderParameters(SqlCommand command, Order order)
        {
            command.Parameters.AddWithValue("@Id", order.Id);
            command.Parameters.AddWithValue("@CompanyId", order.CompanyId);
            command.Parameters.AddWithValue("@CustomerOrderNo", order.CustomerOrderNo);
            command.Parameters.AddWithValue("@TrexOrderNo", order.TrexOrderNo);
            command.Parameters.AddWithValue("@DeviceName", (object)order.DeviceName ?? DBNull.Value);
            
            // DateTime değerlerini kontrol et - SQL Server DateTime aralığı: 1753-01-01 ile 9999-12-31
            var sqlMinDate = new DateTime(1753, 1, 1);
            var sqlMaxDate = new DateTime(9999, 12, 31);
            
            // OrderDate kontrolü
            if (order.OrderDate < sqlMinDate || order.OrderDate > sqlMaxDate)
            {
                command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
            }
            else
            {
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            }
            
            // TermDate kontrolü
            if (order.TermDate < sqlMinDate || order.TermDate > sqlMaxDate)
            {
                command.Parameters.AddWithValue("@TermDate", DateTime.Now.AddDays(7));
            }
            else
            {
                command.Parameters.AddWithValue("@TermDate", order.TermDate);
            }
            
            command.Parameters.AddWithValue("@ProductCode", (object)order.ProductCode ?? DBNull.Value);
            command.Parameters.AddWithValue("@BypassSize", (object)order.BypassSize ?? DBNull.Value);
            command.Parameters.AddWithValue("@BypassType", (object)order.BypassType ?? DBNull.Value);
            command.Parameters.AddWithValue("@LamelThickness", (object)order.LamelThickness ?? DBNull.Value);
            command.Parameters.AddWithValue("@ProductType", (object)order.ProductType ?? DBNull.Value);
            command.Parameters.AddWithValue("@Quantity", order.Quantity);
            command.Parameters.AddWithValue("@SalesPrice", (object)order.SalesPrice ?? DBNull.Value);
            command.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
            
            // ShipmentDate kontrolü (nullable)
            if (order.ShipmentDate.HasValue)
            {
                if (order.ShipmentDate.Value < sqlMinDate || order.ShipmentDate.Value > sqlMaxDate)
                {
                    command.Parameters.AddWithValue("@ShipmentDate", DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@ShipmentDate", order.ShipmentDate.Value);
                }
            }
            else
            {
                command.Parameters.AddWithValue("@ShipmentDate", DBNull.Value);
            }
            
            command.Parameters.AddWithValue("@CurrencyRate", (object)order.CurrencyRate ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", (object)order.Status ?? DBNull.Value);
            
            // CreatedDate kontrolü
            if (order.CreatedDate < sqlMinDate || order.CreatedDate > sqlMaxDate)
            {
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
            }
            else
            {
                command.Parameters.AddWithValue("@CreatedDate", order.CreatedDate);
            }
            
            command.Parameters.AddWithValue("@IsActive", order.IsActive);
        }

        private Order MapToOrder(SqlDataReader reader)
        {
            return new Order
            {
                Id = reader.GetGuid("Id"),
                CompanyId = reader.GetGuid("CompanyId"),
                CustomerOrderNo = reader.GetString("CustomerOrderNo"),
                TrexOrderNo = reader.GetString("TrexOrderNo"),
                DeviceName = reader.IsDBNull("DeviceName") ? null : reader.GetString("DeviceName"),
                OrderDate = reader.GetDateTime("OrderDate"),
                TermDate = reader.GetDateTime("TermDate"),
                ProductCode = reader.IsDBNull("ProductCode") ? null : reader.GetString("ProductCode"),
                BypassSize = reader.IsDBNull("BypassSize") ? null : reader.GetString("BypassSize"),
                BypassType = reader.IsDBNull("BypassType") ? null : reader.GetString("BypassType"),
                LamelThickness = reader.IsDBNull("LamelThickness") ? null : (decimal?)reader.GetDecimal("LamelThickness"),
                ProductType = reader.IsDBNull("ProductType") ? null : reader.GetString("ProductType"),
                Quantity = reader.GetInt32("Quantity"),
                SalesPrice = reader.IsDBNull("SalesPrice") ? null : (decimal?)reader.GetDecimal("SalesPrice"),
                TotalPrice = reader.GetDecimal("TotalPrice"),
                ShipmentDate = reader.IsDBNull("ShipmentDate") ? null : (DateTime?)reader.GetDateTime("ShipmentDate"),
                CurrencyRate = reader.IsDBNull("CurrencyRate") ? null : (decimal?)reader.GetDecimal("CurrencyRate"),
                Status = reader.IsDBNull("Status") ? "Yeni" : reader.GetString("Status"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                ModifiedDate = reader.IsDBNull("ModifiedDate") ? null : (DateTime?)reader.GetDateTime("ModifiedDate"),
                IsActive = reader.GetBoolean("IsActive"),
                Company = reader.IsDBNull("CompanyName") ? null : new Company { Name = reader.GetString("CompanyName") }
            };
        }
    }
}

