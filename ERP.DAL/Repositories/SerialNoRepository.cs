using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class SerialNoRepository : BaseRepository<SerialNo>
    {
        public SerialNoRepository() : base()
        {
        }

        public SerialNoRepository(ErpDbContext context) : base(context)
        {
        }
    }
}

