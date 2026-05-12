namespace AffaliteBL.DTOs.OrderDTOs;

public class OrderItemDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public List<string> Images { get; set; } = new List<string>();
    public decimal TotalPrice { get; set; }
}
