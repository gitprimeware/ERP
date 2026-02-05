using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ERP.Core.Models;

namespace ERP.DAL.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository() : base()
        {
        }

        public UserRepository(ErpDbContext context) : base(context)
        {
        }
        public override Guid Insert(User entity)
        {
            entity.PasswordHash = ComputeSha256Hash(entity.PasswordHash);

            return base.Insert(entity);
        }
        public User? GetByUsername(string username)
        {
            return _dbSet.FirstOrDefault(u => u.Username == username && u.IsActive);
        }

        public bool UsernameExists(string username, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Username == username && u.IsActive);
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }
            return query.Any();
        }

        public User? Authenticate(string username, string password)
        {
            var passwordHash = ComputeSha256Hash(password);
            return _dbSet.FirstOrDefault(u => 
                u.Username == username && 
                u.PasswordHash == passwordHash && 
                u.IsActive);
        }

        public void UpdatePassword(Guid userId, string newPassword)
        {
            var user = _dbSet.Find(userId);
            if (user != null)
            {
                user.PasswordHash = ComputeSha256Hash(newPassword);
                user.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

