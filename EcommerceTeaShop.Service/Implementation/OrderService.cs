using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Repository.Models.EnumModels;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System.Net;

public class OrderService : IOrderService
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<OrderDetails> _orderDetailsRepository;
    private readonly IGenericRepository<Cart> _cartRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly PaymentService _paymentService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
      IGenericRepository<Order> orderRepository,
      IGenericRepository<OrderDetails> orderDetailsRepository,
      IGenericRepository<Cart> cartRepository,
      IGenericRepository<Product> productRepository,
      IUnitOfWork unitOfWork,
      PaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _orderDetailsRepository = orderDetailsRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
    }

    public async Task<ResponseDTO> CheckoutAsync(Guid clientId, Guid addressId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _orderRepository.GetDbContext();

            var cart = await db.Set<Cart>()
               .Include(x => x.CartItems)
.ThenInclude(ci => ci.ProductVariant)
.ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(x => x.ClientId == clientId);

            if (cart == null || !cart.CartItems.Any())
            {
                response.IsSucess = false;
                response.Message = "Giỏ hàng trống.";
                return response;
            }

            // Lấy address user
            var address = await db.Set<Addresses>()
                .FirstOrDefaultAsync(x => x.Id == addressId && x.ClientId == clientId);

            if (address == null)
            {
                response.IsSucess = false;
                response.Message = "Không tìm thấy địa chỉ.";
                return response;
            }

            decimal totalPrice = 0;

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Tính tổng tiền trước
            foreach (var item in cart.CartItems)
            {
                if (item.ProductVariant.StockQuantity < item.Quantity)
                {
                    response.IsSucess = false;
                    response.Message = $"Sản phẩm {item.ProductVariant.Product.Name} không đủ hàng.";
                    return response;
                }

                totalPrice += item.ProductVariant.Price * item.Quantity;
            }

            if (totalPrice < 2000)
            {
                response.IsSucess = false;
                response.Message = "Số tiền phải >= 2000.";
                return response;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                OrderCode = orderCode,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,

                FullName = address.FullName,
                Phone = address.Phone,
                AddressLine = address.AddressLine,
                City = address.City,
                District = address.District,
                Ward = address.Ward,

                TotalPrice = totalPrice
            };

            await _orderRepository.Insert(order);

            foreach (var item in cart.CartItems)
            {
                var orderDetail = new OrderDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    Price = item.ProductVariant.Price
                };

                await _orderDetailsRepository.Insert(orderDetail);
            }

            await _unitOfWork.SaveChangeAsync();

            var checkoutUrl = await _paymentService.CreatePaymentLink(
                orderCode,
                (int)totalPrice
            );

            response.IsSucess = true;
            response.Message = "Đặt hàng thành công.";
            response.Data = new
            {
                OrderId = order.Id,
                CheckoutUrl = checkoutUrl
            };
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.Message = ex.InnerException?.Message ?? ex.Message;
        }

        return response;
    }
    public async Task<ResponseDTO> GetMyOrdersAsync(Guid clientId)
    {
        ResponseDTO response = new();

        try
        {
            var db = _orderRepository.GetDbContext();

            var orders = await db.Set<Order>()
               .Include(x => x.OrderDetails)
.ThenInclude(od => od.ProductVariant)
.ThenInclude(v => v.Product)
                .Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            response.Data = orders.Select(o => new
            {
                o.Id,
                o.OrderCode,
                o.TotalPrice,
                Status = o.Status.ToString(),
                o.OrderDate,

                ShippingAddress = new
                {
                    o.FullName,
                    o.Phone,
                    o.AddressLine,
                    o.Ward,
                    o.District,
                    o.City
                },

                Items = o.OrderDetails.Select(d => new
                {
                    ProductName = d.ProductVariant.Product.Name,
                    Gram = d.ProductVariant.Gram,
                    d.Price,
                    d.Quantity
                })
            });
        }
        catch (Exception ex)
        {
            response.IsSucess = false;
            response.BusinessCode = BusinessCode.EXCEPTION;
            response.Message = ex.Message;
        }

        return response;
    }

    public async Task<ResponseDTO> GetOrderByCodeAsync(long orderCode)
    {
        ResponseDTO response = new();

        try
        {
            var db = _orderRepository.GetDbContext();

            var order = await db.Set<Order>()
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (order == null)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                response.Message = "Không tìm thấy đơn hàng.";
                return response;
            }

            response.IsSucess = true;
            response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;

            response.Data = new
            {
                order.Id,
                order.OrderCode,
                order.TotalPrice,
                Status = order.Status.ToString(),
                order.OrderDate,

                Items = order.OrderDetails.Select(x => new
                {
                    ProductName = x.ProductVariant.Product.Name,
                    Gram = x.ProductVariant.Gram,
                    x.Price,
                    x.Quantity
                })
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
    public async Task ConfirmPayment(long orderCode)
    {
        Console.WriteLine($"[Webhook] Nhận thanh toán cho đơn hàng: {orderCode}");

        var db = _orderRepository.GetDbContext();

        var order = await db.Set<Order>()
            .Include(x => x.OrderDetails)
            .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

        if (order == null)
        {
            Console.WriteLine("Không tìm thấy đơn hàng");
            throw new Exception("Không tìm thấy đơn hàng");
        }

        if (order.Status == OrderStatus.Paid)
        {
            Console.WriteLine("Đơn hàng đã được thanh toán trước đó");
            return;
        }

        order.Status = OrderStatus.Paid;
        Console.WriteLine("Cập nhật trạng thái đơn hàng thành Paid");

        foreach (var item in order.OrderDetails)
        {
            var variant = await db.Set<ProductVariant>()
                .FirstOrDefaultAsync(x => x.Id == item.ProductVariantId);

            variant.StockQuantity -= item.Quantity;
        }

        var cart = await db.Set<Cart>()
            .Include(x => x.CartItems)
            .FirstOrDefaultAsync(x => x.ClientId == order.ClientId);

        if (cart != null)
        {
            db.Set<CartItem>().RemoveRange(cart.CartItems);
            Console.WriteLine("Đã xoá giỏ hàng sau khi thanh toán");
        }

        await _unitOfWork.SaveChangeAsync();

        Console.WriteLine("Hoàn tất xử lý webhook thanh toán");
    }
}