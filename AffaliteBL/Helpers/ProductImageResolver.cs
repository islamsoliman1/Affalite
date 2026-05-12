using AffaliteBL.DTOs.CartDTOs;
using AffaliteBLL.DTOs.Products;
using AffaliteDAL.Entities;
using AutoMapper;

using Microsoft.Extensions.Options;

namespace AffaliteBL.Helpers
{
    public class ImageUrlResolver : IValueResolver<Product, ProductDto, List<string>>
    {
        private readonly ApiSettings _settings;

        public ImageUrlResolver(IOptions<ApiSettings> options)
        {
            _settings = options.Value;
        }

        public List<string> Resolve(Product source, ProductDto destination, List<string> destMember, ResolutionContext context)
        {
            if (source.Images == null || !source.Images.Any())
                return new List<string>();

            // بناء اللينكات لكل صورة
            return source.Images.Select(img => $"{_settings.BaseUrl}images/products/{img.ImageUrl}").ToList();
        }

        public class CartItemImageUrlResolver : IValueResolver<CartItem, AddCartItemDTO, string>
        {
            private readonly ApiSettings _settings;

            public CartItemImageUrlResolver(IOptions<ApiSettings> options)
            {
                _settings = options.Value;
            }

            public string Resolve(CartItem source, AddCartItemDTO destination, string destMember, ResolutionContext context)
            {
                if (source.Product == null || source.Product.Images == null || !source.Product.Images.Any())
                    return null;

                return source.Product.Images.Select(img => $"{_settings.BaseUrl}images/products/{img.ImageUrl}").FirstOrDefault();
            }
        }
    }
}