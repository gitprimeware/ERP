using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using System.Linq.Expressions;

namespace ERP.DAL.Repositories
{
    public class BaseRepository<T> where T : BaseModel
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository()
        {
            _context = new ErpDbContext();
            _dbSet = _context.Set<T>();
        }

        public BaseRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // For backward compatibility with existing code
        public BaseRepository(ErpDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual List<T> GetAll()
        {
            return _dbSet.Where(e => e.IsActive).ToList();
        }

        public virtual T? GetById(Guid id)
        {
            return _dbSet.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public virtual List<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).Where(e => e.IsActive).ToList();
        }

        public virtual Guid Insert(T entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = true;

            _dbSet.Add(entity);
            _context.SaveChanges();
            
            return entity.Id;
        }

        public virtual void Update(T entity)
        {
            entity.ModifiedDate = DateTime.Now;
            
            _dbSet.Update(entity);
            _context.SaveChanges();
        }

        public virtual void Delete(Guid id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public virtual void HardDelete(Guid id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
        }

        public virtual bool Exists(Guid id)
        {
            return _dbSet.Any(e => e.Id == id && e.IsActive);
        }

        public virtual int Count()
        {
            return _dbSet.Count(e => e.IsActive);
        }

        public virtual int Count(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).Count(e => e.IsActive);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
