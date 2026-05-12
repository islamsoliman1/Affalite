using AffaliteBL.DTOs.CommissionDTOs;
using AffaliteBL.IServices;
using AffaliteDAL.Entities;
using AffaliteDAL.Entities.Enums;
using AffaliteDAL.IRepo;
using AutoMapper;

namespace AffaliteBL.Services;

public class CommissionService : ICommissionService
{
    private readonly IGenericRepository<Commission> _commissionRepo;
    private readonly IMapper _mapper;
    private readonly ICommissionRepo _commRepo;

    public CommissionService(IGenericRepository<Commission> commissionRepo, IMapper mapper, ICommissionRepo commRepo)
    {
        _commissionRepo = commissionRepo;
        _mapper = mapper;
        _commRepo = commRepo;
    }

    public IEnumerable<CommissionReadDTO> GetAll()
    {
        var commissions = _commissionRepo.GetAll();
        return _mapper.Map<IEnumerable<CommissionReadDTO>>(commissions);
    }

    public CommissionReadDTO? GetCommissionById(int id)
    {
        var commission = _commissionRepo.GetById(id);
        return commission == null ? null : _mapper.Map<CommissionReadDTO>(commission);
    }

    public CommissionReadDTO? GetCommissionByOrderId(int orderId)
    {
        var commission = _commissionRepo.GetAll().FirstOrDefault(c => c.OrderId == orderId);
        return _mapper.Map<CommissionReadDTO>(commission);
    }

    public IEnumerable<CommissionReadDTO> GetCommissionsByAffiliate(int affiliateId)
    {
        var commissions = _commissionRepo.GetAll()
            .Where(c => c.Order != null && c.Order.AffiliateId == affiliateId);

        return _mapper.Map<IEnumerable<CommissionReadDTO>>(commissions);
    }

    public IEnumerable<CommissionReadDTO> GetCommissionsByMerchant(int merchantId)
    {
        var commissions = _commRepo.GetAllCommissionsByMerchant(merchantId);
        return commissions.Select(c => new CommissionReadDTO
        {
            Id = c.Id,
            OrderId = c.OrderId,
            AffiliateAmount = c.AffiliateAmount,
            PlatformAmount = c.PlatformAmount,
            MerchantAmount = c.MerchantAmount,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt
        });
    }

    public void UpdateCommissionStatus(int id, CommissionStatus status)
    {
        var commission = _commissionRepo.GetById(id);
        if (commission != null)
        {
            commission.Status = status;
            _commissionRepo.Update(commission);
            _commissionRepo.SaveChanges();
        }
    }

    public void CalculateAndSaveCommission(int orderId, decimal totalPrice, decimal pct)
    {
        var commission = new Commission
        {
            OrderId = orderId,
            AffiliateAmount = totalPrice * (pct / 100),
            PlatformAmount = totalPrice * 0.05m,
            CreatedAt = DateTime.UtcNow,
            Status = CommissionStatus.Pending
        };

        commission.MerchantAmount = totalPrice - (commission.AffiliateAmount + commission.PlatformAmount);

        _commissionRepo.Add(commission);
        _commissionRepo.SaveChanges();
    }
}
