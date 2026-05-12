using System.Text.Json.Serialization;

namespace AffaliteDAL.Entities;

public class MerchantOrder
{
    public int MerchantId { get; set; }
    public int OrderId { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }

    public Merchant? Merchant { get; set; }
}
