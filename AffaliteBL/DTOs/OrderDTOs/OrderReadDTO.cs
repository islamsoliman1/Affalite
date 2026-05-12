namespace AffaliteBL.DTOs.OrderDTOs;

public class OrderReadDTO
{
    public int Id { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AffiliateCommissionPct { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AffiliateName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;

    public List<OrderItemDTO> Items { get; set; } = new();
}
