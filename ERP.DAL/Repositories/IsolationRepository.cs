using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class IsolationRepository : BaseRepository<Isolation>
    {
        public IsolationRepository() : base()
        {
        }

        public IsolationRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Isolation> GetAll()
        {
            return _dbSet
                .Include(i => i.Order).ThenInclude(o => o.Company)
                .Include(i => i.Assembly)
                .Include(i => i.SerialNo)
                .Include(i => i.Machine)
                .Include(i => i.Employee)
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.IsolationDate)
                .ToList();
        }

        public override Isolation? GetById(Guid id)
        {
            return _dbSet
                .Include(i => i.Order).ThenInclude(o => o.Company)
                .Include(i => i.Assembly)
                .Include(i => i.SerialNo)
                .Include(i => i.Machine)
                .Include(i => i.Employee)
                .FirstOrDefault(i => i.Id == id && i.IsActive);
        }

        public List<Isolation> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(i => i.Assembly)
                .Include(i => i.SerialNo)
                .Include(i => i.Machine)
                .Include(i => i.Employee)
                .Where(i => i.OrderId == orderId && i.IsActive)
                .OrderByDescending(i => i.IsolationDate)
                .ToList();
        }
    }
}


