using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AffaliteBL.DTOs.AiDTOs;
using AffaliteBL.IServices;
using AffaliteDAL.Entities;
using AffaliteDAL.IRepo;
using Microsoft.Extensions.Configuration;
using Polly;

namespace AffaliteBL.Services;

public class AiContentService : IAiContentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAiContentRepo _contentRepo;
    private readonly IProductRepository _productRepo;
    private readonly IConfiguration _config;

    public AiContentService(
        IHttpClientFactory httpClientFactory,
        IAiContentRepo contentRepo,
        IProductRepository productRepo,
        IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _contentRepo = contentRepo;
        _productRepo = productRepo;
        _config = config;
    }

    public async Task<AiContentResponse> GenerateContentAsync(ContentGenerationRequest request)
    {
        var product = await _productRepo.GetByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException("Product not found");

        var prompt = BuildPrompt(product, request);

        var client = _httpClientFactory.CreateClient();

        var baseUrl = _config["AiSettings:BaseUrl"] ?? "https://openrouter.ai/api/v1/";
        var apiKey = _config["AiSettings:OpenAiApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("AI API key is not configured.");

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        if (baseUrl.Contains("openrouter", StringComparison.OrdinalIgnoreCase))
        {
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:4300");
            client.DefaultRequestHeaders.Add("X-Title", "Affalite Platform");
        }

        var policy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));

        var response = await policy.ExecuteAsync(async () =>
        {
            var payload = new
            {
                model = _config["AiSettings:ContentModel"],
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = int.Parse(_config["AiSettings:MaxTokens"] ?? "500"),
                temperature = double.Parse(_config["AiSettings:Temperature"] ?? "0.7")
            };

            var result = await client.PostAsync("chat/completions",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            if (!result.IsSuccessStatusCode)
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                throw new HttpRequestException($"AI API Error ({(int)result.StatusCode}): {errorContent}");
            }
            return await result.Content.ReadFromJsonAsync<OpenAiResponse>();
        });

        var content = response?.Choices?.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("Failed to generate content: empty response from AI API");

        var tokensUsed = response.Usage?.TotalTokens ?? 0;
        var costPerToken = _config.GetValue<decimal>("AiSettings:CostPerToken", 0.0000003m);

        var history = new AiContentHistory
        {
            AffiliateId = request.AffiliateId,
            ProductId = request.ProductId,
            PromptText = prompt,
            GeneratedContent = content,
            ContentType = request.ContentType,
            Platform = request.Platform,
            Language = request.Language,
            Tone = request.Tone,
            TokensUsed = tokensUsed,
            Cost = tokensUsed * costPerToken,
            Status = "Generated",
            CreatedAt = DateTime.UtcNow
        };

        await _contentRepo.AddAsync(history);
        await _contentRepo.SaveChangesAsync();

        return new AiContentResponse
        {
            Content = content,
            TokensUsed = tokensUsed,
            EstimatedCost = tokensUsed * costPerToken
        };
    }

    private string BuildPrompt(Product product, ContentGenerationRequest req)
    {
        var lang = req.Language == "ar" ? "اكتب بالعربية المبسطة" : "Write in clear English";
        var platform = req.Platform switch
        {
            "tiktok" => "اكتب سكريبت فيديو (15-30 ثانية)",
            "instagram" => "اكتب caption مع 5 هاشتاجات",
            _ => "اكتب بوست سوشيال ميديا"
        };

        return $"""
            أنت خبير تسويق. {lang}. {platform}.
            المنتج: {product.Name} | السعر: {product.Price:C} | الميزات: {product.Description}
            النبرة: {req.Tone}
            الشروط: لا تبالغ، أضف CTA، اترك [رابط التتبع] فارغًا.
            {(!string.IsNullOrEmpty(req.CustomInstructions) ? $"ملاحظات: {req.CustomInstructions}" : "")}
            """;
    }

    public async Task<IEnumerable<AiContentHistoryDTO>> GetAffiliateContentHistoryAsync(int affiliateId, int page = 1, int pageSize = 20)
    {
        var histories = _contentRepo.GetByAffiliateId(affiliateId, page, pageSize);
        return histories.Select(h => new AiContentHistoryDTO
        {
            Id = h.Id,
            ProductName = h.Product?.Name ?? "Unknown",
            ContentType = h.ContentType,
            GeneratedContent = h.GeneratedContent ?? string.Empty,
            CreatedAt = h.CreatedAt,
            Status = h.Status
        });
    }

    public async Task<bool> SaveContentAsync(int affiliateId, int productId, string content, string contentType)
    {
        var existing = _contentRepo.GetByAffiliateAndProduct(affiliateId, productId);
        if (existing == null) return false;
        existing.GeneratedContent = content;
        existing.ContentType = contentType;
        existing.Status = "Published";
        _contentRepo.Update(existing);
        return await _contentRepo.SaveChangesAsync() > 0;
    }

    private class OpenAiResponse
    {
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
        public Usage? Usage { get; set; }
    }
    private class Choice { public Message Message { get; set; } = new(); }
    private class Message { public string Content { get; set; } = string.Empty; }
    private class Usage { public int TotalTokens { get; set; } }
}
