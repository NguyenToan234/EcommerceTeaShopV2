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
    public class UserRatingService : IUserRatingService
    {
        private readonly IGenericRepository<Rating> _ratingRepo;
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public UserRatingService(
            IGenericRepository<Rating> ratingRepo,
            IGenericRepository<Order> orderRepo,
            IUnitOfWork unitOfWork)
        {
            _ratingRepo = ratingRepo;
            _orderRepo = orderRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> CreateRatingAsync(Guid clientId, CreateRatingDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                // ❌ Validate star
                if (dto.Star < 1 || dto.Star > 5)
                {
                    res.IsSucess = false;
                    res.Message = "Số sao phải từ 1 đến 5.";
                    return res;
                }

                var db = _orderRepo.GetDbContext();

                // 🔥 CHECK user đã mua sản phẩm chưa
                var hasPurchased = await db.Set<Order>()
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.ProductVariant)
                    .AnyAsync(o =>
                        o.ClientId == clientId &&
                        o.Status == OrderStatus.Paid &&
                        o.OrderDetails.Any(d => d.ProductVariant.ProductId == dto.ProductId)
                    );

                if (!hasPurchased)
                {
                    res.IsSucess = false;
                    res.Message = "Bạn chưa mua sản phẩm này.";
                    return res;
                }

                // 🔥 CHECK đã rating chưa (chống spam)
                var existed = await _ratingRepo.AsQueryable()
                       .AnyAsync(x =>
                           x.ClientId == clientId &&
                           x.ProductId == dto.ProductId &&
                           !x.IsDeleted);

                if (existed)
                {
                    res.IsSucess = false;
                    res.Message = "Bạn đã đánh giá sản phẩm này rồi.";
                    return res;
                }

                // 🔥 CREATE RATING
                var rating = new Rating
                {
                    Id = Guid.NewGuid(),
                    ProductId = dto.ProductId,
                    ClientId = clientId,
                    Star = dto.Star,
                    Comment = dto.Comment,
                    IsApproved = false, // 👈 cần admin duyệt
                    CreatedAt = DateTime.UtcNow
                };

                await _ratingRepo.Insert(rating);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                res.Message = "Gửi đánh giá thành công. Chờ admin duyệt.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetMyProductsForRatingAsync(Guid clientId)
        {
            ResponseDTO res = new();

            try
            {
                var db = _orderRepo.GetDbContext();

                // 🔥 Lấy tất cả sản phẩm đã mua (Paid)
                var purchasedProducts = await db.Set<Order>()
                    .Include(o => o.OrderDetails)
                        .ThenInclude(d => d.ProductVariant)
                        .ThenInclude(v => v.Product)
                    .Where(o => o.ClientId == clientId &&
                                o.Status == OrderStatus.Paid)
                    .SelectMany(o => o.OrderDetails)
                    .Select(d => new
                    {
                        ProductId = d.ProductVariant.Product.Id,
                        ProductName = d.ProductVariant.Product.Name
                    })
                    .Distinct()
                    .ToListAsync();

                // 🔥 Lấy sản phẩm đã rating
                var ratedProductIds = await _ratingRepo.AsQueryable()
                    .Where(x => x.ClientId == clientId && !x.IsDeleted)
                    .Select(x => x.ProductId)
                    .ToListAsync();

                // 🔥 Lọc ra chưa rating
                var result = purchasedProducts
                    .Where(p => !ratedProductIds.Contains(p.ProductId))
                    .ToList();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = result;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetProductRatingsAsync(Guid productId)
        {
            ResponseDTO res = new();

            try
            {
                var db = _ratingRepo.GetDbContext();

                var ratings = await db.Set<Rating>()
                    .Include(x => x.Client)
                    .Where(x => x.ProductId == productId
                             && x.IsApproved
                             && !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.Star,
                        x.Comment,
                        UserName = x.Client.FullName,
                        x.CreatedAt
                    })
                    .ToListAsync();

                // ⭐ AVG
                var avg = ratings.Any() ? ratings.Average(x => x.Star) : 0;

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = new
                {
                    AverageRating = Math.Round(avg, 1),
                    TotalReviews = ratings.Count,
                    Items = ratings
                };
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.Message;
            }

            return res;
        }
    }

}
