using AffaliteBL.DTOs.NotificationDTOs;
using AffaliteBL.DTOs.OrderDTOs;
using AffaliteBL.IServices;
using AffaliteDAL.Entities;
using AffaliteDAL.Entities.Enums;
using AffaliteDAL.IRepo;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AffaliteBL.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepo _orderRepo;
    private readonly ICartRepo _cartRepo;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ICommissionCalculator _commissionCalculator;

    public OrderService(
        IUnitOfWork unitOfWork,
        IOrderRepo orderRepo,
        ICartRepo cartRepo,
        INotificationService notificationService,
        IEmailService emailService,
        IMapper mapper,
        ICommissionCalculator commissionCalculator)
    {
        _unitOfWork = unitOfWork;
        _orderRepo = orderRepo;
        _cartRepo = cartRepo;
        _notificationService = notificationService;
        _emailService = emailService;
        _mapper = mapper;
        _commissionCalculator = commissionCalculator;
    }

    public async Task<OrderReadDTO> CreateOrderAsync(OrderCreateDTO orderDto, CancellationToken cancellationToken = default)
    {
        var cart = _cartRepo.GetCartWithAffilaiteId(orderDto.AffiliateId);

        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Cart is empty");

        // Build order entity
        var order = new Order
        {
            AffiliateId = orderDto.AffiliateId,
            CustomerName = orderDto.CustomerName,
            CustomerPhone = orderDto.CustomerPhone,
            CustomerAddress = orderDto.CustomerAddress,
            AffiliateCommissionPct = orderDto.AffiliateCommissionPct,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Add items
        foreach (var item in cart.Items)
        {
            if (item.Product == null) continue;

            order.AddItem(item.ProductId, item.Quantity, item.Product.Price);

            // Update product stats
            item.Product.SaleCount += item.Quantity;
            item.Product.Stock -= item.Quantity;
        }

        order.RecalculateTotal();

        // Calculate commission
        var commission = _commissionCalculator.Calculate(order, cart);
        order.Commission = commission;

        // Build merchant orders
        var merchantIds = cart.Items
            .Select(i => i.Product?.MerchantId ?? 0)
            .Where(id => id > 0)
            .Distinct();

        foreach (var merchantId in merchantIds)
        {
            order.MerchantOrder.Add(new MerchantOrder
            {
                MerchantId = merchantId,
                OrderId = order.Id
            });
        }

        // Persist everything inside a transaction
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Repository<Order>().AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
            throw;
        }

        // Side effects: publish notifications OUTSIDE transaction
        await PublishOrderCreatedEventsAsync(order, cart);

        return _mapper.Map<OrderReadDTO>(order);
    }

    public async Task<OrderReadDTO?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        return order == null ? null : _mapper.Map<OrderReadDTO>(order);
    }

    public async Task<IReadOnlyList<OrderReadDTO>> GetOrdersByAffiliateAsync(int affId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepo.GetByAffIdAsync(affId, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderReadDTO>>(orders);
    }

    public async Task<IReadOnlyList<OrderReadDTO>> GetOrdersByMerchantAsync(int merId, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepo.GetByMerIdAsync(merId, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderReadDTO>>(orders);
    }

    public async Task<IReadOnlyList<OrderReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Repository<Order>()
            .GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderReadDTO>>(orders);
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<Order>()
            .GetByIdAsync(orderId, cancellationToken);

        if (order == null) return false;

        // Load related data for domain methods
        var orderWithDetails = await _unitOfWork.Repository<Order>()
            .GetAllQueryable()
            .Include(o => o.Commission)
                .ThenInclude(c => c!.MerchantCommissions)
            .Include(o => o.Affiliate)
            .Include(o => o.MerchantOrder)
                .ThenInclude(mo => mo.Merchant)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (orderWithDetails == null) return false;

        var oldStatus = orderWithDetails.Status;
        if (oldStatus == status) return true;

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            switch (status)
            {
                case OrderStatus.Paid:
                    orderWithDetails.MarkAsPaid();
                    break;
                case OrderStatus.Cancelled:
                    orderWithDetails.Cancel();
                    break;
                default:
                    orderWithDetails.Status = status;
                    break;
            }

            _unitOfWork.Repository<Order>().Update(orderWithDetails);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(transaction, cancellationToken);
            throw;
        }

        // Side effects outside transaction
        await PublishStatusChangedEventsAsync(orderWithDetails, oldStatus, status);

        return true;
    }

    private async Task PublishOrderCreatedEventsAsync(Order order, Cart cart)
    {
        var affiliate = order.Affiliate;
        if (affiliate != null)
        {
            _notificationService.CreateNotification(new CreateNotificationDTO
            {
                UserId = affiliate.AppUserId,
                Title = "Order Placed Successfully!",
                Message = $"Your order #{order.Id} for {order.CustomerName} totaling ${order.TotalPrice:F2} has been placed.",
                Type = NotificationType.Order,
                RelatedEntityId = order.Id.ToString()
            });

            await _emailService.SendOrderConfirmationEmailAsync(
                affiliate.AppUser?.Email ?? "",
                order.CustomerName,
                order.Id,
                order.TotalPrice);
        }

        foreach (var merchantOrder in order.MerchantOrder)
        {
            if (merchantOrder.Merchant?.AppUserId == null) continue;

            _notificationService.CreateNotification(new CreateNotificationDTO
            {
                UserId = merchantOrder.Merchant.AppUserId,
                Title = "New Order Received!",
                Message = $"You have a new order #{order.Id} totaling ${order.TotalPrice:F2}.",
                Type = NotificationType.Merchant,
                RelatedEntityId = order.Id.ToString()
            });
        }
    }

    private async Task PublishStatusChangedEventsAsync(Order order, OrderStatus oldStatus, OrderStatus newStatus)
    {
        var affiliate = order.Affiliate;
        if (affiliate == null) return;

        _notificationService.CreateNotification(new CreateNotificationDTO
        {
            UserId = affiliate.AppUserId,
            Title = "Order Status Updated",
            Message = $"Order #{order.Id} status changed from {oldStatus} to {newStatus}.",
            Type = NotificationType.Order,
            RelatedEntityId = order.Id.ToString()
        });

        await _emailService.SendOrderConfirmationEmailAsync(
            affiliate.AppUser?.Email ?? "",
            order.CustomerName,
            order.Id,
            order.TotalPrice);
    }
}
