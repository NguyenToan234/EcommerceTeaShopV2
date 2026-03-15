using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;

public class CartService : ICartService
{
    private readonly IGenericRepository<Cart> _cartRepository;
    private readonly IGenericRepository<CartItem> _cartItemRepository;
    private readonly IGenericRepository<ProductVariant> _variantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Coupon> _couponRepository;

    public CartService(
        IGenericRepository<Cart> cartRepository,
        IGenericRepository<CartItem> cartItemRepository,
        IGenericRepository<ProductVariant> variantRepository,
        IGenericRepository<Coupon> couponRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _variantRepository = variantRepository;
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    private async Task RecalculateCartAsync(Cart cart)
    {
        var db = _cartRepository.GetDbContext();

        var items = await db.Set<CartItem>()
            .Where(x => x.CartId == cart.Id)
            .Include(x => x.ProductVariant)
            .ToListAsync();

        var total = items
            .Where(x => x.ProductVariant != null)
            .Sum(x => x.ProductVariant.Price * x.Quantity);
        cart.TotalAmount = total;

        if (cart.CouponId == null)
        {
            cart.DiscountAmount = 0;
            cart.FinalAmount = total;
            return;
        }

        var coupon = await db.Set<Coupon>()
            .FirstOrDefaultAsync(x => x.Id == cart.CouponId && x.IsActive);

        if (coupon == null)
        {
            cart.CouponId = null;
            cart.DiscountAmount = 0;
            cart.FinalAmount = total;
            return;
        }

        if (coupon.MinOrderValue != null && total < coupon.MinOrderValue)
        {
            cart.CouponId = null;
            cart.DiscountAmount = 0;
            cart.FinalAmount = total;
            return;
        }

        decimal discount = total * coupon.DiscountPercent / 100;

        if (coupon.MaxDiscountAmount != null)
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

        cart.DiscountAmount = discount;
        cart.FinalAmount = total - discount;
    }

    public async Task<ResponseDTO> AddToCartAsync(Guid clientId, AddToCartDTO dto)
    {
        ResponseDTO response = new();

        try
        {
            if (dto.Quantity <= 0)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.VALIDATION_FAILED;
                response.Message = "Số lượng không hợp lệ.";
                return response;
            }

            var db = _cartRepository.GetDbContext();

            var variant = await db.Set<ProductVariant>()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == dto.ProductVariantId);

            if (variant == null)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Không tìm thấy biến thể sản phẩm.";
                return response;
            }

