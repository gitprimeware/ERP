using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class CuttingRepository : BaseRepository<Cutting>
    {
        public CuttingRepository() : base()
        {
        }

        public CuttingRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Cutting> GetAll()
        {
            return _dbSet
                .Include(c => c.Order).ThenInclude(o => o.Company)
                .Include(c => c.Machine)
                .Include(c => c.SerialNo)
                .Include(c => c.Employee)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CuttingDate)
                .ToList();
        }

        public override Cutting? GetById(Guid id)
        {
            return _dbSet
                .Include(c => c.Order).ThenInclude(o => o.Company)
                .Include(c => c.Machine)
                .Include(c => c.SerialNo)
                .Include(c => c.Employee)
                .FirstOrDefault(c => c.Id == id && c.IsActive);
        }

        public List<Cutting> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(c => c.Machine)
                .Include(c => c.SerialNo)
                .Include(c => c.Employee)
                .Where(c => c.OrderId == orderId && c.IsActive)
                .OrderByDescending(c => c.CuttingDate)
                .ToList();
        }
    }
}

