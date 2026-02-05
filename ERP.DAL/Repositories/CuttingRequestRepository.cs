using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.DAL.Repositories
{
    public class CuttingRequestRepository : BaseRepository<CuttingRequest>
    {
        public CuttingRequestRepository() : base()
        {
        }

        public CuttingRequestRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<CuttingRequest> GetAll()
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.Machine)
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Employee)
                .Where(cr => cr.IsActive)
                .OrderByDescending(cr => cr.RequestDate)
                .ToList();
        }

        public override CuttingRequest? GetById(Guid id)
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.Machine)
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Employee)
                .FirstOrDefault(cr => cr.Id == id && cr.IsActive);
        }

        public List<CuttingRequest> GetPendingRequests()
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.Machine)
                .Include(cr => cr.SerialNo)
                .Where(cr => cr.Status == "Beklemede" && cr.IsActive)
                .OrderBy(cr => cr.RequestDate)
                .ToList();
        }

        public List<CuttingRequest> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(cr => cr.Machine)
                .Include(cr => cr.SerialNo)
                .Include(cr => cr.Employee)
                .Where(cr => cr.OrderId == orderId && cr.IsActive)
                .OrderByDescending(cr => cr.RequestDate)
                .ToList();
        }
    }
}

