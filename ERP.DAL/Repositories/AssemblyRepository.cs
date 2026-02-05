using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class AssemblyRepository : BaseRepository<Assembly>
    {
        public AssemblyRepository() : base()
        {
        }

        public AssemblyRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Assembly> GetAll()
        {
            return _dbSet
                .Include(a => a.Order).ThenInclude(o => o.Company)
                .Include(a => a.Clamping)
                .Include(a => a.SerialNo)
                .Include(a => a.Machine)
                .Include(a => a.Employee)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.AssemblyDate)
                .ToList();
        }

        public override Assembly? GetById(Guid id)
        {
            return _dbSet
                .Include(a => a.Order).ThenInclude(o => o.Company)
                .Include(a => a.Clamping)
                .Include(a => a.SerialNo)
                .Include(a => a.Machine)
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.Id == id && a.IsActive);
        }

        public List<Assembly> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(a => a.Clamping)
                .Include(a => a.SerialNo)
                .Include(a => a.Machine)
                .Include(a => a.Employee)
                .Where(a => a.OrderId == orderId && a.IsActive)
                .OrderByDescending(a => a.AssemblyDate)
                .ToList();
        }
    }
}

