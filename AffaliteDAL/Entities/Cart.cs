namespace AffaliteDAL.Entities;

public class Cart
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    public int AffiliateId { get; set; }
    public Affiliate Affiliate { get; set; } = null!;

    // Computed properties - not stored in DB
    public decimal SubTotal => Items.Sum(i => i.Quantity * i.Product?.Price ?? 0m);
    public decimal Shipping => 10m;
    public decimal AffiliateCommission => SubTotal * (AffiliateCommissionPct / 100m);
    public decimal Total => SubTotal + Shipping + AffiliateCommission;

    // This property is used for per-cart affiliate commission override
    public decimal AffiliateCommissionPct { get; set; } = 0m;

    public void AddItem(int productId, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.Quantity = quantity;
        }
        else
        {
            Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                CartId = Id
            });
        }
    }

    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void Clear()
    {
        Items.Clear();
    }
}
