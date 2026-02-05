using Microsoft.EntityFrameworkCore;
using ERP.Core.Models;

namespace ERP.DAL
{
    public class ErpDbContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SerialNo> SerialNos { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MaterialEntry> MaterialEntries { get; set; }
        public DbSet<MaterialExit> MaterialExits { get; set; }
        public DbSet<Cutting> Cuttings { get; set; }
        public DbSet<Pressing> Pressings { get; set; }
        public DbSet<Clamping> Clampings { get; set; }
        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<Isolation> Isolations { get; set; }
        public DbSet<Packaging> Packagings { get; set; }
        public DbSet<CoverStock> CoverStocks { get; set; }
        public DbSet<SideProfileStock> SideProfileStocks { get; set; }
        public DbSet<SideProfileRemnant> SideProfileRemnants { get; set; }
        public DbSet<IsolationStock> IsolationStocks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<EventFeed> EventFeeds { get; set; }
        public DbSet<CuttingRequest> CuttingRequests { get; set; }
        public DbSet<PressingRequest> PressingRequests { get; set; }
        public DbSet<ClampingRequest> ClampingRequests { get; set; }
        public DbSet<AssemblyRequest> AssemblyRequests { get; set; }
        public DbSet<PackagingRequest> PackagingRequests { get; set; }
        public DbSet<Clamping2Request> Clamping2Requests { get; set; }
        public DbSet<Clamping2RequestItem> Clamping2RequestItems { get; set; }

