using AffaliteBL.IServices;
using AffaliteDAL.Data;
using AffaliteDAL.Entities;
using AffaliteDAL.IRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AffaliteBL.BackgroundJobs
{
    public class MatchingBackgroundJob : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<MatchingBackgroundJob> _logger;
        private readonly TimeSpan _runInterval;

        public MatchingBackgroundJob(
            IServiceProvider services,
            IConfiguration config,
            ILogger<MatchingBackgroundJob> logger)
        {
            _services = services;
            _config = config;
            _logger = logger;

            // قراءة الفترة من الإعدادات (بالدقائق) - الافتراضي 360 دقيقة = 6 ساعات
            var intervalMinutes = int.Parse(config["MatchingSettings:RunIntervalMinutes"] ?? "360");
            _runInterval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🤖 Matching Background Job started with interval: {Interval}", _runInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🔄 Starting matching job at {UtcNow}", DateTime.UtcNow);

                    using var scope = _services.CreateScope();
                    var matchingService = scope.ServiceProvider.GetRequiredService<IMatchingService>();
                    var affiliateRepo = scope.ServiceProvider.GetRequiredService<IAffiliateRepo>();
                    var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

                    // جلب كل الـ Affiliates النشطين
                    var affiliates = affiliateRepo.GetAll()
                        .Where(a => a.AppUser != null) // تأكد إن اليوزر موجود
                        .ToList();

                    _logger.LogInformation("📊 Processing {Count} affiliates", affiliates.Count());

                    int matchesCreated = 0;

                    foreach (var affiliate in affiliates)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        try
                        {
                            // جلب التوصيات (الحد الأقصى 5 عشان متغرقش في الـ API Calls)
                            var recommendations = await matchingService.GetRecommendationsForAffiliateAsync(affiliate.Id, 5);

                            foreach (var rec in recommendations)
                            {
                                if (stoppingToken.IsCancellationRequested) break;

                                // نتأكد إن مفيش مطابقة مكررة أو منتهية
                                var exists = await scope.ServiceProvider
                                    .GetRequiredService<AffaliteDBContext>()
                                    .AffiliateMerchantMatches
                                    .AnyAsync(m =>
                                        m.AffiliateId == affiliate.Id &&
                                        m.MerchantId == rec.TargetId &&
                                        m.ProductId == rec.ProductId &&
                                        (m.Status == "Pending" || m.Status == "Accepted"),
                                        stoppingToken);

                                if (!exists && rec.ProductId > 0)
                                {
                                    var newMatch = new AffiliateMerchantMatch
                                    {
                                        AffiliateId = affiliate.Id,
                                        MerchantId = rec.TargetId,
                                        ProductId = rec.ProductId,
                                        MatchScore = rec.MatchScore,
                                        MatchReason = rec.MatchReason,
                                        Status = "Pending",
                                        CreatedAt = DateTime.UtcNow,
                                        ExpiredAt = DateTime.UtcNow.AddDays(7) // تنتهي بعد أسبوع لو مفيش رد
                                    };

                                    await scope.ServiceProvider
                                        .GetRequiredService<IMatchingRepo>()
                                        .AddAsync(newMatch);

                                    matchesCreated++;
                                    _logger.LogDebug("✅ Created match: Affiliate {AffiliateId} ↔ Merchant {MerchantId}",
                                        affiliate.Id, rec.TargetId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error processing affiliate {AffiliateId}", affiliate.Id);
                            // نكمل مع الباقي من غير ما نوقف الـ Job كله
                        }
                    }

                    // حفظ كل التغييرات دفعة واحدة
                    await scope.ServiceProvider
                        .GetRequiredService<IMatchingRepo>()
                        .SaveChangesAsync();

                    _logger.LogInformation("✅ Matching job completed: {Count} new matches created", matchesCreated);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "💥 Critical error in matching job at {UtcNow}", DateTime.UtcNow);
                }

                // انتظار الفترة المحددة قبل التشغيل الجاي
                _logger.LogInformation("😴 Waiting {Interval} before next run", _runInterval);
                await Task.Delay(_runInterval, stoppingToken);
            }

            _logger.LogInformation("🛑 Matching Background Job stopped");
        }
    }
}