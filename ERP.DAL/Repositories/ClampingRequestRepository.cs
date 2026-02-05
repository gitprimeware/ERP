using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class ClampingRequestRepository : BaseRepository<ClampingRequest>
    {
        public ClampingRequestRepository() : base()
        {
        }

        public ClampingRequestRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<ClampingRequest> GetAll()
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Pressing)
                .Include(cr => cr.Machine)
                .Include(cr => cr.Employee)
                .Where(cr => cr.IsActive)
                .OrderByDescending(cr => cr.RequestDate)
                .ToList();
        }

        public override ClampingRequest? GetById(Guid id)
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Pressing)
                .Include(cr => cr.Machine)
                .Include(cr => cr.Employee)
                .FirstOrDefault(cr => cr.Id == id && cr.IsActive);
        }

        public List<ClampingRequest> GetPendingRequests()
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Pressing)
                .Include(cr => cr.Machine)
                .Where(cr => cr.Status == "Beklemede" && cr.IsActive)
                .OrderBy(cr => cr.RequestDate)
                .ToList();
        }

        public List<ClampingRequest> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Pressing)
                .Include(cr => cr.Machine)
                .Include(cr => cr.Employee)
                .Where(cr => cr.OrderId == orderId && cr.IsActive)
                .OrderByDescending(cr => cr.RequestDate)
                .ToList();
        }
    }
}

