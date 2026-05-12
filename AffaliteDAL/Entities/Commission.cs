using AffaliteDAL.Entities.Enums;

namespace AffaliteDAL.Entities;

public class Commission
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal AffiliateAmount { get; set; }
    public decimal PlatformAmount { get; set; }
    public decimal MerchantAmount { get; set; }
    public CommissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
    public List<MerchantCommissions> MerchantCommissions { get; set; } = new List<MerchantCommissions>();

    public void MarkAsPaid()
    {
        Status = CommissionStatus.Paid;
    }

    public void MarkAsFailed()
    {
        Status = CommissionStatus.Failed;
    }

    public void AddMerchantCommission(int merchantId, decimal value)
    {
        MerchantCommissions.Add(new MerchantCommissions
        {
            MerchantId = merchantId,
            CommissionId = Id,
            value = value
        });
    }
}