        public ErpDbContext() : base()
        {
        }

        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(DatabaseHelper.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Companies
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.TaxNumber).HasMaxLength(50);
            });

            // Configure Suppliers
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.TaxNumber).HasMaxLength(50);
            });

            // Configure SerialNos
            modelBuilder.Entity<SerialNo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure Machines
            modelBuilder.Entity<Machine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure Employees
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
            });

            // Configure Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerOrderNo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TrexOrderNo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DeviceName).HasMaxLength(200);
                entity.Property(e => e.ProductCode).HasMaxLength(200);
                entity.Property(e => e.BypassSize).HasMaxLength(100);
                entity.Property(e => e.BypassType).HasMaxLength(100);
                entity.Property(e => e.LamelThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.ProductType).HasMaxLength(50);
                entity.Property(e => e.SalesPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrencyRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Yeni");
                entity.Property(e => e.IsStockOrder).HasDefaultValue(false);
                
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MaterialEntries
            modelBuilder.Entity<MaterialEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaterialType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaterialSize).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Thickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.InvoiceNo).HasMaxLength(100);
                entity.Property(e => e.TrexPurchaseNo).HasMaxLength(100);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
                
                entity.HasOne(e => e.Supplier)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MaterialExits
            modelBuilder.Entity<MaterialExit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaterialType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MaterialSize).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Thickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.TrexInvoiceNo).HasMaxLength(100);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
                
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Cuttings
            modelBuilder.Entity<Cutting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalKg).HasColumnType("decimal(18,3)");
                entity.Property(e => e.CutKg).HasColumnType("decimal(18,3)");
                entity.Property(e => e.WasteKg).HasColumnType("decimal(18,3)").HasDefaultValue(0);
                entity.Property(e => e.RemainingKg).HasColumnType("decimal(18,3)");
                entity.Property(e => e.CuttingCount).HasDefaultValue(0);
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Pressings
            modelBuilder.Entity<Pressing>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PressNo).HasMaxLength(50);
                entity.Property(e => e.Pressure).HasColumnType("decimal(18,3)");
                entity.Property(e => e.WasteAmount).HasColumnType("decimal(18,3)").HasDefaultValue(0);
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Cutting)
                    .WithMany()
                    .HasForeignKey(e => e.CuttingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Clampings
            modelBuilder.Entity<Clamping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Pressing)
                    .WithMany()
                    .HasForeignKey(e => e.PressingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Assemblies
            modelBuilder.Entity<Assembly>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Clamping)
                    .WithMany()
                    .HasForeignKey(e => e.ClampingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Isolations
            modelBuilder.Entity<Isolation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                entity.Property(e => e.IsolationLiquidAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.IsolationMethod).HasMaxLength(50).HasDefaultValue("Ýzosiyanat+Poliol");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Assembly)
                    .WithMany()
                    .HasForeignKey(e => e.AssemblyId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Packagings
            modelBuilder.Entity<Packaging>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Assembly)
                    .WithMany()
                    .HasForeignKey(e => e.AssemblyId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Isolation)
                    .WithMany()
                    .HasForeignKey(e => e.IsolationId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CoverStocks
            modelBuilder.Entity<CoverStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProfileType).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => new { e.ProfileType, e.Size, e.CoverLength })
                    .IsUnique()
                    .HasFilter("IsActive = 1");
            });

            // Configure SideProfileStocks
            modelBuilder.Entity<SideProfileStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProfileType).HasMaxLength(50).HasDefaultValue("Standart");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                entity.Property(e => e.UsedLength).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.WastedLength).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.RemainingLength).HasColumnType("decimal(10,2)");
            });

            // Configure SideProfileRemnants
            modelBuilder.Entity<SideProfileRemnant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProfileType).HasMaxLength(50).HasDefaultValue("Standart");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                entity.Property(e => e.IsWaste).HasDefaultValue(false);
            });

            // Configure IsolationStocks
            modelBuilder.Entity<IsolationStock>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LiquidType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Kilogram).HasColumnType("decimal(10,3)").HasDefaultValue(0);
                entity.Property(e => e.Quantity).HasDefaultValue(0);
                entity.Property(e => e.Liter).HasColumnType("decimal(10,2)").HasDefaultValue(0);
            });

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

            // Configure EventFeeds
            modelBuilder.Entity<EventFeed>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RequiredPermission).HasMaxLength(100);
                entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CuttingRequests
            modelBuilder.Entity<CuttingRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.OnePlateWeight).HasColumnType("decimal(18,3)");
                entity.Property(e => e.TotalRequiredPlateWeight).HasColumnType("decimal(18,3)");
                entity.Property(e => e.RemainingKg).HasColumnType("decimal(18,3)");
                entity.Property(e => e.IsRollFinished).HasDefaultValue(false);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Beklemede");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PressingRequests
            modelBuilder.Entity<PressingRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.PressNo).HasMaxLength(50);
                entity.Property(e => e.Pressure).HasColumnType("decimal(10,2)");
                entity.Property(e => e.WasteAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Beklemede");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Cutting)
                    .WithMany()
                    .HasForeignKey(e => e.CuttingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ClampingRequests
            modelBuilder.Entity<ClampingRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Beklemede");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Pressing)
                    .WithMany()
                    .HasForeignKey(e => e.PressingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure AssemblyRequests
            modelBuilder.Entity<AssemblyRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Beklemede");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Clamping)
                    .WithMany()
                    .HasForeignKey(e => e.ClampingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PackagingRequests
            modelBuilder.Entity<PackagingRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Size).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Length).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Beklemede");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Isolation)
                    .WithMany()
                    .HasForeignKey(e => e.IsolationId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SerialNo)
                    .WithMany()
                    .HasForeignKey(e => e.SerialNoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Clamping2Requests
            modelBuilder.Entity<Clamping2Request>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hatve).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PlateThickness).HasColumnType("decimal(10,3)");
                entity.Property(e => e.ResultedSize).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ResultedLength).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Beklemede");
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.FirstClamping)
                    .WithMany()
                    .HasForeignKey(e => e.FirstClampingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.SecondClamping)
                    .WithMany()
                    .HasForeignKey(e => e.SecondClampingId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Clamping2RequestItems
            modelBuilder.Entity<Clamping2RequestItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Clamping2Request)
                    .WithMany()
                    .HasForeignKey(e => e.Clamping2RequestId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Clamping)
                    .WithMany()
                    .HasForeignKey(e => e.ClampingId)
                    .OnDelete(DeleteBehavior.Restrict);
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
