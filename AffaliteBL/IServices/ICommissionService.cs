using AffaliteBL.DTOs.CommissionDTOs;
using AffaliteDAL.Entities.Enums;

namespace AffaliteBL.IServices;

public interface ICommissionService
{
    IEnumerable<CommissionReadDTO> GetAll();
    CommissionReadDTO? GetCommissionById(int id);
    CommissionReadDTO? GetCommissionByOrderId(int orderId);
    IEnumerable<CommissionReadDTO> GetCommissionsByAffiliate(int affiliateId);
    IEnumerable<CommissionReadDTO> GetCommissionsByMerchant(int merchantId);
    void UpdateCommissionStatus(int id, CommissionStatus status);
    void CalculateAndSaveCommission(int orderId, decimal totalPrice, decimal pct);
}
