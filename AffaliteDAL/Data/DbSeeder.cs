using AffaliteDAL.Entities;
using AffaliteDAL.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AffaliteDAL.Data;

public static class DbSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedUsers(modelBuilder);
        SeedUserRoles(modelBuilder);
        SeedCategories(modelBuilder);
        SeedMerchants(modelBuilder);
        SeedAffiliates(modelBuilder);
        SeedProducts(modelBuilder);
        SeedProductImages(modelBuilder);
        SeedProductReviews(modelBuilder);
        SeedCarts(modelBuilder);
        SeedCartItems(modelBuilder);
        SeedOrders(modelBuilder);
        SeedMerchantOrders(modelBuilder);
        SeedOrderItems(modelBuilder);
        SeedCommissions(modelBuilder);
        SeedMerchantCommissions(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "role-admin", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "role-merchant", Name = "Merchant", NormalizedName = "MERCHANT" },
            new IdentityRole { Id = "role-affiliate", Name = "Affiliate", NormalizedName = "AFFILIATE" },
            new IdentityRole { Id = "role-customer", Name = "Customer", NormalizedName = "CUSTOMER" }
        );
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<AppUser>();
        modelBuilder.Entity<AppUser>().HasData(
            new AppUser { Id = "user1", UserName = "merchant1", Email = "merchant1@affalite.com", NormalizedUserName = "MERCHANT1@AFFALITE.COM", NormalizedEmail = "MERCHANT1@AFFALITE.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(null!, "Password@123"), FullName = "Ahmed Hassan", PhoneNumber = "01001234567" },
            new AppUser { Id = "user2", UserName = "affiliate1", Email = "affiliate1@affalite.com", NormalizedUserName = "AFFILIATE1@AFFALITE.COM", NormalizedEmail = "AFFILIATE1@AFFALITE.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(null!, "Password@123"), FullName = "Youssef Ali", PhoneNumber = "01001112233" },
            new AppUser { Id = "user3", UserName = "customer1", Email = "customer1@affalite.com", NormalizedUserName = "CUSTOMER1@AFFALITE.COM", NormalizedEmail = "CUSTOMER1@AFFALITE.COM", EmailConfirmed = true, PasswordHash = hasher.HashPassword(null!, "Password@123"), FullName = "Hana Adel", PhoneNumber = "01002223344" }
        );
    }

    private static void SeedUserRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { RoleId = "role-merchant", UserId = "user1" },
            new IdentityUserRole<string> { RoleId = "role-affiliate", UserId = "user2" },
            new IdentityUserRole<string> { RoleId = "role-customer", UserId = "user3" }
        );
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Slug = "electronics", CreatedAt = DateTime.UtcNow },
            new Category { Id = 2, Name = "Fashion", Slug = "fashion", CreatedAt = DateTime.UtcNow }
        );
    }

    private static void SeedMerchants(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Merchant>().HasData(
            new Merchant { Id = 1, AppUserId = "user1", Balance = 5000, CreatedAt = DateTime.UtcNow }
        );
    }

    private static void SeedAffiliates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Affiliate>().HasData(
            new Affiliate { Id = 1, AppUserId = "user2", Balance = 1500, CreatedAt = DateTime.UtcNow }
        );
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "iPhone 14",
                CategoryId = 1,
                Description = "Latest Apple iPhone",
                Details = "Details here",
                Price = 999,
                Stock = 50,
                SaleCount = 10,
                MerchantId = 1,
                PlatformCommissionPct = 5,
                Status = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Harry Potter Book",
                CategoryId = 2,
                Description = "Fantasy novel",
                Details = "Details here",
                Price = 20,
                Stock = 100,
                SaleCount = 50,
                MerchantId = 1,
                PlatformCommissionPct = 2,
                Status = ProductStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        );
    }

    private static void SeedProductImages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImage>().HasData(
            new ProductImage { Id = 1, ProductId = 1, ImageUrl = "p1.jpg", FileName = "iphone14.jpg" },
            new ProductImage { Id = 2, ProductId = 1, ImageUrl = "p4.jpg", FileName = "iphone14.jpg" },
            new ProductImage { Id = 3, ProductId = 2, ImageUrl = "p2.png", FileName = "harrypotter.jpg" },
            new ProductImage { Id = 4, ProductId = 2, ImageUrl = "p3.jpg", FileName = "harrypotter.jpg" }
        );
    }

    private static void SeedProductReviews(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductReviews>().HasData(
            new ProductReviews { Id = 1, ProductId = 1, AffiliateId = 1, Comment = "Great phone!", Rating = 5, CreatedAt = DateTime.UtcNow },
            new ProductReviews { Id = 2, ProductId = 2, AffiliateId = 1, Comment = "Loved the book", Rating = 4, CreatedAt = DateTime.UtcNow }
        );
    }

    private static void SeedCarts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>().HasData(
            new Cart { Id = 1, CreatedAt = DateTime.UtcNow, AffiliateId = 1 }
        );
    }

    private static void SeedCartItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>().HasData(
            new CartItem { Id = 1, CartId = 1, ProductId = 1, Quantity = 2, CreatedAt = DateTime.UtcNow },
            new CartItem { Id = 2, CartId = 1, ProductId = 2, Quantity = 1, CreatedAt = DateTime.UtcNow }
        );
    }

    private static void SeedOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                Id = 1,
                AffiliateId = 1,
                CustomerName = "David",
                CustomerPhone = "01000000004",
                CustomerAddress = "123 Street",
                TotalPrice = 2018,
                AffiliateCommissionPct = 5,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            }
        );
    }

    private static void SeedMerchantOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantOrder>().HasData(
            new MerchantOrder { MerchantId = 1, OrderId = 1 }
        );
    }

    private static void SeedOrderItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 999, CreatedAt = DateTime.UtcNow },
            new OrderItem { Id = 2, OrderId = 1, ProductId = 2, Quantity = 1, Price = 20, CreatedAt = DateTime.UtcNow }
        );
    }

    private static void SeedCommissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commission>().HasData(
            new Commission
            {
                Id = 1,
                OrderId = 1,
                AffiliateAmount = 578.99m,
                PlatformAmount = 964.99m,
                MerchantAmount = 17756.00m,
                Status = CommissionStatus.Paid,
                CreatedAt = DateTime.UtcNow.AddDays(-29)
            }
        );
    }

    private static void SeedMerchantCommissions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MerchantCommissions>().HasData(
            new MerchantCommissions
            {
                Id = 1,
                MerchantId = 1,
                CommissionId = 1,
                value = 20
            }
        );
    }
}
