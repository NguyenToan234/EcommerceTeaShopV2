using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
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
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<ProductVariant> _variantRepo;

        public AdminDashboardService(
            IGenericRepository<Order> orderRepo,
            IGenericRepository<ProductVariant> variantRepo)
        {
            _orderRepo = orderRepo;
            _variantRepo = variantRepo;
        }

        public async Task<ResponseDTO> GetDashboardAsync()
        {
            ResponseDTO res = new();

            var db = _orderRepo.GetDbContext();

            // ✅ chỉ lấy đơn đã thanh toán thành công
            var paidOrders = db.Set<Order>()
                .Where(x => !x.IsDeleted && x.Status == OrderStatus.Paid);

            // ✅ DOANH THU (bao gồm trà + addon + cả 2)
            var totalRevenue = await paidOrders
                .SumAsync(x => (decimal?)x.TotalPrice) ?? 0;

            // ✅ đơn đang xử lý
            var processingOrders = await db.Set<Order>()
        .Where(x => !x.IsDeleted &&
            (x.Status == OrderStatus.Pending || x.Status == OrderStatus.Paid))
        .CountAsync();

            // ✅ sản phẩm sắp hết
            var lowStock = await db.Set<ProductVariant>()
                .Where(x => !x.IsDeleted && x.StockQuantity < 10)
                .CountAsync();

            res.IsSucess = true;
            res.Data = new
            {
                SalesRevenue = totalRevenue,
                ProcessingOrders = processingOrders,
                LowStockProducts = lowStock
            };

            return res;
        }

        public async Task<ResponseDTO> GetRecentTransactionsAsync()
        {
            ResponseDTO res = new();

            var db = _orderRepo.GetDbContext();

            var data = await db.Set<Order>()
                .Include(x => x.Client)
                .Where(x => !x.IsDeleted && x.Status == OrderStatus.Paid)
                .OrderByDescending(x => x.OrderDate)
                .Take(5)
                .Select(x => new
                {
                    OrderCode = x.OrderCode,
                    Customer = x.Client.FullName,
                    Date = x.OrderDate,
                    Amount = x.TotalPrice,
                    Status = x.Status
                })
                .ToListAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            res.Data = data;

            return res;
        }

        public async Task<ResponseDTO> GetRevenueChartAsync(string type)
        {
            ResponseDTO res = new();

            var db = _orderRepo.GetDbContext();

            var orders = db.Set<Order>()
                .Where(x => !x.IsDeleted &&
                            x.Status == OrderStatus.Paid);

            var now = DateTime.UtcNow;

            List<object> result;

            if (type == "week")
            {
                var start = now.AddDays(-7);

                result = await orders
                    .Where(x => x.OrderDate >= start)
                    .GroupBy(x => x.OrderDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.TotalPrice)
                    })
                    .ToListAsync<object>();
            }
            else
            {
                var start = now.AddMonths(-6);

                result = await orders
                    .Where(x => x.OrderDate >= start)
                    .GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month })
                    .Select(g => new
                    {
                        Month = g.Key.Month,
                        Revenue = g.Sum(x => x.TotalPrice)
                    })
                    .ToListAsync<object>();
            }

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            res.Data = result;

            return res;
        }
    }
}
