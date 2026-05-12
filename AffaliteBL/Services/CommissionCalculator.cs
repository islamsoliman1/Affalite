using AffaliteDAL.Entities;

namespace AffaliteBL.Services;

public interface ICommissionCalculator
{
    Commission Calculate(Order order, Cart cart);
}

public class CommissionCalculator : ICommissionCalculator
{
    private readonly decimal _shippingFee;
    private readonly decimal _defaultPlatformFeePct;

    public CommissionCalculator(decimal shippingFee = 10m, decimal defaultPlatformFeePct = 5m)
    {
        _shippingFee = shippingFee;
        _defaultPlatformFeePct = defaultPlatformFeePct;
    }

    public Commission Calculate(Order order, Cart cart)
    {
        decimal itemsTotal = cart.Items.Sum(i => (i.Product?.Price ?? 0m) * i.Quantity);
        decimal platformAmount = itemsTotal * (_defaultPlatformFeePct / 100m);
        decimal affiliateAmount = itemsTotal * (order.AffiliateCommissionPct / 100m);
        decimal merchantAmount = itemsTotal - platformAmount - affiliateAmount;

        var commission = new Commission
        {
            OrderId = order.Id,
            PlatformAmount = platformAmount,
            AffiliateAmount = affiliateAmount,
            MerchantAmount = merchantAmount,
            Status = AffaliteDAL.Entities.Enums.CommissionStatus.Pending
        };

        // Add per-merchant commission breakdown
        var merchantGroups = cart.Items
            .GroupBy(i => i.Product?.MerchantId ?? 0)
            .Where(g => g.Key > 0);

        foreach (var group in merchantGroups)
        {
            var merchantItemTotal = group.Sum(i => (i.Product?.Price ?? 0m) * i.Quantity);
            var merchantPlatformDeduction = merchantItemTotal * (_defaultPlatformFeePct / 100m);
            var merchantValue = merchantItemTotal - merchantPlatformDeduction;

            commission.AddMerchantCommission(group.Key, merchantValue);
        }

        return commission;
    }
}
