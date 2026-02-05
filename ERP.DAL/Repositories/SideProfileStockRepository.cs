using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class SideProfileStockRepository : BaseRepository<SideProfileStock>
    {
        public SideProfileStockRepository() : base()
        {
        }

        public SideProfileStockRepository(ErpDbContext context) : base(context)
        {
        }

        public List<SideProfileStock> GetByProfileType(string profileType)
        {
            return _dbSet
                .Where(s => s.ProfileType == profileType && s.IsActive)
                .OrderBy(s => s.EntryDate)
                .ToList();
        }

        public SideProfileStock GetById(Guid id)
        {
            return _dbSet.SingleOrDefault(s => s.Id == id && s.IsActive);
        }

        public SideProfileStock GetByLengthAndProfileType(decimal length, string profileType)
        {
            return _dbSet.SingleOrDefault(s => s.Length == length && s.ProfileType == profileType && s.IsActive);
        }

        public SideProfileStock GetByLength(decimal length)
        {
            // Backward compatibility - returns first match (deprecated, use GetByLengthAndProfileType instead)
            return _dbSet
                .Where(s => s.Length == length && s.IsActive)
                .OrderByDescending(s => s.EntryDate)
                .FirstOrDefault();
        }

        public Guid InsertOrUpdate(SideProfileStock stock)
        {
            // Önce mevcut kaydı kontrol et (ProfileType ve Length'e göre)
            var existing = GetByLengthAndProfileType(stock.Length, stock.ProfileType);
            
            if (existing != null)
            {
                // Mevcut kayıt varsa adedi artır
                existing.InitialQuantity += stock.InitialQuantity;
                existing.RemainingLength = (existing.Length * existing.InitialQuantity) - existing.UsedLength - existing.WastedLength;
                existing.EntryDate = DateTime.Now;
                existing.ModifiedDate = DateTime.Now;
                Update(existing);
                return existing.Id;
            }
            else
            {
                // Yeni kayıt oluştur
                stock.RemainingLength = stock.Length * stock.InitialQuantity;
                return Insert(stock);
            }
        }

        public override Guid Insert(SideProfileStock stock)
        {
            stock.EntryDate = DateTime.Now;
            stock.RemainingLength = stock.Length * stock.InitialQuantity;
            return base.Insert(stock);
        }

        public override void Update(SideProfileStock stock)
        {
            stock.RemainingLength = (stock.Length * stock.InitialQuantity) - stock.UsedLength - stock.WastedLength;
            base.Update(stock);
        }
    }
}

