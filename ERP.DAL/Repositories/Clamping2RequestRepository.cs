using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class Clamping2RequestRepository : BaseRepository<Clamping2Request>
    {
        public Clamping2RequestRepository() : base()
        {
        }

        public Clamping2RequestRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<Clamping2Request> GetAll()
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.FirstClamping)
                .Include(cr => cr.SecondClamping)
                .Include(cr => cr.Machine)
                .Include(cr => cr.Employee)
                .Where(cr => cr.IsActive)
                .OrderByDescending(cr => cr.RequestDate)
                .ToList();
        }

        public override Clamping2Request? GetById(Guid id)
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.FirstClamping)
                .Include(cr => cr.SecondClamping)
                .Include(cr => cr.Machine)
                .Include(cr => cr.Employee)
                .FirstOrDefault(cr => cr.Id == id && cr.IsActive);
        }

        public List<Clamping2Request> GetPendingRequests()
        {
            return _dbSet
                .Include(cr => cr.Order).ThenInclude(o => o.Company)
                .Include(cr => cr.FirstClamping)
                .Include(cr => cr.SecondClamping)
                .Include(cr => cr.Machine)
                .Where(cr => cr.Status == "Beklemede" && cr.IsActive)
                .OrderBy(cr => cr.RequestDate)
                .ToList();
        }

        public List<Clamping2Request> GetByOrderId(Guid orderId)
        {
            return _dbSet
                .Include(cr => cr.FirstClamping)
                .Include(cr => cr.SecondClamping)
                .Include(cr => cr.Machine)
                .Include(cr => cr.Employee)
                .Where(cr => cr.OrderId == orderId && cr.IsActive)
                .OrderByDescending(cr => cr.RequestDate)
                .ToList();
        }
    }
}

