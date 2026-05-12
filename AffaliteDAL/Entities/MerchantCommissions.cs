using System.Text.Json.Serialization;

namespace AffaliteDAL.Entities;

public class MerchantCommissions
{
    public int Id { get; set; }
    public int MerchantId { get; set; }
    public int CommissionId { get; set; }
    public decimal value { get; set; }

    [JsonIgnore]
    public Commission? Commission { get; set; }
}
