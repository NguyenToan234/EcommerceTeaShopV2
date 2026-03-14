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
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        IGenericRepository<Cart> cartRepository,
        IGenericRepository<CartItem> cartItemRepository,
        IGenericRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
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

            var items = cart.CartItems.Select(x => new ReadCartItemDTO
            {
                CartItemId = x.Id,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.ProductVariant.Product.Name,
                Gram = x.ProductVariant.Gram,
                Price = x.ProductVariant.Price,
                Quantity = x.Quantity
            }).ToList();

            var total = items.Sum(x => x.Price * x.Quantity);

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            response.Data = new
            {
                Items = items,
                TotalPrice = total
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
                .FirstOrDefaultAsync(x => x.Id == dto.CartItemId);

            if (cartItem == null || cartItem.Cart.ClientId != clientId)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Không tìm thấy sản phẩm trong giỏ.";
                return response;
            }

            cartItem.Quantity = dto.Quantity;

            await _cartItemRepository.Update(cartItem);
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

            await _cartItemRepository.Delete(cartItem);
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
}