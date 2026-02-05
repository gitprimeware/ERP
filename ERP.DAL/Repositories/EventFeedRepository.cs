using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class EventFeedRepository : BaseRepository<EventFeed>
    {
        public EventFeedRepository() : base()
        {
        }

        public EventFeedRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<EventFeed> GetAll()
        {
            return _dbSet
                .Include(e => e.CreatedByUser)
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.EventDate)
                .ToList();
        }

        public List<EventFeed> GetUnreadEvents()
        {
            return _dbSet
                .Include(e => e.CreatedByUser)
                .Where(e => e.IsActive && !e.IsRead)
                .OrderByDescending(e => e.EventDate)
                .ToList();
        }

        public List<EventFeed> GetByUserPermissions(Guid userId, bool isAdmin, int limit = 100)
        {
            var query = _dbSet
                .Include(e => e.CreatedByUser)
                .Where(e => e.IsActive);

            if (!isAdmin)
            {
                // Get user's permissions
                var userPermissions = _context.UserPermissions
                    .Where(up => up.UserId == userId && up.IsActive)
                    .Select(up => up.PermissionKey)
                    .ToList();

                // Filter events based on required permissions
                query = query.Where(e => 
                    e.RequiredPermission == null || 
                    string.IsNullOrEmpty(e.RequiredPermission) ||
                    userPermissions.Contains(e.RequiredPermission));
            }

            return query
                .OrderByDescending(e => e.EventDate)
                .Take(limit)
                .ToList();
        }

        public void MarkAsRead(Guid id)
        {
            var eventFeed = _dbSet.Find(id);
            if (eventFeed != null)
            {
                eventFeed.IsRead = true;
                eventFeed.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void MarkAsRead(Guid id, Guid userId)
        {
            // For backward compatibility - userId parameter can be used for future user-specific read tracking
            MarkAsRead(id);
        }

        public void MarkAsUnread(Guid id, Guid userId)
        {
            var eventFeed = _dbSet.Find(id);
            if (eventFeed != null)
            {
                eventFeed.IsRead = false;
                eventFeed.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void Delete(Guid id, Guid userId)
        {
            // For backward compatibility - userId parameter can be used for audit logging
            Delete(id);
        }
    }
}

