using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class UserPermissionRepository : BaseRepository<UserPermission>
    {
        public UserPermissionRepository() : base()
        {
        }

        public UserPermissionRepository(ErpDbContext context) : base(context)
        {
        }

        public List<UserPermission> GetByUserId(Guid userId)
        {
            return _dbSet
                .Include(up => up.User)
                .Where(up => up.UserId == userId && up.IsActive)
                .ToList();
        }

        public List<string> GetPermissionKeysByUserId(Guid userId)
        {
            return _dbSet
                .Where(up => up.UserId == userId && up.IsActive)
                .Select(up => up.PermissionKey)
                .ToList();
        }

        public bool HasPermission(Guid userId, string permissionKey)
        {
            return _dbSet.Any(up => 
                up.UserId == userId && 
                up.PermissionKey == permissionKey && 
                up.IsActive);
        }

        public void SetUserPermissions(Guid userId, List<string> permissionKeys)
        {
            // Delete existing permissions
            var existingPermissions = _dbSet.Where(up => up.UserId == userId).ToList();
            _dbSet.RemoveRange(existingPermissions);

            // Add new permissions
            foreach (var permissionKey in permissionKeys)
            {
                var permission = new UserPermission
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PermissionKey = permissionKey,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };
                _dbSet.Add(permission);
            }

            _context.SaveChanges();
        }

        public void DeleteByUserId(Guid userId)
        {
            var permissions = _dbSet.Where(up => up.UserId == userId).ToList();
            foreach (var permission in permissions)
            {
                permission.IsActive = false;
                permission.ModifiedDate = DateTime.Now;
            }
            _context.SaveChanges();
        }
    }
}

