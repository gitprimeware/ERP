using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class CoverStockRepository : BaseRepository<CoverStock>
    {
        public CoverStockRepository() : base()
        {
        }

        public CoverStockRepository(ErpDbContext context) : base(context)
        {
        }

        public CoverStock? GetByProfileTypeSizeAndLength(string profileType, int size, int coverLength)
        {
            return _dbSet.FirstOrDefault(cs => 
                cs.ProfileType == profileType && 
                cs.Size == size && 
                cs.CoverLength == coverLength && 
                cs.IsActive);
        }

        public CoverStock? GetByProperties(string profileType, int size, int coverLength)
        {
            // Alias for GetByProfileTypeSizeAndLength for backward compatibility
            return GetByProfileTypeSizeAndLength(profileType, size, coverLength);
        }

        public Guid InsertOrUpdate(CoverStock stock)
        {
            // Check if a stock with the same properties exists
            var existing = GetByProfileTypeSizeAndLength(stock.ProfileType, stock.Size, stock.CoverLength);
            
            if (existing != null)
            {
                // Update: increase quantity
                existing.Quantity += stock.Quantity;
                existing.EntryDate = DateTime.Now;
                existing.ModifiedDate = DateTime.Now;
                Update(existing);
                return existing.Id;
            }
            else
            {
                // Insert new stock
                stock.EntryDate = DateTime.Now;
                return Insert(stock);
            }
        }
    }
}

