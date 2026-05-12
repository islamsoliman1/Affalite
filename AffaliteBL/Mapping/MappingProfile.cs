using AffaliteBL.DTOs.AffiliateDTOs;
using AffaliteBL.DTOs.Auth;
using AffaliteBL.DTOs.CartDTOs;
using AffaliteBL.DTOs.CategoryDTOs;
using AffaliteBL.DTOs.CommissionDTOs;
using AffaliteBL.DTOs.MerchantDTOs;
using AffaliteBL.DTOs.OrderDTOs;
using AffaliteBL.DTOs.ReviewDTOs;
using AffaliteBL.Helpers;
using AffaliteBLL.DTOs;
using AffaliteBLL.DTOs.Products;
using AffaliteDAL.Entities;
using AffaliteDAL.Entities.Enums;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffaliteBL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Affiliate
            CreateMap<Affiliate, GetAffiliateDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.AppUser.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.AppUser.Email))
                .ReverseMap();
            CreateMap<Affiliate, CreateAffiliateDTO>().ReverseMap();
            CreateMap<Affiliate, UpdateAffiliateDTO>().ReverseMap();
            CreateMap<Affiliate, AffiliateBalanceDTO>().ReverseMap();
            //CreateMap<Order, OrderReadDTO>().ReverseMap();
            CreateMap<Commission, CommissionReadDTO>().ReverseMap();
            //Cart
            CreateMap<Cart, CartDTO>().ReverseMap();
            CreateMap<CartItem, CartItemDTO>().ReverseMap();

            CreateMap<CartItem, UpdateCartItemDTO>().ReverseMap();
            //.ForMember(dest => dest.im, opt => opt.MapFrom<ImageUrlResolver>())


            //review
            CreateMap<ProductReviews, ProductReviewDto>()
                .ForMember(dest => dest.AffiliateName, opt => opt.MapFrom(src => src.Affiliate.AppUser.FullName));


            //Product
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                //.ForMember(dest => dest.MerchantName, opt => opt.MapFrom(src => src.Merchant != null ? src.Merchant.Name : string.Empty))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))

                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.MerchantName, opt => opt.MapFrom(src => src.Merchant != null && src.Merchant.AppUser != null ? src.Merchant.AppUser.FullName : string.Empty))
                //.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Images, opt => opt.MapFrom<ImageUrlResolver>())
                .ForMember(dest => dest.MerchantName, opt => opt.MapFrom(src => src.Merchant.AppUser.FullName))
                .ForMember(dest => dest.Images, opt => opt.MapFrom<ImageUrlResolver>())
                    .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews)).ReverseMap();


            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProductStatus.Active)) // Default status on create
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            CreateMap<UpdateProductDto, Product>();

            // Ordera  and commissions
            CreateMap<Order, OrderReadDTO>().ForMember(dest => dest.AffiliateName,
               opt => opt.MapFrom(src => src.Affiliate.AppUser.FullName))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<OrderItem, OrderItemDTO>()
        .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Product.Images)).ForMember(dest => dest.ProductName,
              opt => opt.MapFrom(src => src.Product.Name)).ReverseMap();


            CreateMap<OrderCreateDTO, Order>();
            CreateMap<Commission, CommissionReadDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));



            //// User
            //CreateMap<AppUser, UserDTO>().ReverseMap();
            //CreateMap<CartItem, CartItemDTO>()
            //    .ForMember(d => d.ProductName,
            //        o => o.MapFrom(s => s.Product.Name))
            //    .ForMember(d => d.Price,
            //        o => o.MapFrom(s => s.Product.Price))
            //    .ForMember(d => d.TotalPrice,
            //    o => o.MapFrom(s => (s.Product.Price * s.Quantity)))
            //    .ForMember(d => d.PictureUrl,
            //        o => o.MapFrom<CartItemImageUrlResolver>()); // 👈 هنا بدل MapFrom مباشرة
            //// OrderItem
            //CreateMap<OrderItem, OrderItemDTO>()
            //    .ForMember(d => d.ProductName,
            //        o => o.MapFrom(s => s.Product.Name))
            //    .ForMember(d => d.Price,
            //        o => o.MapFrom(s => s.Product.Price));

            //// Cart
            //CreateMap<Cart, CartDTO>()
            //    .ForMember(d => d.TotalPrice,
            //        o => o.MapFrom(s => s.Items.Sum(i => i.Product.Price * i.Quantity)));

            //// Order
            //CreateMap<Order, OrderDTO>().ReverseMap();
            //CreateMap<Order, CreateOrderDTO>().ReverseMap();

            //CreateMap<OrderItem, OrderItemDTO>()
            //    .ForMember(dest => dest.ProductName,
            //               opt => opt.MapFrom(src => src.Product.Name));

            // Register -> AppUser
            CreateMap<RegisterDTO, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // AppUser -> Response
            CreateMap<AppUser, AuthResponseDTO>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.Expiration, opt => opt.Ignore());


            // Merchant
            CreateMap<Merchant, GetMerchantDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.AppUser.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.AppUser.Email))
                .ReverseMap();
            CreateMap<Merchant, CreateMerchantDTO>().ReverseMap();
            CreateMap<Merchant, UpdateMerchantDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.AppUser.FullName))
                .ReverseMap();
            CreateMap<Merchant, MerchantBalanceDTO>().ReverseMap();

            //Category
            CreateMap<Category, GetCategoryDTO>().ReverseMap();
            CreateMap<Category, CreateCategoryDTO>().ReverseMap();
            CreateMap<Category, UpdateCategoryDTO>().ReverseMap();


            //CreateMap<Order, OrderReadDTO>();





        }
    }
}