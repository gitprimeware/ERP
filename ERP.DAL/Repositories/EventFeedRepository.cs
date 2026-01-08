using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class EventFeedRepository
    {
        public Guid Insert(EventFeed eventFeed)
        {
            eventFeed.Id = Guid.NewGuid();
            eventFeed.CreatedDate = DateTime.Now;
            eventFeed.IsActive = true;
            eventFeed.EventDate = DateTime.Now;
            eventFeed.IsRead = false;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"INSERT INTO EventFeeds (Id, EventType, Title, Message, RequiredPermission, 
                             RelatedEntityId, RelatedEntityType, CreatedByUserId, EventDate, IsRead, 
                             CreatedDate, IsActive) 
                             VALUES (@Id, @EventType, @Title, @Message, @RequiredPermission, 
                             @RelatedEntityId, @RelatedEntityType, @CreatedByUserId, @EventDate, @IsRead, 
                             @CreatedDate, @IsActive)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    AddEventFeedParameters(command, eventFeed);
                    command.ExecuteNonQuery();
                }
            }
            
            return eventFeed.Id;
        }

        public List<EventFeed> GetByUserPermissions(Guid userId, bool isAdmin, int? limit = null)
        {
            var events = new List<EventFeed>();
            
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                // Admin ise tüm event'leri göster, değilse sadece izinleri olan event'leri göster
                string query;
                if (isAdmin)
                {
                    query = @"SELECT ef.Id, ef.EventType, ef.Title, ef.Message, ef.RequiredPermission, 
                             ef.RelatedEntityId, ef.RelatedEntityType, ef.CreatedByUserId, ef.EventDate, 
                             ef.IsRead, ef.CreatedDate, ef.ModifiedDate, ef.IsActive,
                             u.FullName as CreatedByUserName
                             FROM EventFeeds ef
                             LEFT JOIN Users u ON ef.CreatedByUserId = u.Id
                             WHERE ef.IsActive = 1";
                }
                else
                {
                    // RequiredPermission virgülle ayrılmış izinler olabilir (örn: "CuttingRequests,ProductionPlanning")
                    // Kullanıcının herhangi bir iznine sahipse event'i göster
                    query = @"SELECT DISTINCT ef.Id, ef.EventType, ef.Title, ef.Message, ef.RequiredPermission, 
                             ef.RelatedEntityId, ef.RelatedEntityType, ef.CreatedByUserId, ef.EventDate, 
                             ef.IsRead, ef.CreatedDate, ef.ModifiedDate, ef.IsActive,
                             u.FullName as CreatedByUserName
                             FROM EventFeeds ef
                             LEFT JOIN Users u ON ef.CreatedByUserId = u.Id
                             LEFT JOIN UserPermissions up ON (
                                 ef.RequiredPermission IS NULL OR ef.RequiredPermission = '' OR
                                 ef.RequiredPermission = up.PermissionKey OR
                                 ef.RequiredPermission LIKE up.PermissionKey + ',%' OR
                                 ef.RequiredPermission LIKE '%,' + up.PermissionKey + ',%' OR
                                 ef.RequiredPermission LIKE '%,' + up.PermissionKey
                             ) AND up.UserId = @UserId
                             WHERE ef.IsActive = 1 
                             AND (ef.RequiredPermission IS NULL OR ef.RequiredPermission = '' OR up.Id IS NOT NULL)";
                }
                
                query += " ORDER BY ef.EventDate DESC";
                
                if (limit.HasValue)
                {
                    query += $" OFFSET 0 ROWS FETCH NEXT {limit.Value} ROWS ONLY";
                }
                
                using (var command = new SqlCommand(query, connection))
                {
                    if (!isAdmin)
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                    }
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(MapToEventFeed(reader));
                        }
                    }
                }
            }
            
            return events;
        }

        public void MarkAsRead(Guid eventFeedId, Guid userId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE EventFeeds SET IsRead = 1, ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", eventFeedId);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void MarkAsUnread(Guid eventFeedId, Guid userId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE EventFeeds SET IsRead = 0, ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", eventFeedId);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(Guid eventFeedId, Guid userId)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var query = @"UPDATE EventFeeds SET IsActive = 0, ModifiedDate = @ModifiedDate 
                             WHERE Id = @Id";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", eventFeedId);
                    command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void AddEventFeedParameters(SqlCommand command, EventFeed eventFeed)
        {
            command.Parameters.AddWithValue("@Id", eventFeed.Id);
            command.Parameters.AddWithValue("@EventType", eventFeed.EventType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Title", eventFeed.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Message", eventFeed.Message ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RequiredPermission", eventFeed.RequiredPermission ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RelatedEntityId", eventFeed.RelatedEntityId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@RelatedEntityType", eventFeed.RelatedEntityType ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedByUserId", eventFeed.CreatedByUserId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@EventDate", eventFeed.EventDate);
            command.Parameters.AddWithValue("@IsRead", eventFeed.IsRead);
            command.Parameters.AddWithValue("@CreatedDate", eventFeed.CreatedDate);
            command.Parameters.AddWithValue("@IsActive", eventFeed.IsActive);
        }

        private EventFeed MapToEventFeed(SqlDataReader reader)
        {
            return new EventFeed
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                EventType = reader.IsDBNull(reader.GetOrdinal("EventType")) ? null : reader.GetString(reader.GetOrdinal("EventType")),
                Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? null : reader.GetString(reader.GetOrdinal("Title")),
                Message = reader.IsDBNull(reader.GetOrdinal("Message")) ? null : reader.GetString(reader.GetOrdinal("Message")),
                RequiredPermission = reader.IsDBNull(reader.GetOrdinal("RequiredPermission")) ? null : reader.GetString(reader.GetOrdinal("RequiredPermission")),
                RelatedEntityId = reader.IsDBNull(reader.GetOrdinal("RelatedEntityId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("RelatedEntityId")),
                RelatedEntityType = reader.IsDBNull(reader.GetOrdinal("RelatedEntityType")) ? null : reader.GetString(reader.GetOrdinal("RelatedEntityType")),
                CreatedByUserId = reader.IsDBNull(reader.GetOrdinal("CreatedByUserId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("CreatedByUserId")),
                EventDate = reader.GetDateTime(reader.GetOrdinal("EventDate")),
                IsRead = reader.IsDBNull(reader.GetOrdinal("IsRead")) ? false : reader.GetBoolean(reader.GetOrdinal("IsRead")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}

