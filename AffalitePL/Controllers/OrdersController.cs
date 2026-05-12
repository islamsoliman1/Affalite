using AffaliteBL.DTOs.OrderDTOs;
using AffaliteBL.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AffalitePL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IAffiliateService _affiliateService;

    public OrdersController(IOrderService orderService, IAffiliateService affiliateService)
    {
        _orderService = orderService;
        _affiliateService = affiliateService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderCreateDTO dto)
    {
        var result = await _orderService.CreateOrderAsync(dto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("merchant/{merchantId}")]
    public async Task<IActionResult> GetByMerchant(int merchantId)
    {
        var orders = await _orderService.GetOrdersByMerchantAsync(merchantId);
        if (!orders.Any()) return NotFound();
        return Ok(orders);
    }

    [HttpGet("affiliate")]
    public async Task<IActionResult> GetByAffiliate()
    {
        var userId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var aff = _affiliateService.GetAffiliateUserId(userId);
        if (aff == null) return NotFound();

        var orders = await _orderService.GetOrdersByAffiliateAsync(aff.Id);
        return Ok(orders);
    }

    [HttpGet("affiliate/{id}")]
    public async Task<IActionResult> GetByAffiliate(int id)
    {
        var orders = await _orderService.GetOrdersByAffiliateAsync(id);
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AffaliteDAL.Entities.Enums.OrderStatus status)
    {
        var success = await _orderService.UpdateStatusAsync(id, status);
        if (!success) return NotFound();
        return NoContent();
    }
}
