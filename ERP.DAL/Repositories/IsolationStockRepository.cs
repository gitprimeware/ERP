using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class IsolationStockRepository : BaseRepository<IsolationStock>
    {
        public IsolationStockRepository() : base()
        {
        }

        public IsolationStockRepository(ErpDbContext context) : base(context)
        {
        }

        public List<IsolationStock> GetAllActiveStocks()
        {
            return _dbSet.Where(i => i.IsActive).OrderByDescending(i => i.EntryDate).ToList();
        }

        public IsolationStock? GetById(Guid id)
        {
            return _dbSet.FirstOrDefault(i => i.Id == id && i.IsActive);
        }

        public IsolationStock? GetByLiquidType(string liquidType)
        {
            return _dbSet.FirstOrDefault(i => i.LiquidType == liquidType && i.IsActive);
        }

        public Guid Insert(IsolationStock stock)
        {
            stock.Id = Guid.NewGuid();
            stock.CreatedDate = DateTime.Now;
            stock.EntryDate = DateTime.Now;
            stock.IsActive = true;

            _dbSet.Add(stock);
            _context.SaveChanges();

            return stock.Id;
        }

        public void Update(IsolationStock stock)
        {
            stock.ModifiedDate = DateTime.Now;

            _dbSet.Update(stock);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var stock = GetById(id);
            if (stock != null)
            {
                stock.IsActive = false;
                stock.ModifiedDate = DateTime.Now;

                _dbSet.Update(stock);
                _context.SaveChanges();
            }
        }

        // Toplam stok hesaplamaları için yardımcı metodlar
        public (decimal TotalIsosiyanat, decimal TotalPoliol, decimal TotalLiter) GetTotalStocks()
        {
            decimal totalIsosiyanat = 0;
            decimal totalPoliol = 0;
            decimal totalLiter = 0;

            var stocks = GetAllActiveStocks();
            foreach (var stock in stocks)
            {
                if (stock.LiquidType == "İzosiyanat")
                {
                    totalIsosiyanat += stock.Liter;
                    totalLiter += stock.Liter;
                }
                else if (stock.LiquidType == "Poliol")
                {
                    totalPoliol += stock.Liter;
                    totalLiter += stock.Liter;
                }
                else if (stock.LiquidType == "İzolasyon")
                {
                    // İzolasyon karışımı genellikle 1:1 oranında (50% İzosiyanat, 50% Poliol)
                    decimal halfLiter = stock.Liter / 2.0m;
                    totalIsosiyanat += halfLiter;
                    totalPoliol += halfLiter;
                    totalLiter += stock.Liter;
                }
            }

            return (totalIsosiyanat, totalPoliol, totalLiter);
        }
    }
}

