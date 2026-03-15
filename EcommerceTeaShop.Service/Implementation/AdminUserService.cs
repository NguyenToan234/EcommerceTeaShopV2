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
    public class AdminUserService : IAdminUserService
    {
        private readonly IGenericRepository<Client> _clientRepo;
        private readonly IGenericRepository<Rating> _ratingRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AdminUserService(
            IGenericRepository<Client> clientRepo,
            IGenericRepository<Rating> ratingRepo,
            IUnitOfWork unitOfWork,
            IEmailService emailService)
        {
            _clientRepo = clientRepo;
            _ratingRepo = ratingRepo;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ResponseDTO> ApproveReviewAsync(Guid id)
        {
            ResponseDTO res = new();

            var review = await _ratingRepo.GetById(id);

            if (review == null)
            {
                res.IsSucess = false;
                res.Message = "Review không tồn tại.";
                return res;
            }

            review.IsApproved = true;
            review.UpdatedAt = DateTime.UtcNow;

            await _ratingRepo.Update(review);
            await _unitOfWork.SaveChangeAsync();

            res.IsSucess = true;
            res.Message = "Đã duyệt đánh giá.";

            return res;
        }

        public async Task<ResponseDTO> BlockUserAsync(Guid id)
        {
            ResponseDTO res = new();

            var user = await _clientRepo.GetById(id);

            if (user == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                res.Message = "User không tồn tại.";
                return res;
            }
            if (user.Role == "Admin")
            {
                return new ResponseDTO
                {
                    IsSucess = false,
                    Message = "Không thể khóa tài khoản Admin."
                };
            }

            user.Status = "Blocked";
            user.UpdatedAt = DateTime.UtcNow;

            await _clientRepo.Update(user);
            await _unitOfWork.SaveChangeAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Tài khoản TeaVault đã bị khóa",
                $@"
Xin chào {user.FullName},

Tài khoản TeaVault của bạn đã bị khóa bởi quản trị viên.

Nếu bạn cho rằng đây là nhầm lẫn, vui lòng liên hệ hỗ trợ.

TeaVault System
"
            );

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            res.Message = "Đã khóa tài khoản và gửi email.";

            return res;
        }

        public async Task<ResponseDTO> GetReviewsAsync(string keyword,int pageNumber,int pageSize)
        {
            ResponseDTO res = new();

            var db = _ratingRepo.GetDbContext();

            var query = db.Set<Rating>()
                .Include(x => x.Product)
                .Include(x => x.Client)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.Product.Name.Contains(keyword) ||
                    x.Client.FullName.Contains(keyword) ||
                     (x.Comment != null && x.Comment.Contains(keyword)));
            }

            var totalItems = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    ReviewId = x.Id,
                    ProductName = x.Product.Name,
                    UserName = x.Client.FullName,
                    Comment = x.Comment,
                    Star = x.Star,
                    Status = x.IsApproved ? "Approved" : "Pending",
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            res.IsSucess = true;

            res.Data = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = reviews
            };

            return res;
        }

        public async Task<ResponseDTO> GetUserDetailAsync(Guid id)
        {
            ResponseDTO res = new();

            var db = _clientRepo.GetDbContext();

            var user = await db.Set<Client>()
                .Include(x => x.Orders)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (user == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                res.Message = "User không tồn tại.";
                return res;
            }

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;

            res.Data = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                user.Status,
                user.CreatedAt,
                TotalOrders = user.Orders.Count
            };

            return res;
        }

        public async Task<ResponseDTO> GetUserReviewStatsAsync()
        {
            ResponseDTO res = new();

            var db = _clientRepo.GetDbContext();

            var totalUsers = await db.Set<Client>()
                .Where(x => x.Role == "User" && !x.IsDeleted)
                .CountAsync();

            var pendingReviews = await db.Set<Rating>()
                .Where(x => !x.IsApproved)
                .CountAsync();

            var avgRating = await db.Set<Rating>()
                .Where(x => x.IsApproved)
                .AverageAsync(x => (double?)x.Star) ?? 0;

            res.IsSucess = true;

            res.Data = new
            {
                TotalUsers = totalUsers,
                PendingReviews = pendingReviews,
                AvgRating = Math.Round(avgRating, 1)
            };

            return res;
        }

        public async Task<ResponseDTO> GetUsersAsync(string keyword, int pageNumber, int pageSize)
        {
            ResponseDTO res = new();

            var db = _clientRepo.GetDbContext();

            var query = db.Set<Client>()
                .Include(x => x.Orders)
                .Where(x => x.Role == "User" && !x.IsDeleted);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.FullName.Contains(keyword) ||
                    x.Email.Contains(keyword));
            }

            var totalItems = await query.CountAsync();

            var users = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.FullName,
                    Email = x.Email,
                    TotalOrders = x.Orders.Count,
                    LifetimeValue = x.Orders.Sum(o => (decimal?)o.TotalPrice) ?? 0,
                    Status = x.Status
                })
                .ToListAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;

            res.Data = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = users
            };

            return res;
        }

        public async Task<ResponseDTO> UnblockUserAsync(Guid id)
        {
            ResponseDTO res = new();

            var user = await _clientRepo.GetById(id);

            if (user == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                res.Message = "User không tồn tại.";
                return res;
            }

            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;

            await _clientRepo.Update(user);
            await _unitOfWork.SaveChangeAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "Tài khoản TeaVault đã được mở khóa",
                $@"
Xin chào {user.FullName},

Tài khoản TeaVault của bạn đã được mở khóa.

Bạn có thể đăng nhập và tiếp tục mua sắm.

TeaVault System
"
            );

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            res.Message = "Đã mở khóa tài khoản và gửi email.";

            return res;
        
    }
    }
}
