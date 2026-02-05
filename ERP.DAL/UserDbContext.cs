using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;
using ERP.DAL.Configuration;

namespace ERP.DAL
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public UserDbContext() : base()
        {
        }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConnectionStringManager.GetConnectionString("UserDbConnection");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.IsAdmin).HasDefaultValue(false);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configure UserPermissions
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PermissionKey).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => new { e.UserId, e.PermissionKey }).IsUnique();
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed default admin user
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            
            // Create default admin user (password: admin123)
            // SHA256 hash of "admin123": 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                Username = "admin",
                PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9",
                FullName = "Administrator",
                IsAdmin = true,
                CreatedDate = DateTime.Now,
                IsActive = true
            });

            // Add all permissions for admin user
            var permissionKeys = new[]
            {
                "OrderEntry", "StockEntry", "Accounting", "StockManagement", "ProductionPlanning",
                "CuttingRequests", "PressingRequests", "ClampingRequests", "Clamping2Requests",
                "AssemblyRequests", "Consumption", "ConsumptionMaterialStock", "Reports", "Settings"
            };

            for (int i = 0; i < permissionKeys.Length; i++)
            {
                modelBuilder.Entity<UserPermission>().HasData(new UserPermission
                {
                    Id = Guid.Parse($"00000000-0000-0000-0000-0000000000{(i + 10):D2}"),
                    UserId = adminId,
                    PermissionKey = permissionKeys[i],
                    CreatedDate = DateTime.Now,
                    IsActive = true
                });
            }
        }
    }
}
