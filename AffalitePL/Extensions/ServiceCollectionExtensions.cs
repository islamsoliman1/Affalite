using AffaliteBL.IServices;
using AffaliteBL.Mapper;
using AffaliteBL.Mapping;
using AffaliteBL.Services;
using AffaliteBLL.Services;
using AffaliteBLL.Services.Interfaces;
using AffaliteDAL.Data;
using AffaliteDAL.Entities;
using AffaliteDAL.IRepo;
using AffaliteDAL.Repo;
using AffaliteBL.Helpers;
using AffalitePL.Options;
using AffalitePL.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace AffalitePL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AffaliteDBContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AffaliteDBContext>();

        services.Configure<JwtOptions>(configuration.GetSection("JWT"));
        services.Configure<DefaultAdminOptions>(configuration.GetSection("DefaultAdmin"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Generic repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auth
        services.AddScoped<IAuthServices, AuthServices>();
        services.AddScoped<IJwtServices, JwtServices>();

        // Product
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReviewRepo, ProductReviewRepo>();
        services.AddScoped<IProductReviewService, ProductReviewService>();

        // Order & Cart
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderRepo, OrderRepo>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICartRepo, CartRepo>();

        // Affiliate & Merchant
        services.AddScoped<IAffiliateRepo, AffiliateRepo>();
        services.AddScoped<IAffiliateService, AffiliateService>();
        services.AddScoped<IMerchantRepo, MerchantRepo>();
        services.AddScoped<IMerchantService, MerchantService>();

        // Category
        services.AddScoped<ICategoryRepo, CategoryRepo>();
        services.AddScoped<ICategoryService, CategoryService>();

        // Coupons
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponService, CouponService>();

        // Wishlist
        services.AddScoped<IWishlistRepo, WishlistRepo>();
        services.AddScoped<IWishlistService, WishlistService>();

        // Notifications
        services.AddScoped<INotificationRepo, NotificationRepo>();
        services.AddScoped<INotificationService, NotificationService>();

        // AI & Matching
        services.AddScoped<IAiContentRepo, AiContentRepo>();
        services.AddScoped<IMatchingRepo, MatchingRepo>();
        services.AddScoped<IAiContentService, AiContentService>();
        services.AddScoped<IMatchingService, MatchingService>();

        // Commissions & Withdrawals
        services.AddScoped<ICommissionRepo, CommissionRepo>();
        services.AddScoped<ICommissionService, CommissionService>();
        services.AddScoped<IWithdrawalService, WithdrawalService>();
        services.AddScoped<IWithdrawalRepo, WithdrawalRepo>();

        // Admin & Marketing
        services.AddScoped<IAdminDashboardRepo, AdminDashboardRepo>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IMarketingService, MarketingService>();

        // Commission Calculator
        services.AddScoped<ICommissionCalculator, CommissionCalculator>();

        // Email
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(MappingProfile),
            typeof(AffaliteBL.Mapper.AiMappingProfile));

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("JWT").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section is missing or invalid.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.MapInboundClaims = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "uid",
                RoleClaimType = "role",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "bearer"
                    }
                }] = Array.Empty<string>()
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:4200",
                        "http://localhost:55000",
                        "http://localhost:4300",
                        "https://affilliate-front.vercel.app")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
