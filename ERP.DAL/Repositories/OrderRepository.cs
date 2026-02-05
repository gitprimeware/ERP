using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class OrderRepository : BaseRepository<Order>
    {
        public OrderRepository() : base()
        {
        }

        public OrderRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Order> GetAll()
        {
            return _dbSet
                .Include(o => o.Company)
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> GetAll(string searchTerm = null, Guid? companyId = null)
        {
            var query = _dbSet
                .Include(o => o.Company)
                .Where(o => o.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o => 
                    o.CustomerOrderNo.Contains(searchTerm) ||
                    o.TrexOrderNo.Contains(searchTerm) ||
                    (o.DeviceName != null && o.DeviceName.Contains(searchTerm)) ||
                    (o.Company != null && o.Company.Name.Contains(searchTerm)));
            }

            if (companyId.HasValue)
            {
                query = query.Where(o => o.CompanyId == companyId.Value);
            }

            return query.OrderByDescending(o => o.OrderDate).ToList();
        }

        public override Order? GetById(Guid id)
        {
            return _dbSet
                .Include(o => o.Company)
                .FirstOrDefault(o => o.Id == id && o.IsActive);
        }

        public int GetNextOrderNumber(int year)
        {
            return _dbSet
                .Count(o => o.OrderDate.Year == year && o.IsActive) + 1;
        }

        public int GetNextStockNumber(int year)
        {
            return _dbSet
                .Count(o => o.OrderDate.Year == year && o.IsActive && o.IsStockOrder) + 1;
        }
    }
}

