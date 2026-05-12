using AffaliteDAL.Entities.Enums;
using System.Text.Json.Serialization;

namespace AffaliteDAL.Entities;

public class Order
{
    public int Id { get; set; }
    public int AffiliateId { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }
    public decimal AffiliateCommissionPct { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<MerchantOrder> MerchantOrder { get; set; } = new List<MerchantOrder>();

    [JsonIgnore]
    public Affiliate? Affiliate { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Commission? Commission { get; set; }

    // Domain behavior methods
    public void AddItem(int productId, int quantity, decimal price)
    {
        Items.Add(new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            Price = price
        });
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        var itemsTotal = Items.Sum(i => i.Price * i.Quantity);
        // Shipping = 10, AffiliateCommission = calculated from pct
        var affiliateCommission = itemsTotal * (AffiliateCommissionPct / 100m);
        TotalPrice = itemsTotal + affiliateCommission + 10m;
    }

    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
            throw new InvalidOperationException("Order is already paid.");

        Status = OrderStatus.Paid;
        Commission?.MarkAsPaid();

        // Update affiliate balance
        if (Affiliate != null && Commission != null)
        {
            Affiliate.Balance += Commission.AffiliateAmount;
        }

        // Update merchant balances
        if (Commission?.MerchantCommissions != null)
        {
            foreach (var mc in Commission.MerchantCommissions)
            {
                if (mc.Commission?.Order?.MerchantOrder != null)
                {
                    var merchant = mc.Commission.Order.MerchantOrder
                        .FirstOrDefault(mo => mo.MerchantId == mc.MerchantId)
                        ?.Merchant;
                    if (merchant != null)
                    {
                        merchant.Balance += mc.value;
                    }
                }
            }
        }
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        Status = OrderStatus.Cancelled;
        if (Commission != null)
        {
            Commission.MarkAsFailed();
        }
    }

    public void InitializeCommission(decimal platformAmount, decimal affiliateAmount, decimal merchantAmount)
    {
        Commission = new Commission
        {
            OrderId = Id,
            PlatformAmount = platformAmount,
            AffiliateAmount = affiliateAmount,
            MerchantAmount = merchantAmount,
            Status = CommissionStatus.Pending
        };
    }
}
