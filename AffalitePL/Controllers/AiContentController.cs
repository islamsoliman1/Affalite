using AffaliteBL.DTOs.AiDTOs;
using AffaliteBL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AffalitePL.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AiContentController : ControllerBase
{
    private readonly IAiContentService _aiService;
    private readonly IAffiliateService _affiliateService;

    public AiContentController(IAiContentService aiService, IAffiliateService affiliateService)
    {
        _aiService = aiService;
        _affiliateService = affiliateService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateContent([FromBody] ContentGenerationRequest request)
    {
        var userId = User.FindFirst("uid")?.Value
           ?? User.FindFirst("sub")?.Value;
        var affiliate = _affiliateService.GetAffiliateUserId(userId);

        if (affiliate == null)
            return Unauthorized();

        request.AffiliateId = affiliate.Id;
        var result = await _aiService.GenerateContentAsync(request);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("uid")?.Value;
        var affiliate = _affiliateService.GetAffiliateUserId(userId);

        if (affiliate == null) return Unauthorized();

        var history = await _aiService.GetAffiliateContentHistoryAsync(affiliate.Id, page, pageSize);
        return Ok(history);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveContent([FromBody] SaveContentRequest request)
    {
        var userId = User.FindFirst("uid")?.Value;
        var affiliate = _affiliateService.GetAffiliateUserId(userId);

        if (affiliate?.Id != request.AffiliateId)
            return Forbid();

        var success = await _aiService.SaveContentAsync(
            request.AffiliateId,
            request.ProductId,
            request.Content,
            request.ContentType);

        return Ok(new { success });
    }
}

public class SaveContentRequest
{
    public int AffiliateId { get; set; }
    public int ProductId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentType { get; set; } = "social_post";
}
