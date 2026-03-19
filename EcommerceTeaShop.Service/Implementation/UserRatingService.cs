using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
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
                    .AnyAsync(o =>
                        o.ClientId == clientId &&
                        o.Status == Repository.Models.EnumModels.OrderStatus.Paid &&
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
                        x.ProductId == dto.ProductId);

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

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = ratings;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }
    }

}
