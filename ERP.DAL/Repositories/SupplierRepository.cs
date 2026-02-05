using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class SupplierRepository : BaseRepository<Supplier>
    {
        public SupplierRepository() : base()
        {
        }

        public SupplierRepository(ErpDbContext context) : base(context)
        {
        }
    }
}