            if (!variant.Product.IsActive)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.INVALID_ACTION;
                response.Message = "Sản phẩm đã ngừng bán.";
                return response;
            }

            var cart = await db.Set<Cart>()
                .Include(x => x.CartItems)
                .FirstOrDefaultAsync(x => x.ClientId == clientId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    ClientId = clientId,
                    CartItems = new List<CartItem>()
                };

                await _cartRepository.Insert(cart);
            }

            var cartItem = cart.CartItems
                .FirstOrDefault(x => x.ProductVariantId == dto.ProductVariantId);

            var newQuantity = cartItem != null
                ? cartItem.Quantity + dto.Quantity
                : dto.Quantity;

            if (variant.StockQuantity < newQuantity)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.INVALID_ACTION;
                response.Message = "Sản phẩm không đủ tồn kho.";
                return response;
            }

            if (cartItem != null)
            {
                cartItem.Quantity += dto.Quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity
                };

                await _cartItemRepository.Insert(cartItem);
            }

            await RecalculateCartAsync(cart);

            await _cartRepository.Update(cart);
            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
            response.Message = "Thêm vào giỏ hàng thành công.";
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseDTO> GetCartAsync(Guid clientId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _cartRepository.GetDbContext();

            var cart = await db.Set<Cart>()
                .Include(x => x.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(x => x.ClientId == clientId);

            if (cart == null || !cart.CartItems.Any())
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Giỏ hàng trống.";
                return response;
            }

            var items = cart.CartItems
                .Select(x => new ReadCartItemDTO
                {
                    CartItemId = x.Id,
                    ProductVariantId = x.ProductVariantId,
                    ProductName = x.ProductVariant.Product.Name,
                    Gram = x.ProductVariant.Gram,
                    Price = x.ProductVariant.Price,
                    Quantity = x.Quantity
                }).ToList();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            response.Data = new
            {
                Items = items,
                TotalPrice = cart.TotalAmount,
                Discount = cart.DiscountAmount,
                FinalPrice = cart.FinalAmount,
                CouponId = cart.CouponId
            };
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }
    public async Task<ResponseDTO> UpdateQuantityAsync(Guid clientId, UpdateCartItemDTO dto)
    {
        ResponseDTO response = new();

        try
        {
            if (dto.Quantity <= 0)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.VALIDATION_FAILED;
                response.Message = "Số lượng không hợp lệ.";
                return response;
            }

            var db = _cartRepository.GetDbContext();

            var cartItem = await db.Set<CartItem>()
                .Include(x => x.Cart)
                .Include(x => x.ProductVariant)
                .FirstOrDefaultAsync(x => x.Id == dto.CartItemId);

            if (cartItem == null || cartItem.Cart.ClientId != clientId)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Không tìm thấy sản phẩm trong giỏ.";
                return response;
            }

            // check stock
            if (cartItem.ProductVariant.StockQuantity < dto.Quantity)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.INVALID_ACTION;
                response.Message = "Sản phẩm không đủ tồn kho.";
                return response;
            }

            cartItem.Quantity = dto.Quantity;

            await _cartItemRepository.Update(cartItem);

            await RecalculateCartAsync(cartItem.Cart);

            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            response.Message = "Cập nhật giỏ hàng thành công.";
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }


    public async Task<ResponseDTO> RemoveItemAsync(Guid clientId, Guid cartItemId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _cartRepository.GetDbContext();

            var cartItem = await db.Set<CartItem>()
                .Include(x => x.Cart)
                .FirstOrDefaultAsync(x => x.Id == cartItemId);

            if (cartItem == null || cartItem.Cart.ClientId != clientId)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Không tìm thấy sản phẩm trong giỏ.";
                return response;
            }

            var cart = cartItem.Cart;

            await _cartItemRepository.Delete(cartItem);

            await RecalculateCartAsync(cart);

            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
            response.Message = "Xóa sản phẩm khỏi giỏ hàng thành công.";
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseDTO> ApplyCouponAsync(Guid clientId, string code)
    {
        ResponseDTO response = new();

        try
        {
            var db = _cartRepository.GetDbContext();

            var cart = await db.Set<Cart>()
                .Include(x => x.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(x => x.ClientId == clientId);

            if (cart == null || !cart.CartItems.Any())
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Giỏ hàng trống.";
                return response;
            }

            var coupon = await _couponRepository
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Code == code && x.IsActive);

            if (coupon == null)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Coupon không hợp lệ.";
                return response;
            }

            if (DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.INVALID_ACTION;
                response.Message = "Coupon đã hết hạn.";
                return response;
            }

            cart.CouponId = coupon.Id;

            await RecalculateCartAsync(cart);

            await _cartRepository.Update(cart);
            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            response.Data = new
            {
                TotalPrice = cart.TotalAmount,
                Discount = cart.DiscountAmount,
                FinalPrice = cart.FinalAmount
            };
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseDTO> RemoveCouponAsync(Guid clientId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _cartRepository.GetDbContext();

            var cart = await db.Set<Cart>()
                .FirstOrDefaultAsync(x => x.ClientId == clientId);

            if (cart == null)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Giỏ hàng không tồn tại.";
                return response;
            }

            cart.CouponId = null;

            await RecalculateCartAsync(cart);

            await _cartRepository.Update(cart);
            await _unitOfWork.SaveChangeAsync();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            response.Message = "Đã gỡ coupon.";
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }
}