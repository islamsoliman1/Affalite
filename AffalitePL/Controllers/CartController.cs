using AffaliteBL.DTOs.CartDTOs;
using AffaliteBL.IServices;
using AffalitePL.Helpers;
using AutoMapper;
using AffaliteBL.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AffalitePL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;
        private readonly string _imagesBaseUrl;

        public CartController(ICartService cartService, IOptions<ApiSettings> apiSettings,IMapper mapper)
        {
            _cartService = cartService;
            _mapper = mapper;
            _imagesBaseUrl = apiSettings.Value.BaseUrl ?? string.Empty;

        }

        //private CartUiResponseDto MapUi(int cartId)
        //{
        //    var cart = _cartService.GetCartById(cartId);
        //    return CartUiMapper.Map(cart, _imagesBaseUrl);
        //}

        [HttpGet("{userId:int}")]
        public IActionResult Get(int userId)
        {
            var cart = _cartService.GetCartByUserId(userId);
            var res = _mapper.Map<CartDTO> (cart);
            if (cart == null)
                return NotFound($"Cart with id: {userId} not found");

            return Ok(res);
        }

        //[HttpPost]
        //public IActionResult CreateCart(int userId)
        //{
        //    var cart = _cartService.CreateCart(userId);
        //    return Ok(CartUiMapper.Map(cart, _imagesBaseUrl));
        //}

        //[HttpDelete("{cartId:int}")]
        //public IActionResult DeleteCart(int cartId)
        //{
        //    var cart = _cartService.GetCartById(cartId);
        //    if (cart == null)
        //        return NotFound($"Cart with id: {cartId} not found");

        //    _cartService.DeleteCart(cartId);
        //    return Ok("Cart deleted successfully");
        //}

        [HttpPost("{userId:int}/items")]
        public IActionResult CreateItem(int userId, [FromBody] AddCartItemDTO? addCartItemDTO,decimal? affilaiteCommission)
        {
            try
            {
                _cartService.CreateItem(userId, addCartItemDTO, affilaiteCommission);
                var cart = _cartService.GetCartByUserId(userId);
                var res = _mapper.Map<CartDTO>(cart);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //[HttpDelete("{cartId:int}/items/{itemId:int}")]
        //public IActionResult DeleteItems(int cartId, int itemId)
        //{
        //    try
        //    {
        //        _cartService.DeleteItem(cartId, itemId);
        //        return Ok(MapUi(cartId));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        ///// <summary>Matches Angular cart service (update by product id).</summary>
        //[HttpPut("{cartId:int}/products/{productId:int}")]
        //public IActionResult UpdateItemByProduct(int cartId, int productId, [FromBody] UpdateCartItemDTO updateCartItemDTO)
        //{
        //    try
        //    {
        //        _cartService.UpdateItemByProductId(cartId, productId, updateCartItemDTO);
        //        return Ok(MapUi(cartId));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        /// <summary>Matches Angular cart service (remove by product id).</summary>
        [HttpDelete("{userId:int}/products/{productId:int}")]
        public IActionResult DeleteItemByProduct(int userId, int productId)
        {
            try
            {
                _cartService.DeleteItemByProductId(userId, productId);
                var cart = _cartService.GetCartByUserId(userId);
                var res = _mapper.Map <CartDTO>(cart);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Coupon handling not implemented yet; returns false for the SPA.</summary>
        [HttpPost("{userId:int}/coupon")]
        public IActionResult ApplyCoupon(int userId, [FromBody] ApplyCouponRequestDto? body)
        {
            _ = body?.Code;
            if (_cartService.GetCartByUserId(userId) == null)
                return NotFound($"Cart with id: {userId} not found");

            return Ok(false);
        }
    }
}
