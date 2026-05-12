using AffaliteBL.DTOs;
using AffaliteBL.Helpers;
using AffaliteBL.IServices;
using AffaliteBLL.DTOs.Products;
using AffaliteDAL.Entities;
using AffalitePL.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AffaliteAPI.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IMapper _mapper;
    private readonly IMerchantService _merchantService;

    public ProductsController(IProductService service, IMapper mapper, IMerchantService merchantService)
    {
        _service = service;
        _mapper = mapper;
        _merchantService = merchantService;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] ProductQueryParams query)
    {
        var products = _service.GetAll(query);
        var result = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _service.GetById(id);
        if (product == null) return NotFound();

        var result = _mapper.Map<ProductDto>(product);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
    {
        var userId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var merchant = _merchantService.GetMerchantByUserId(userId);
        if (merchant == null)
            return BadRequest(new { success = false, message = "Merchant profile not found." });

        dto.MerchantId = merchant.Id;
        var product = _mapper.Map<Product>(dto);

        if (dto.Images != null && dto.Images.Any())
        {
            foreach (var file in dto.Images)
            {
                var validation = FileUploadValidator.ValidateImage(file);
                if (validation != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                {
                    return BadRequest(new { success = false, message = validation.ErrorMessage });
                }

                var fileName = await FileUploadValidator.SaveImageAsync(file, "wwwroot/images/products/");
                product.Images.Add(new ProductImage
                {
                    FileName = fileName,
                    ImageUrl = fileName
                });
            }
        }

        _service.Create(product);
        return Ok(product);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromForm] UpdateProductDto dto)
    {
        _service.Update(id, dto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return Ok();
    }

    [HttpGet("category/{categoryId}")]
    public IActionResult GetByCategory(int categoryId, [FromQuery] ProductQueryParams query)
    {
        query.CategoryId = categoryId;
        var products = _service.GetAll(query);
        var result = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(result);
    }

    [HttpGet("merchant")]
    public IActionResult GetByMerchant([FromQuery] ProductQueryParams query)
    {
        var merchantId = User.FindFirst("uid")?.Value;
        var merchant = _merchantService.GetMerchantByUserId(merchantId);
        if (merchant == null)
            return BadRequest(new { success = false, message = "Merchant profile not found." });

        query.MerchantId = merchant.Id;
        var products = _service.GetAll(query);
        var result = _mapper.Map<IEnumerable<ProductDto>>(products);
        return Ok(result);
    }
}
