using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Users & Security
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Catalog
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }

        // Inventory & Supply Chain
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }

        // Procurement
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IGNORE the problematic navigation properties
            modelBuilder.Entity<Category>()
                //.Ignore(c => c.UpdatedByUser)
                //.Ignore(c => c.UpdatedByUser)
                ;

            modelBuilder.Entity<Product>()
                //.Ignore(p => p.UpdatedByUser)
                //.Ignore(p => p.UpdatedByUser)
                ;

            // User - unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Category - unique code
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Category self-reference for hierarchy
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product configuration with precision
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();
                entity.HasIndex(p => p.Slug).IsUnique().HasFilter("[Slug] IS NOT NULL");

                // Add precision for decimal properties
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);
                entity.Property(p => p.SellingPrice).HasPrecision(18, 2);
                entity.Property(p => p.DiscountPrice).HasPrecision(18, 2);
                entity.Property(p => p.TaxRate).HasPrecision(18, 2);
                entity.Property(p => p.Weight).HasPrecision(18, 2);
                entity.Property(p => p.Length).HasPrecision(18, 2);
                entity.Property(p => p.Width).HasPrecision(18, 2);
                entity.Property(p => p.Height).HasPrecision(18, 2);

                // Relationships
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Brand)
                    .WithMany(b => b.Products)
                    .HasForeignKey(p => p.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ProductVariant
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasIndex(pv => pv.SKU).IsUnique();
                entity.Property(pv => pv.AdditionalPrice).HasPrecision(18, 2);
            });

            // PurchaseOrder
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.Property(po => po.TotalAmount).HasPrecision(18, 2);
            });

            // PurchaseOrderItem
            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.Property(poi => poi.UnitPrice).HasPrecision(18, 2);
                entity.Property(poi => poi.TotalPrice).HasPrecision(18, 2);
            });

            // UserRole composite key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // RolePermission composite key
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // PriceHistory precision
            modelBuilder.Entity<PriceHistory>()
                .Property(ph => ph.OldPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PriceHistory>()
                .Property(ph => ph.NewPrice)
                .HasPrecision(18, 2);

            // Global query filter for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Brand>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Warehouse>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}