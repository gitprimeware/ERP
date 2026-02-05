using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class Clamping2RequestItemRepository : BaseRepository<Clamping2RequestItem>
    {
        public Clamping2RequestItemRepository() : base()
        {
        }

        public Clamping2RequestItemRepository(ErpDbContext context) : base(context)
        {
        }

        public List<Clamping2RequestItem> GetByRequestId(Guid requestId)
        {
            return _dbSet
                .Include(ci => ci.Clamping2Request)
                .Include(ci => ci.Clamping)
                .Where(ci => ci.Clamping2RequestId == requestId && ci.IsActive)
                .OrderBy(ci => ci.Sequence)
                .ToList();
        }
    }
}

