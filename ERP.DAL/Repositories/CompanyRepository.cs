using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class CompanyRepository : BaseRepository<Company>
    {
        public CompanyRepository() : base()
        {
        }

        public CompanyRepository(ErpDbContext context) : base(context)
        {
        }

        // Add any Company-specific methods here if needed
        public List<Company> GetCompaniesByName(string name)
        {
            return Find(c => c.Name.Contains(name));
        }
    }
}

