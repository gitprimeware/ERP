using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class SideProfileRemnantRepository : BaseRepository<SideProfileRemnant>
    {
        public SideProfileRemnantRepository() : base()
        {
        }

        public SideProfileRemnantRepository(ErpDbContext context) : base(context)
        {
        }

        public List<SideProfileRemnant> GetAll(bool includeWaste)
        {
            var query = _dbSet.Where(r => r.IsActive);
            
            if (!includeWaste)
            {
                query = query.Where(r => !r.IsWaste);
            }
            
            return query.OrderByDescending(r => r.Length).ToList();
        }

        public List<SideProfileRemnant> GetUsableRemnants(string profileType)
        {
            return _dbSet
                .Where(r => r.ProfileType == profileType && !r.IsWaste && r.IsActive)
                .OrderByDescending(r => r.Length)
                .ToList();
        }

        public Guid InsertOrMerge(SideProfileRemnant remnant)
        {
            // Check if a remnant with the same profile type and length exists
            var existing = _dbSet.FirstOrDefault(r => 
                r.ProfileType == remnant.ProfileType && 
                r.Length == remnant.Length && 
                !r.IsWaste && 
                r.IsActive);
            
            if (existing != null)
            {
                // Merge: increase quantity
                existing.Quantity += remnant.Quantity;
                existing.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
                return existing.Id;
            }
            else
            {
                // Insert new remnant
                return Insert(remnant);
            }
        }

        public void MarkAsWaste(Guid id)
        {
            var remnant = _dbSet.Find(id);
            if (remnant != null)
            {
                remnant.IsWaste = true;
                remnant.WasteDate = DateTime.Now;
                remnant.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }
    }
}

