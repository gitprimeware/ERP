using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class AssemblyRequestRepository : BaseRepository<AssemblyRequest>
    {
        public AssemblyRequestRepository() : base()
        {
        }

        public AssemblyRequestRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<AssemblyRequest> GetAll()
        {
            return _dbSet
                .Include(ar => ar.Order).ThenInclude(o => o.Company)
                .Include(ar => ar.SerialNo)
                .Include(ar => ar.Clamping)
                .Include(ar => ar.Machine)
                .Include(ar => ar.Employee)
                .Where(ar => ar.IsActive)
                .OrderByDescending(ar => ar.RequestDate)
                .ToList();
        }

        public override AssemblyRequest? GetById(Guid id)
        {
            return _dbSet
                .Include(ar => ar.Order).ThenInclude(o => o.Company)
                .Include(ar => ar.SerialNo)
                .Include(ar => ar.Clamping)
                .Include(ar => ar.Machine)
                .Include(ar => ar.Employee)
                .FirstOrDefault(ar => ar.Id == id && ar.IsActive);
        }

        public List<AssemblyRequest> GetPendingRequests()
        {
            return _dbSet
                .Include(ar => ar.Order).ThenInclude(o => o.Company)
                .Include(ar => ar.SerialNo)
                .Include(ar => ar.Clamping)
                .Include(ar => ar.Machine)
                .Where(ar => ar.Status == "Beklemede" && ar.IsActive)
                .OrderBy(ar => ar.RequestDate)
                .ToList();
        }

        public List<AssemblyRequest> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(ar => ar.SerialNo)
                .Include(ar => ar.Clamping)
                .Include(ar => ar.Machine)
                .Include(ar => ar.Employee)
                .Where(ar => ar.OrderId == orderId && ar.IsActive)
                .OrderByDescending(ar => ar.RequestDate)
                .ToList();
        }
    }
}

