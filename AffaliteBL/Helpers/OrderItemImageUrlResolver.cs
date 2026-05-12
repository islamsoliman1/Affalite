using AffaliteBL.DTOs.OrderDTOs;
using AffaliteDAL.Entities;
using AutoMapper;
using AffaliteBL.Helpers;
using Microsoft.Extensions.Options;

public class OrderItemImageUrlResolver
    : IValueResolver<OrderItem, OrderItemDTO, List<string>>
{
    private readonly ApiSettings _settings;

    public OrderItemImageUrlResolver(IOptions<ApiSettings> options)
    {
        _settings = options.Value;
    }

    public List<string> Resolve(
        OrderItem source,
        OrderItemDTO destination,
        List<string> destMember,
        ResolutionContext context)
    {
        if (source.Product?.Images == null || !source.Product.Images.Any())
            return new List<string>();

        return source.Product.Images
            .Select(img => $"{_settings.BaseUrl}images/products/{img.ImageUrl}")
            .ToList();
    }
}