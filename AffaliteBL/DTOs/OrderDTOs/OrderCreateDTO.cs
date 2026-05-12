using System.ComponentModel.DataAnnotations;

namespace AffaliteBL.DTOs.OrderDTOs;

public class OrderCreateDTO
{
    [Required]
    public int AffiliateId { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CustomerAddress { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal AffiliateCommissionPct { get; set; }
}
