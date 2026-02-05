using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class MachineRepository : BaseRepository<Machine>
    {
        public MachineRepository() : base()
        {
        }

        public MachineRepository(ErpDbContext context) : base(context)
        {
        }
    }
}

