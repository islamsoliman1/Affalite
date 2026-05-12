namespace AffaliteBL.DTOs.AiDTOs;

public class AiContentHistoryDTO
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string GeneratedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
