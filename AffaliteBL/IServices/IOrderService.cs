using AffaliteBL.DTOs.OrderDTOs;
using AffaliteDAL.Entities;
using AffaliteDAL.Entities.Enums;

namespace AffaliteBL.IServices;

public interface IOrderService
{
    Task<OrderReadDTO> CreateOrderAsync(OrderCreateDTO orderDto, CancellationToken cancellationToken = default);
    Task<OrderReadDTO?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReadDTO>> GetOrdersByAffiliateAsync(int affId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReadDTO>> GetOrdersByMerchantAsync(int merId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderReadDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
