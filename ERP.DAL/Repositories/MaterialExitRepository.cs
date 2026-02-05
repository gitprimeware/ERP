using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class MaterialExitRepository : BaseRepository<MaterialExit>
    {
        // Parameterless constructor for dependency injection
        public MaterialExitRepository() : base()
        {
        }

        // Constructor that accepts a DbContext, useful for unit testing
        public MaterialExitRepository(ErpDbContext context) : base(context)
        {
        }

        // Gets all active MaterialExits, includes related Company data, and orders by ExitDate descending
        public override List<MaterialExit> GetAll()
        {
            return _dbSet
                .Include(me => me.Company)
                .Where(me => me.IsActive)
                .OrderByDescending(me => me.ExitDate)
                .ToList();
        }

        // Gets all active MaterialExits with optional search and company filtering
        public List<MaterialExit> GetAll(string? searchTerm, Guid? companyId)
        {
            var query = _dbSet
                .Include(me => me.Company)
                .Where(me => me.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(me =>
                    me.TransactionType.Contains(searchTerm) ||
                    me.MaterialType.Contains(searchTerm) ||
                    me.MaterialSize.Contains(searchTerm) ||
                    (me.TrexInvoiceNo != null && me.TrexInvoiceNo.Contains(searchTerm)) ||
                    (me.Company != null && me.Company.Name.Contains(searchTerm)));
            }

            if (companyId.HasValue)
            {
                query = query.Where(me => me.CompanyId == companyId.Value);
            }

            return query.OrderByDescending(me => me.ExitDate).ToList();
        }

        // Gets a specific MaterialExit by Id, includes related Company data, and checks if it's active
        public override MaterialExit? GetById(Guid id)
        {
            return _dbSet
                .Include(me => me.Company)
                .FirstOrDefault(me => me.Id == id && me.IsActive);
        }
    }
}

