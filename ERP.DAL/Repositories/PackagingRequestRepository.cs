using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class PackagingRequestRepository : BaseRepository<PackagingRequest>
    {
        public PackagingRequestRepository() : base()
        {
        }

        public PackagingRequestRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<PackagingRequest> GetAll()
        {
            return _dbSet
                .Include(pr => pr.Order).ThenInclude(o => o.Company)
                .Include(pr => pr.Isolation)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Machine)
                .Include(pr => pr.Employee)
                .Where(pr => pr.IsActive)
                .OrderByDescending(pr => pr.RequestDate)
                .ToList();
        }

        public override PackagingRequest? GetById(Guid id)
        {
            return _dbSet
                .Include(pr => pr.Order).ThenInclude(o => o.Company)
                .Include(pr => pr.Isolation)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Machine)
                .Include(pr => pr.Employee)
                .FirstOrDefault(pr => pr.Id == id && pr.IsActive);
        }

        public List<PackagingRequest> GetPendingRequests()
        {
            return _dbSet
                .Include(pr => pr.Order).ThenInclude(o => o.Company)
                .Include(pr => pr.Isolation)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Machine)
                .Where(pr => pr.Status == "Beklemede" && pr.IsActive)
                .OrderBy(pr => pr.RequestDate)
                .ToList();
        }

        public List<PackagingRequest> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(pr => pr.Isolation)
                .Include(pr => pr.SerialNo)
                .Include(pr => pr.Machine)
                .Include(pr => pr.Employee)
                .Where(pr => pr.OrderId == orderId && pr.IsActive)
                .OrderByDescending(pr => pr.RequestDate)
                .ToList();
        }
    }
}

