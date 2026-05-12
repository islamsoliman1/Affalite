using AffaliteDAL.Data.Configurations;
using AffaliteDAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AffaliteDAL.Data;

public class AffaliteDBContext : IdentityDbContext<AppUser>
{
    public AffaliteDBContext(DbContextOptions<AffaliteDBContext> options) : base(options)
    {
    }

    public DbSet<WithdrawRequest> WithdrawRequests { get; set; }
    public DbSet<Affiliate> Affiliates { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Commission> Commissions { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<ProductReviews> ProductReviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AiContentHistory> AiContentHistories { get; set; }
    public DbSet<AffiliateMerchantMatch> AffiliateMerchantMatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureDecimalPrecision(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ConfigureIndexes(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        DbSeeder.Seed(modelBuilder);
    }

    private static void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .Property(p => p.PlatformCommissionPct)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Merchant>()
            .Property(m => m.Balance)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Affiliate>()
            .Property(a => a.Balance)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.AffiliateCommissionPct)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Commission>()
            .Property(c => c.AffiliateAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Commission>()
            .Property(c => c.PlatformAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Commission>()
            .Property(c => c.MerchantAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Coupon>()
            .Property(c => c.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Coupon>()
            .Property(c => c.DiscountPercentage)
            .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<MerchantCommissions>()
            .Property(mc => mc.value)
            .HasColumnType("decimal(18,2)");
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Commission)
            .WithOne(c => c.Order)
            .HasForeignKey<Commission>(c => c.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Affiliate)
            .WithMany(a => a.Orders)
            .HasForeignKey(o => o.AffiliateId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MerchantOrder>()
            .HasKey(mo => new { mo.MerchantId, mo.OrderId });

        modelBuilder.Entity<MerchantOrder>()
            .HasOne(mo => mo.Merchant)
            .WithMany(m => m.MerchantOrder)
            .HasForeignKey(mo => mo.MerchantId);

        modelBuilder.Entity<MerchantOrder>()
            .HasOne(mo => mo.Order)
            .WithMany(o => o.MerchantOrder)
            .HasForeignKey(mo => mo.OrderId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Merchant)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.MerchantId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AppUser>()
            .OwnsMany(u => u.RefreshTokens, b =>
            {
                b.WithOwner().HasForeignKey("AppUserId");
                b.Property<int>("Id");
                b.HasKey("Id");
                b.Property(p => p.Token).IsRequired();
            });

        modelBuilder.Entity<Merchant>()
            .HasOne(m => m.AppUser)
            .WithOne(u => u.MerchantProfile)
            .HasForeignKey<Merchant>(m => m.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Affiliate>()
            .HasOne(a => a.AppUser)
            .WithOne(u => u.AffiliateProfile)
            .HasForeignKey<Affiliate>(a => a.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AiContentHistory>(entity =>
        {
            entity.HasIndex(e => e.AffiliateId);
            entity.HasIndex(e => e.ProductId);
            entity.HasOne(e => e.Affiliate)
                  .WithMany()
                  .HasForeignKey(e => e.AffiliateId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AffiliateMerchantMatch>(entity =>
        {
            entity.HasIndex(e => new { e.Status, e.CreatedAt });
            entity.HasOne(e => e.Merchant)
                  .WithMany()
                  .HasForeignKey(e => e.MerchantId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Affiliate)
                  .WithMany()
                  .HasForeignKey(e => e.AffiliateId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Additional composite or specific indexes can be defined here
    }
}
