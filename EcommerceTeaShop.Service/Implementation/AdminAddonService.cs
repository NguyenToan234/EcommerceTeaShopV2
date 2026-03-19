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
    public class AdminAddonService : IAdminAddonService
    {
        private readonly IGenericRepository<Addon> _addonRepo;
        private readonly IGenericRepository<ProductAddon> _productAddonRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinary;

        public AdminAddonService(
            IGenericRepository<Addon> addonRepo,
            IGenericRepository<ProductAddon> productAddonRepo,
            IGenericRepository<Product> productRepo,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinary)
        {
            _addonRepo = addonRepo;
            _productAddonRepo = productAddonRepo;
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseDTO> AssignAddonToProduct(Guid productId, List<Guid> addonIds)
        {
            ResponseDTO res = new();

            try
            {
                if (addonIds == null || !addonIds.Any())
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.INVALID_DATA;
                    res.Message = "Danh sách addon không hợp lệ.";
                    return res;
                }

                var product = await _productRepo.GetById(productId);

                if (product == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Sản phẩm không tồn tại.";
                    return res;
                }

                // 🔥 lấy danh sách addon đã gán
                var existingAddonIds = await _productAddonRepo
                    .AsQueryable()
                    .Where(x => x.ProductId == productId)
                    .Select(x => x.AddonId)
                    .ToListAsync();

                foreach (var addonId in addonIds)
                {
                    // ❌ tránh duplicate
                    if (existingAddonIds.Contains(addonId))
                        continue;

                    // ❌ check addon tồn tại
                    var addon = await _addonRepo.GetById(addonId);

                    if (addon == null || addon.IsDeleted || !addon.IsActive)
                        continue;

                    await _productAddonRepo.Insert(new ProductAddon
                    {
                        ProductId = productId,
                        AddonId = addonId
                    });
                }

                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                res.Message = "Gán thiết kế cho sản phẩm thành công.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> CreateAddonAsync(CreateAddonDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                string imageUrl = null;

                if (dto.Image != null)
                {
                    imageUrl = await _cloudinary.UploadImageAsync(dto.Image, "tea-addon");
                }

                var addon = new Addon
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = imageUrl,
                    Price = dto.Price,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _addonRepo.Insert(addon);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                res.Message = "Tạo thiết kế thành công.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> DeleteAddonAsync(Guid id)
        {
            ResponseDTO res = new();

            try
            {
                var addon = await _addonRepo.GetById(id);

                if (addon == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Thiết kế không tồn tại.";
                    return res;
                }

                // 🔥 xóa liên kết trước
                var productAddons = await _productAddonRepo
                    .AsQueryable()
                    .Where(x => x.AddonId == id)
                    .ToListAsync();

                if (productAddons.Any())
                {
                    await _productAddonRepo.DeleteRange(productAddons);
                }

                // 🔥 soft delete addon
                addon.IsDeleted = true;
                addon.UpdatedAt = DateTime.UtcNow;

                await _addonRepo.Update(addon);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                res.Message = "Xóa thiết kế thành công.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetAddonByProductAsync(Guid productId)
        {
            ResponseDTO res = new();

            try
            {
                var product = await _productRepo.GetById(productId);

                if (product == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Sản phẩm không tồn tại.";
                    return res;
                }

                var db = _productAddonRepo.GetDbContext();

                var addons = await db.Set<ProductAddon>()
                    .Include(x => x.Addon)
                    .Where(x => x.ProductId == productId
                             && !x.Addon.IsDeleted
                             && x.Addon.IsActive)
                    .Select(x => new
                    {
                        x.Addon.Id,
                        x.Addon.Name,
                        x.Addon.Description,
                        x.Addon.ImageUrl,
                        x.Addon.Price
                    })
                    .Distinct()
                    .ToListAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = addons;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> GetAllAddonAsync(int pageNumber, int pageSize)
        {
            ResponseDTO res = new();

            try
            {
                var db = _addonRepo.GetDbContext();

                var query = db.Set<Addon>()
                    .Where(x => !x.IsDeleted);

                var totalItems = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = data
                };
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> UpdateAddonAsync(Guid id, UpdateAddonDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                var addon = await _addonRepo.GetById(id);

                if (addon == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Thiết kế không tồn tại.";
                    return res;
                }

                if (dto.Name != null)
                    addon.Name = dto.Name;

                if (dto.Description != null)
                    addon.Description = dto.Description;

                if (dto.Price != null)
                    addon.Price = dto.Price.Value;

                if (dto.IsActive != null)
                    addon.IsActive = dto.IsActive.Value;

                if (dto.Image != null)
                {
                    var url = await _cloudinary.UploadImageAsync(dto.Image, "tea-addon");
                    addon.ImageUrl = url;
                }

                addon.UpdatedAt = DateTime.UtcNow;

                await _addonRepo.Update(addon);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                res.Message = "Cập nhật thiết kế thành công.";
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
