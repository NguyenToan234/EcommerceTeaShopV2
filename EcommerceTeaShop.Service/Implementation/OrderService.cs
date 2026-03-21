using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Repository.Models.EnumModels;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;
using EcommerceTeaShop.Common.DTOs.Enums;
public class OrderService : IOrderService
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<OrderDetails> _orderDetailsRepository;
    private readonly IGenericRepository<Cart> _cartRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly PaymentService _paymentService;
        private readonly ITransactionService _transactionService;
    private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;

    public OrderService(
      IGenericRepository<Order> orderRepository,
      IGenericRepository<OrderDetails> orderDetailsRepository,
      IGenericRepository<Cart> cartRepository,
      IGenericRepository<Product> productRepository,
      IUnitOfWork unitOfWork,
      PaymentService paymentService,
      ITransactionService transactionService,
      ICartService cartService)
    {
        _orderRepository = orderRepository;
        _orderDetailsRepository = orderDetailsRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _transactionService = transactionService;
        _cartService = cartService;
    }

    public async Task<ResponseDTO> CheckoutAsync(Guid clientId, Guid addressId, List<Guid>? cartItemIds)
    {
        ResponseDTO response = new();

        try
        {
            var db = _orderRepository.GetDbContext();

            // 🔥 0. CHECK existing pending order
            var existingOrder = await db.Set<Order>()
                .Where(x => x.ClientId == clientId && x.Status == OrderStatus.Pending)
                .OrderByDescending(x => x.OrderDate)
                .FirstOrDefaultAsync();

            if (existingOrder != null)
            {
                var diff = DateTime.UtcNow - existingOrder.OrderDate;

                // ⏱ còn hạn 2 phút → reuse
                if (diff.TotalMinutes <= 2)
                {
                    return new ResponseDTO
                    {
                        IsSucess = true,
                        Message = "Tiếp tục thanh toán đơn cũ",
                        Data = new
                        {
                            OrderId = existingOrder.Id,
                            CheckoutUrl = existingOrder.CheckoutUrl // ✅ dùng lại link cũ
                        }
                    };
                }
                else
                {
                    // ⏱ hết hạn → hủy
                    existingOrder.Status = OrderStatus.Cancelled;
                    await _unitOfWork.SaveChangeAsync();
                }
            }

            // 1. Lấy cart
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

            // 2. Chọn item
            var selectedItems = (cartItemIds == null || !cartItemIds.Any())
                ? cart.CartItems.ToList()
                : cart.CartItems.Where(x => cartItemIds.Contains(x.Id)).ToList();

            if (!selectedItems.Any())
            {
                response.IsSucess = false;
                response.Message = "Không có sản phẩm nào được chọn.";
                return response;
            }

            // 3. Address
            var address = await db.Set<Addresses>()
                .FirstOrDefaultAsync(x => x.Id == addressId && x.ClientId == clientId);

            if (address == null)
            {
                response.IsSucess = false;
                response.Message = "Không tìm thấy địa chỉ.";
                return response;
            }

            // 4. Check stock (CHỈ check)
            foreach (var item in selectedItems)
            {
                if (item.ProductVariant.StockQuantity < item.Quantity)
                {
                    response.IsSucess = false;
                    response.Message = $"Sản phẩm {item.ProductVariant.Product.Name} không đủ hàng.";
                    return response;
                }
            }

            // 5. Tính tiền
            var totalPrice = selectedItems.Sum(x => x.Price * x.Quantity);

            if (totalPrice < 2000)
            {
                response.IsSucess = false;
                response.Message = "Số tiền phải >= 2000.";
                return response;
            }

            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // 6. Tạo order
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

            // 7. OrderDetails
            foreach (var item in selectedItems)
            {
                var orderDetail = new OrderDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    AddonId = item.AddonId
                };

                await _orderDetailsRepository.Insert(orderDetail);
            }

            await _unitOfWork.SaveChangeAsync();

            // 8. Payment (TẠO 1 LẦN DUY NHẤT)
            var checkoutUrl = await _paymentService.CreatePaymentLink(
                orderCode,
                (int)totalPrice
            );

            // 🔥 lưu lại để reuse
            order.CheckoutUrl = checkoutUrl;
            await _unitOfWork.SaveChangeAsync();

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

        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var order = await db.Set<Order>()
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.OrderCode == orderCode);

            if (order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng");
            }

            // 🟢 FIX 1: chống webhook gọi 2 lần
            if (order.Status == OrderStatus.Paid)
            {
                Console.WriteLine("Đơn hàng đã thanh toán trước đó.");
                return;
            }

            // 🟢 Lock order
            order.Status = OrderStatus.Paid;

            await _transactionService.CreateTransactionAsync(
    order.Id,
    order.TotalPrice,
    orderCode.ToString(),
    EcommerceTeaShop.Common.DTOs.Enums.PaymentMethod.PayOS,
    "PayOS"
);

            foreach (var item in order.OrderDetails)
            {
                var variant = await db.Set<ProductVariant>()
                    .FirstOrDefaultAsync(x => x.Id == item.ProductVariantId);

                if (variant == null)
                    throw new Exception("Product variant không tồn tại");

                // 🟢 FIX 2: check stock trước khi trừ
                if (variant.StockQuantity < item.Quantity)
                    throw new Exception("Stock không đủ khi confirm payment");

                variant.StockQuantity -= item.Quantity;
            }

            // Xóa cart
            var cart = await db.Set<Cart>()
                .Include(x => x.CartItems)
                .FirstOrDefaultAsync(x => x.ClientId == order.ClientId);

            if (cart != null)
            {
                db.Set<CartItem>().RemoveRange(cart.CartItems);
            }

            await _unitOfWork.SaveChangeAsync();

            await transaction.CommitAsync();

            Console.WriteLine("Hoàn tất xử lý webhook thanh toán");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine("Payment confirm failed: " + ex.Message);
            throw;
        }
    }
}