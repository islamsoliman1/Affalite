namespace AffaliteBL.DTOs.MatchingDTOs;

public class MatchRecommendationDTO
{
    public int TargetId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal MatchScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public decimal ExpectedCommission { get; set; }
    public DateTime CreatedAt { get; set; }
}
