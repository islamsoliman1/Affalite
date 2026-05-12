using AffaliteBL.DTOs.CommissionDTOs;
using AffaliteBL.IServices;
using AffaliteDAL.Entities.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AffalitePL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommissionsController : ControllerBase
{
    private readonly ICommissionService _commissionService;
    private readonly IMerchantService _merchantService;

    public CommissionsController(ICommissionService commissionService, IMerchantService merchantService)
    {
        _commissionService = commissionService;
        _merchantService = merchantService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _commissionService.GetAll();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var result = _commissionService.GetCommissionById(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("order/{orderId}")]
    public IActionResult GetByOrder(int orderId)
    {
        var result = _commissionService.GetCommissionByOrderId(orderId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("affiliate/{affiliateId}")]
    public IActionResult GetByAffiliate(int affiliateId)
    {
        var result = _commissionService.GetCommissionsByAffiliate(affiliateId);
        return Ok(result);
    }

    [HttpGet("merchant")]
    public IActionResult GetByMerchant()
    {
        var userId = User.FindFirst("uid")?.Value;
        var merchant = _merchantService.GetMerchantByUserId(userId);
        if (merchant == null) return Unauthorized();

        var result = _commissionService.GetCommissionsByMerchant(merchant.Id);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] CommissionStatus status)
    {
        _commissionService.UpdateCommissionStatus(id, status);
        return NoContent();
    }
}
