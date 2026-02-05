using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>
    {
        public EmployeeRepository() : base()
        {
        }

        public EmployeeRepository(ErpDbContext context) : base(context)
        {
        }
    }
}

