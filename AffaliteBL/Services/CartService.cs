using AffaliteBL.DTOs.CartDTOs;
using AffaliteBL.IServices;
using AffaliteDAL.Entities;
using AffaliteDAL.IRepo;
using AutoMapper;

namespace AffaliteBL.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepo _repo;
        private readonly IMapper mapper;

        public CartService(ICartRepo repo , IMapper mapper)
        {
            _repo = repo;
            this.mapper = mapper;
        }

        public CartDTO? GetCartByUserId(int userId)
        {
             var cart = _repo.GetCartWithAffilaiteId(userId);

            var cartDto = mapper.Map<CartDTO>(cart);
            if (cart == null)
                return cartDto;
            // SubTotal, AffiliateCommission, Shipping, and Total are computed from cart entity
            return cartDto;
        }

        public Cart CreateCart(int userId)
        {
            var cart = new Cart { AffiliateId = userId, CreatedAt = DateTime.Now };

            _repo.Add(cart);
            _repo.Save();

            return cart;
        }

        public void DeleteCart(int id)
        {
            var cart = _repo.GetById(id);
            if (cart == null)
                throw new Exception("Cart not found");

            _repo.Delete(cart);
            _repo.Save();
        }

        public void CreateItem(int userId, AddCartItemDTO? addCartItemDTO,decimal? affilaiteCommission)
        {
            var cart = _repo.GetCartWithAffilaiteId(userId);
            if (cart == null)
                cart = CreateCart(userId);
            //
            if(addCartItemDTO.ProductId !=0)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == addCartItemDTO.ProductId);
                if (item != null)
                {
                    item.Quantity = addCartItemDTO.Quantity;
                    _repo.UpdateItem(item);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = addCartItemDTO.ProductId,
                        Quantity = addCartItemDTO.Quantity
                    };
                    _repo.AddItem(cartItem);
                }
            }
            if(affilaiteCommission != null)
            {
                cart.AffiliateCommissionPct = (decimal)affilaiteCommission;
            }
            _repo.Save();
             cart = _repo.GetCartWithAffilaiteId(userId);

            // Cart totals are computed automatically from Items

            _repo.Save();
            
        }


        //public void DeleteItem(int cartId, int itemId)
        //{
        //    var item = _repo.GetCartItem(cartId, itemId);
        //    if (item == null)
        //        throw new Exception("Cart item not found");

        //    _repo.DeleteItem(item);
        //    _repo.Save();
        //}

        //public void UpdateItemByProductId(int cartId, int productId, UpdateCartItemDTO updateCartItemDTO)
        //{
        //    var cart = _repo.GetCartWithItems(cartId);
        //    if (cart == null)
        //        throw new Exception("Cart not found");

        //    var line = cart.Items?.FirstOrDefault(i => i.ProductId == productId);
        //    if (line == null)
        //        throw new Exception("Cart item not found");

        //    if (updateCartItemDTO.Quantity <= 0)
        //    {
        //        _repo.DeleteItem(line);
        //        _repo.Save();
        //        return;
        //    }

        //    line.Quantity = updateCartItemDTO.Quantity;
        //    _repo.UpdateItem(line);
        //    _repo.Save();
        //}

        public void DeleteItemByProductId(int userId, int productId)
        {
            var cart = _repo.GetCartWithAffilaiteId(userId);
            if (cart == null)
                throw new Exception("Cart not found");

            var line = cart.Items?.FirstOrDefault(i => i.ProductId == productId);
            if (line == null)
                throw new Exception("Cart item not found");

            _repo.DeleteItem(line);
            _repo.Save();
        }
    }
}