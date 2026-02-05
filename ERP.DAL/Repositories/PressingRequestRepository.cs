using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PressingRequestRepository : BaseRepository<PressingRequest>
    {
        public PressingRequestRepository() : base()
        {
        }

        public PressingRequestRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<PressingRequest> GetAll()
        {
            return _dbSet
                .Include(pr => pr.Order).ThenInclude(o => o.Company)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Cutting)
                .Include(pr => pr.Employee)
                .Where(pr => pr.IsActive)
                .OrderByDescending(pr => pr.RequestDate)
                .ToList();
        }

        public override PressingRequest? GetById(Guid id)
        {
            return _dbSet
                .Include(pr => pr.Order).ThenInclude(o => o.Company)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Cutting)
                .Include(pr => pr.Employee)
                .FirstOrDefault(pr => pr.Id == id && pr.IsActive);
        }

        public List<PressingRequest> GetPendingRequests()
        {
            return _dbSet
                .Include(pr => pr.Order).ThenInclude(o => o.Company)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Cutting)
                .Where(pr => pr.Status == "Beklemede" && pr.IsActive)
                .OrderBy(pr => pr.RequestDate)
                .ToList();
        }

        public List<PressingRequest> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Cutting)
                .Include(pr => pr.Employee)
                .Where(pr => pr.OrderId == orderId && pr.IsActive)
                .OrderByDescending(pr => pr.RequestDate)
                .ToList();
        }
    }
}

