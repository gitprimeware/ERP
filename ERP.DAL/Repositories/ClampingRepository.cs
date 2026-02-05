using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class ClampingRepository : BaseRepository<Clamping>
    {
        public ClampingRepository() : base()
        {
        }

        public ClampingRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Clamping> GetAll()
        {
            return _dbSet
                .Include(c => c.Order).ThenInclude(o => o.Company)
                .Include(c => c.Pressing)
                .Include(c => c.SerialNo)
                .Include(c => c.Machine)
                .Include(c => c.Employee)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.ClampingDate)
                .ToList();
        }

        public override Clamping? GetById(Guid id)
        {
            return _dbSet
                .Include(c => c.Order).ThenInclude(o => o.Company)
                .Include(c => c.Pressing)
                .Include(c => c.SerialNo)
                .Include(c => c.Machine)
                .Include(c => c.Employee)
                .FirstOrDefault(c => c.Id == id && c.IsActive);
        }

        public List<Clamping> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(c => c.Pressing)
                .Include(c => c.SerialNo)
                .Include(c => c.Machine)
                .Include(c => c.Employee)
                .Where(c => c.OrderId == orderId && c.IsActive)
                .OrderByDescending(c => c.ClampingDate)
                .ToList();
        }
    }
}

