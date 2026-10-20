using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Models;

public partial class ClothingStoreDbContext : DbContext
{
    public ClothingStoreDbContext()
    {
    }

    public ClothingStoreDbContext(DbContextOptions<ClothingStoreDbContext> options)
        : base(options)
    {
    }

    // --- Danh sách bảng đầy đủ theo đúng Script SQL ---
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Material> Materials { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<TargetAudience> TargetAudiences { get; set; }
    public virtual DbSet<Size> Sizes { get; set; }
    public virtual DbSet<Color> Colors { get; set; }
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<Promotion> Promotions { get; set; }
    public virtual DbSet<Blog> Blogs { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductVariant> ProductVariants { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<ProductVariantImage> ProductVariantImages { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartDetail> CartDetails { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderDetail> OrderDetails { get; set; }
    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=MSI\\SQLEXPRESS05;Database=QLShopBanQuanAo;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Username);
            entity.Property(e => e.Username).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(256).IsUnicode(false);
        });

        // 2. Country
        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.CountryId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.CountryName).HasMaxLength(100);
        });

        // 3. Brand
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.Property(e => e.BrandId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.BrandCountryId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.BrandName).HasMaxLength(100);

            //entity.HasOne<Country>()
            //      .WithMany()
            //      .HasForeignKey(b => b.BrandCountryId) // Khớp chính xác với cột trong DB
            //      .HasPrincipalKey(c => c.CountryId)   // Trỏ tới khóa chính của bảng Country
            //      .OnDelete(DeleteBehavior.NoAction);
        });

        // 4. Material, Category, TargetAudience, Size, Color
        modelBuilder.Entity<Material>(entity =>
        {
            entity.Property(e => e.MaterialId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.MaterialName).HasMaxLength(150);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Thumbnail).HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<TargetAudience>(entity =>
        {
            entity.HasKey(e => e.AudienceId);
            entity.Property(e => e.AudienceId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.AudienceName).HasMaxLength(100);
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.Property(e => e.SizeId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.SizeName).HasMaxLength(150);
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.Property(e => e.ColorId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.ColorName).HasMaxLength(100);
        });

        // 5. Blog
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.Property(e => e.BlogId).UseIdentityColumn();
            entity.Property(e => e.CommentCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Thumbnail).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(250);
        });

        // 6. Customer & CustomerAddress
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(150);
            entity.Property(e => e.Avatar).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(100).IsUnicode(false);

            entity.HasOne(c => c.Account)
                  .WithMany()
                  .HasForeignKey(c => c.Username)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId);
            entity.Property(e => e.AddressId).UseIdentityColumn();
            entity.Property(e => e.CustomerId).HasMaxLength(25).IsUnicode(false);

            entity.HasOne(a => a.Customer)
                  .WithMany(c => c.Addresses)
                  .HasForeignKey(x => x.CustomerId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // 7. Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.EmployeeId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Address).HasMaxLength(150);
            entity.Property(e => e.Avatar).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber1).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.PhoneNumber2).HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(100).IsUnicode(false);

            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.Username)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // 8. Product, ProductImage, ProductVariant, ProductVariantImage
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.CategoryId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.MaterialId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.BrandId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.OriginCountryId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.AudienceId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountPercent).HasDefaultValue(0.0);
            entity.Property(e => e.FitType).HasMaxLength(55);
            entity.Property(e => e.MaxPrice).HasColumnType("money");
            entity.Property(e => e.MinPrice).HasColumnType("money");
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.Thumbnail).HasMaxLength(100).IsUnicode(false);

            entity.HasOne(p => p.Material)
                  .WithMany()
                  .HasForeignKey(p => p.MaterialId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Brand)
                  .WithMany()
                  .HasForeignKey(p => p.BrandId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.OriginCountry)
                  .WithMany()
                  .HasForeignKey(p => p.OriginCountryId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Category)
                  .WithMany()
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Audience)
                  .WithMany()
                  .HasForeignKey(p => p.AudienceId)
                  .OnDelete(DeleteBehavior.NoAction);

        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.ImageFileName });
            entity.Property(e => e.ProductId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.ImageFileName).HasMaxLength(100).IsUnicode(false);

            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId);
            entity.Property(e => e.VariantId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.ColorId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.ProductId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.SizeId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.Discount).HasDefaultValue(0.0);
            entity.Property(e => e.Thumbnail).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.VideoUrl).HasMaxLength(100).IsUnicode(false);

            entity.HasOne(p => p.Product)
                  .WithMany(p => p.Variants)
                  .HasForeignKey(p => p.ProductId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Size)
                  .WithMany()
                  .HasForeignKey(p => p.SizeId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Color)
                  .WithMany()
                  .HasForeignKey(p => p.ColorId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProductVariantImage>(entity =>
        {
            entity.HasKey(e => new { e.VariantId, e.ImageFileName });
            entity.Property(e => e.VariantId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.ImageFileName).HasMaxLength(100).IsUnicode(false);

            entity.HasOne<ProductVariant>()
                .WithMany()
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // 9. Cart & CartDetail
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId);
            entity.Property(e => e.CartId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");

            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.Username)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CartDetail>(entity =>
        {
            entity.HasKey(x => new { x.CartId, x.VariantId });
            entity.Property(e => e.CartId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.VariantId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart)
                  .WithMany(p => p.CartDetails)
                  .HasForeignKey(x => x.CartId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Variant) // Hoặc ProductVariant tùy theo tên property trong model CartDetail.cs của bạn
                  .WithMany()
                  .HasForeignKey(x => x.VariantId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // 10. Order & OrderDetail
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.CustomerId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.EmployeeId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(100);
            entity.Property(e => e.TaxCode).HasMaxLength(100);
            entity.Property(e => e.TaxInfo).HasMaxLength(250);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasColumnType("money");

            entity.HasOne(d => d.Customer)
                  .WithMany()
                  .HasForeignKey(d => d.CustomerId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.Employee)
                  .WithMany()
                  .HasForeignKey(d => d.EmployeeId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.VariantId });
            entity.Property(e => e.OrderId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.VariantId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.Discount).HasDefaultValue(0.0);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            entity.Property(e => e.Notes).HasMaxLength(100);

            entity.HasOne(d => d.Order)
                  .WithMany(p => p.OrderDetails)
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.Variant)
                  .WithMany()
                  .HasForeignKey(d => d.VariantId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // 11. PurchaseOrder, PurchaseOrderDetail, Review, Wishlist
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId);
            entity.Property(e => e.PurchaseOrderId).HasMaxLength(25).IsUnicode(false);
            entity.HasOne<Supplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Employee>().WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(x => new { x.PurchaseOrderId, x.VariantId });
            entity.HasOne<PurchaseOrder>().WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(e => e.ReviewId).UseIdentityColumn();

            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(x => x.ProductId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.Customer)
                  .WithMany()
                  .HasForeignKey(x => x.CustomerId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(x => new { x.CustomerId, x.ProductId });
            entity.Property(e => e.CustomerId).HasMaxLength(25).IsUnicode(false);
            entity.Property(e => e.ProductId).HasMaxLength(25).IsUnicode(false);

            entity.HasOne(w => w.Customer)
                  .WithMany()
                  .HasForeignKey(x => x.CustomerId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(w => w.Product)
                  .WithMany(p => p.Wishlists) // Liên kết chính xác vào collection Wishlists của Product
                  .HasForeignKey(x => x.ProductId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}