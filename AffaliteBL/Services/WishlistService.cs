using AffaliteBL.DTOs.WishlistDTOS;
using AffaliteBL.IServices;
using AffaliteDAL.Entities;
using AffaliteBL.Helpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffaliteBL.Services;
public class WishlistService : IWishlistService
{
    private readonly IWishlistRepo _wishlistRepo;
    private readonly ApiSettings _settings;

    public WishlistService(IWishlistRepo wishlistRepo, IOptions<ApiSettings> options)
    {
        _wishlistRepo = wishlistRepo;
        _settings = options.Value;
    }

    public List<WishlistItemDTO> GetWishlist(int affiliateId)
    {
        var items = _wishlistRepo.GetByAffiliateId(affiliateId);
        return items.Select(w => new WishlistItemDTO
        {
            Id = w.Id,
            ProductId = w.ProductId,
            Name = w.Product?.Name ?? "",
            Price = w.Product?.Price ?? 0,
            ImageUrl = w.Product?.Images?.FirstOrDefault()?.ImageUrl != null
                ? $"{_settings.BaseUrl}images/products/{w.Product!.Images.First().ImageUrl}"
                : null,
            CreatedAt = w.CreatedAt
        }).ToList();
    }

    public string AddToWishlist(int affiliateId, int productId)
    {
        var existing = _wishlistRepo.GetByAffiliateAndProduct(affiliateId, productId);
        if (existing != null)
            return "Already in wishlist";

        _wishlistRepo.Add(new Wishlist
        {
            AffiliateId = affiliateId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        });
        _wishlistRepo.Save();
        return "Added to wishlist";
    }

    public string RemoveFromWishlist(int affiliateId, int productId)
    {
        var item = _wishlistRepo.GetByAffiliateAndProduct(affiliateId, productId);
        if (item == null)
            return "Item not found";

        _wishlistRepo.Remove(item);
        _wishlistRepo.Save();
        return "Removed from wishlist";
    }
}