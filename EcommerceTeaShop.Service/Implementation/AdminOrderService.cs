using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Repository.Models.EnumModels;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Implementation
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IGenericRepository<Order> _orderRepo;

        public AdminOrderService(IGenericRepository<Order> orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<ResponseDTO> GetOrderDetailAsync(Guid orderId)
        {
            ResponseDTO res = new();

            try
            {
                var db = _orderRepo.GetDbContext();

                var order = await db.Set<Order>()
                    .Include(x => x.Client)
                    .Include(x => x.OrderDetails)
                        .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                    .FirstOrDefaultAsync(x => x.Id == orderId);

                if (order == null)
                {
                    res.IsSucess = false;
                    res.Message = "Không tìm thấy đơn hàng.";
                    return res;
                }

                res.IsSucess = true;
                res.Data = new
                {
                    order.Id,
                    order.OrderCode,
                    order.OrderDate,
                    Status = order.Status.ToString(),

                    Customer = new
                    {
                        order.FullName,
                        order.Phone
                    },

                    Items = order.OrderDetails.Select(d => new
                    {
                        Product = d.ProductVariant?.Product?.Name,
                        Gram = d.ProductVariant?.Gram,
                        d.AddonId,
                        d.Price,
                        d.Quantity
                    }),

                    order.TotalPrice
                };
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetOrdersAsync(string? keyword, string sort, string type, int pageNumber, int pageSize)
        {
            ResponseDTO res = new();

            try
            {
                var db = _orderRepo.GetDbContext();

                // 👉 base query
                var query = db.Set<Order>()
                    .Include(x => x.Client)
                    .Include(x => x.OrderDetails)
                    .Where(x => !x.IsDeleted)
                    .AsQueryable();

                // ================= SEARCH =================
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.OrderCode.ToString().Contains(keyword) ||
                        x.Client.FullName.Contains(keyword) ||
                        x.Client.Email.Contains(keyword));
                }

                // ================= FILTER TYPE =================
                if (!string.IsNullOrEmpty(type) && type != "all")
                {
                    query = type switch
                    {
                        // 👉 chỉ mua trà
                        "tea" => query.Where(o =>
                            o.OrderDetails.Any(d => d.ProductVariantId != null) &&
                            !o.OrderDetails.Any(d => d.AddonId != null)),

                        // 👉 chỉ mua thiết kế
                        "addon" => query.Where(o =>
                            !o.OrderDetails.Any(d => d.ProductVariantId != null) &&
                            o.OrderDetails.Any(d => d.AddonId != null)),

                        // 👉 mua cả 2
                        "both" => query.Where(o =>
                            o.OrderDetails.Any(d => d.ProductVariantId != null) &&
                            o.OrderDetails.Any(d => d.AddonId != null)),

                        _ => query
                    };
                }

                // ================= SORT =================
                query = sort switch
                {
                    "oldest" => query.OrderBy(x => x.OrderDate),
                    "price_asc" => query.OrderBy(x => x.TotalPrice),
                    "price_desc" => query.OrderByDescending(x => x.TotalPrice),
                    _ => query.OrderByDescending(x => x.OrderDate)
                };

                var totalItems = await query.CountAsync();

                // ================= DATA =================
                var data = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.OrderCode,
                        CustomerName = x.Client.FullName,
                        x.Client.Email,

                        // 🔥 TYPE (QUAN TRỌNG)
                        Type =
                            x.OrderDetails.Any(d => d.ProductVariantId != null) &&
                            !x.OrderDetails.Any(d => d.AddonId != null) ? "Tea"
                          : !x.OrderDetails.Any(d => d.ProductVariantId != null) &&
                            x.OrderDetails.Any(d => d.AddonId != null) ? "Addon"
                          : "Both",

                        x.TotalPrice,
                        Status = x.Status.ToString(),
                        x.OrderDate
                    })
                    .ToListAsync();

                res.IsSucess = true;
                res.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                    Items = data
                };
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.Message = ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetOrderStatsAsync()
        {
            ResponseDTO res = new();

            try
            {
                var db = _orderRepo.GetDbContext();

                var totalOrders = await db.Set<Order>()
                    .Where(x => !x.IsDeleted)
                    .CountAsync();

                var successOrders = await db.Set<Order>()
                    .Where(x => x.Status == OrderStatus.Paid && !x.IsDeleted)
                    .CountAsync();

                res.IsSucess = true;
                res.Data = new
                {
                    TotalOrders = totalOrders,
                    SuccessOrders = successOrders
                };
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.Message = ex.Message;
            }

            return res;
        }
    }
}
