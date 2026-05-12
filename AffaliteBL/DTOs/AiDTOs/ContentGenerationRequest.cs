namespace AffaliteBL.DTOs.AiDTOs;

public class ContentGenerationRequest
{
    public int ProductId { get; set; }
    public int AffiliateId { get; set; }
    public string ContentType { get; set; } = "social_post";
    public string Platform { get; set; } = "general";
    public string Language { get; set; } = "en";
    public string Tone { get; set; } = "professional";
    public string? CustomInstructions { get; set; }
}
