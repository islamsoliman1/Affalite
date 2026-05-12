namespace AffaliteBL.DTOs.AiDTOs;

public class AiContentResponse
{
    public string Content { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
}
