using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PackagingRepository : BaseRepository<Packaging>
    {
        public PackagingRepository() : base()
        {
        }

        public PackagingRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Packaging> GetAll()
        {
            return _dbSet
                .Include(p => p.Order).ThenInclude(o => o.Company)
                .Include(p => p.Assembly)
                .Include(p => p.Isolation)
                .Include(p => p.SerialNo)
                .Include(p => p.Machine)
                .Include(p => p.Employee)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PackagingDate)
                .ToList();
        }

        public override Packaging? GetById(Guid id)
        {
            return _dbSet
                .Include(p => p.Order).ThenInclude(o => o.Company)
                .Include(p => p.Assembly)
                .Include(p => p.Isolation)
                .Include(p => p.SerialNo)
                .Include(p => p.Machine)
                .Include(p => p.Employee)
                .FirstOrDefault(p => p.Id == id && p.IsActive);
        }

        public List<Packaging> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(p => p.Assembly)
                .Include(p => p.Isolation)
                .Include(p => p.SerialNo)
                .Include(p => p.Machine)
                .Include(p => p.Employee)
                .Where(p => p.OrderId == orderId && p.IsActive)
                .OrderByDescending(p => p.PackagingDate)
                .ToList();
        }
    }
}

