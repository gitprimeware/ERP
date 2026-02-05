using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PressingRepository : BaseRepository<Pressing>
    {
        public PressingRepository() : base()
        {
        }

        public PressingRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Pressing> GetAll()
        {
            return _dbSet
                .Include(p => p.Order).ThenInclude(o => o.Company)
                .Include(p => p.SerialNo)
                .Include(p => p.Cutting)
                .Include(p => p.Employee)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PressingDate)
                .ToList();
        }

        public override Pressing? GetById(Guid id)
        {
            return _dbSet
                .Include(p => p.Order).ThenInclude(o => o.Company)
                .Include(p => p.SerialNo)
                .Include(p => p.Cutting)
                .Include(p => p.Employee)
                .FirstOrDefault(p => p.Id == id && p.IsActive);
        }

        public List<Pressing> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(p => p.SerialNo)
                .Include(p => p.Cutting)
                .Include(p => p.Employee)
                .Where(p => p.OrderId == orderId && p.IsActive)
                .OrderByDescending(p => p.PressingDate)
                .ToList();
        }
    }
}

