using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL;

namespace ERP.DAL.Repositories
{
    public class MaterialEntryRepository : BaseRepository<MaterialEntry>
    {
        public MaterialEntryRepository() : base()
        {
        }

        public MaterialEntryRepository(ErpDbContext context) : base(context)
        {
        }

        public override List<MaterialEntry> GetAll()
        {
            return _dbSet
                .Include(me => me.Supplier)
                .Include(me => me.SerialNo)
                .Where(me => me.IsActive)
                .OrderByDescending(me => me.EntryDate)
                .ToList();
        }

        public List<MaterialEntry> GetAll(string searchTerm = null, Guid? supplierId = null)
        {
            var query = _dbSet
                .Include(me => me.Supplier)
                .Include(me => me.SerialNo)
                .Where(me => me.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(me =>
                    me.TransactionType.Contains(searchTerm) ||
                    me.MaterialType.Contains(searchTerm) ||
                    me.MaterialSize.Contains(searchTerm) ||
                    (me.InvoiceNo != null && me.InvoiceNo.Contains(searchTerm)) ||
                    (me.TrexPurchaseNo != null && me.TrexPurchaseNo.Contains(searchTerm)) ||
                    (me.Supplier != null && me.Supplier.Name.Contains(searchTerm)));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(me => me.SupplierId == supplierId.Value);
            }

            return query.OrderByDescending(me => me.EntryDate).ToList();
        }

        public override MaterialEntry? GetById(Guid id)
        {
            return _dbSet
                .Include(me => me.Supplier)
                .Include(me => me.SerialNo)
                .FirstOrDefault(me => me.Id == id && me.IsActive);
        }

        public override Guid Insert(MaterialEntry entry)
        {
            if (entry.EntryDate == default(DateTime))
            {
                entry.EntryDate = DateTime.Now;
            }
            return base.Insert(entry);
        }
    }
}

