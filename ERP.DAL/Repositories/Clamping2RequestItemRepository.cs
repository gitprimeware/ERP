using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class Clamping2RequestItemRepository
    {
        public List<Clamping2RequestItem> GetAll()
        {
            var items = new List<Clamping2RequestItem>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cri.Id, cri.Clamping2RequestId, cri.ClampingId, cri.Sequence,
                             cri.CreatedDate, cri.ModifiedDate, cri.IsActive
                             FROM Clamping2RequestItems cri
                             WHERE cri.IsActive = 1
                             ORDER BY cri.Clamping2RequestId, cri.Sequence";
                
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(MapToClamping2RequestItem(reader));
                        }
                    }
                }
            }
            
            return items;
        }

        public List<Clamping2RequestItem> GetByClamping2RequestId(Guid clamping2RequestId)
        {
            var items = new List<Clamping2RequestItem>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"SELECT cri.Id, cri.Clamping2RequestId, cri.ClampingId, cri.Sequence,
                             cri.CreatedDate, cri.ModifiedDate, cri.IsActive
                             FROM Clamping2RequestItems cri
                             WHERE cri.Clamping2RequestId = @Clamping2RequestId AND cri.IsActive = 1
                             ORDER BY cri.Sequence";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Clamping2RequestId", clamping2RequestId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(MapToClamping2RequestItem(reader));
                        }
                    }
                }
            }
            
            return items;
        }

        public Guid Insert(Clamping2RequestItem item)
        {
            item.Id = Guid.NewGuid();
            item.CreatedDate = DateTime.Now;
            item.IsActive = true;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO Clamping2RequestItems (Id, Clamping2RequestId, ClampingId, Sequence,
                             CreatedDate, IsActive)
                             VALUES (@Id, @Clamping2RequestId, @ClampingId, @Sequence,
                             @CreatedDate, @IsActive)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@Clamping2RequestId", item.Clamping2RequestId);
                    command.Parameters.AddWithValue("@ClampingId", item.ClampingId);
                    command.Parameters.AddWithValue("@Sequence", item.Sequence);
                    command.Parameters.AddWithValue("@CreatedDate", item.CreatedDate);
                    command.Parameters.AddWithValue("@IsActive", item.IsActive);
                    command.ExecuteNonQuery();
                }
            }
            return item.Id;
        }

        public void DeleteByClamping2RequestId(Guid clamping2RequestId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE Clamping2RequestItems 
                             SET IsActive = 0, ModifiedDate = @ModifiedDate
                             WHERE Clamping2RequestId = @Clamping2RequestId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Clamping2RequestId", clamping2RequestId);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private Clamping2RequestItem MapToClamping2RequestItem(SqlDataReader reader)
        {
            return new Clamping2RequestItem
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Clamping2RequestId = reader.GetGuid(reader.GetOrdinal("Clamping2RequestId")),
                ClampingId = reader.GetGuid(reader.GetOrdinal("ClampingId")),
                Sequence = reader.GetInt32(reader.GetOrdinal("Sequence")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}

