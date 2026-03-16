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
    public class AdminBannerService : IAdminBannerService
    {
        private readonly IGenericRepository<Banner> _bannerRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinary;

        public AdminBannerService(
            IGenericRepository<Banner> bannerRepo,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinary)
        {
            _bannerRepo = bannerRepo;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseDTO> CreateBannerAsync(CreateBannerDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                if (dto.ImageUrl == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    res.Message = "Banner image is required.";
                    return res;
                }
                if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    res.Message = "StartDate và EndDate là bắt buộc.";
                    return res;
                }
                if (dto.StartDate >= dto.EndDate)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    res.Message = "Ngày bắt đầu phải trước Ngày kết thúc.";
                    return res;
                }

                var imageUrl = await _cloudinary.UploadImageAsync(dto.ImageUrl, "tea-banner");

                var banner = new Banner
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = imageUrl,
                    RedirectUrl = dto.RedirectUrl,
                    DisplayOrder = dto.DisplayOrder,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    CreatedAt = DateTime.UtcNow
                };
                var db = _bannerRepo.GetDbContext();

                var existed = await db.Set<Banner>()
                    .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.DisplayOrder == dto.DisplayOrder &&
                    x.StartDate <= dto.EndDate.Value &&
                    x.EndDate >= dto.StartDate.Value);

                if (existed)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.VALIDATION_FAILED;
                    res.Message = "DisplayOrder already exists in this time range.";
                    return res;
                }

                await _bannerRepo.Insert(banner);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                res.Message = "Banner created successfully.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> DeleteBannerAsync(Guid id)
        {
            ResponseDTO res = new();

            try
            {
                var banner = await _bannerRepo.GetById(id);

                if (banner == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Banner not found.";
                    return res;
                }

                await _bannerRepo.Delete(banner);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                res.Message = "Banner deleted.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetActiveBannersAsync()
        {
            ResponseDTO res = new();

            try
            {
                var db = _bannerRepo.GetDbContext();
                var now = DateTime.UtcNow;

                // auto disable expired banners
                var expired = await db.Set<Banner>()
                    .Where(x => x.IsActive && x.EndDate < now)
                    .ToListAsync();

                foreach (var item in expired)
                    item.IsActive = false;

                if (expired.Any())
                    await _unitOfWork.SaveChangeAsync();

                var banners = await db.Set<Banner>()
                    .Where(x => !x.IsDeleted &&
                           x.IsActive &&
                           x.StartDate <= now &&
                           x.EndDate >= now)
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.StartDate)
                    .ThenBy(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.ImageUrl,
                        x.RedirectUrl
                    })
                    .ToListAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = banners;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetAllBannersAsync()
        {
            ResponseDTO res = new();

            try
            {
                var db = _bannerRepo.GetDbContext();

                var banners = await db.Set<Banner>()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = banners;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> UpdateBannerAsync(Guid id, UpdateBannerDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                var banner = await _bannerRepo.GetById(id);

                if (banner == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Banner not found.";
                    return res;
                }

                if (dto.ImageUrl != null)
                {
                    var url = await _cloudinary.UploadImageAsync(dto.ImageUrl, "tea-banner");
                    banner.ImageUrl = url;
                }

                if (dto.StartDate.HasValue && dto.EndDate.HasValue)
                {
                    if (dto.StartDate.Value >= dto.EndDate.Value)
                    {
                        res.IsSucess = false;
                        res.BusinessCode = BusinessCode.VALIDATION_FAILED;
                        res.Message = "Ngày bắt đầu phải trước ngày kết thúc.";
                        return res;
                    }
                }

                banner.RedirectUrl = dto.RedirectUrl;
                banner.DisplayOrder = dto.DisplayOrder;
                banner.StartDate = dto.StartDate;
                banner.EndDate = dto.EndDate;
                banner.IsActive = dto.IsActive;
                banner.UpdatedAt = DateTime.UtcNow;

                await _bannerRepo.Update(banner);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                res.Message = "Banner updated successfully.";
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
