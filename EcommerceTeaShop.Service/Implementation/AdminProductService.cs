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
using System.Text.Json;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Implementation
{
    public class AdminProductService : IAdminProductService
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Image> _imageRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinary;
        private readonly IGenericRepository<ProductVariant> _variantRepo;

        public AdminProductService(
            IGenericRepository<Product> productRepo,
            IGenericRepository<Image> imageRepo,
            IGenericRepository<Category> categoryRepo,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinary,
            IGenericRepository<ProductVariant> variantRepo)
        {
            _productRepo = productRepo;
            _imageRepo = imageRepo;
            _categoryRepo = categoryRepo;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
            _variantRepo = variantRepo;
        }

        public async Task<ResponseDTO> CreateProductAsync(CreateProductDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                var category = await _categoryRepo.GetById(dto.CategoryId);

                if (category == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Category không tồn tại.";
                    return res;
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                   
                    CategoryId = dto.CategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _productRepo.Insert(product);

                var variants = JsonSerializer.Deserialize<List<CreateProductVariantDTO>>(
           dto.Variants,
           new JsonSerializerOptions
           {
               PropertyNameCaseInsensitive = true
           });

                if (variants == null || !variants.Any())
                {
                    res.IsSucess = false;
                    res.Message = "Product phải có ít nhất 1 variant.";
                    return res;
                }

                foreach (var v in variants)
                {
                    var variant = new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Gram = v.Gram,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _variantRepo.Insert(variant);
                }

                if (dto.Images != null)
                {
                    int index = 0;

                    foreach (var file in dto.Images)
                    {
                        var url = await _cloudinary.UploadImageAsync(file, "tea-products");

                        await _imageRepo.Insert(new Image
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = url,
                            IsMain = index == 0,
                            CreatedAt = DateTime.UtcNow
                        });

                        index++;
                    }
                }

                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                res.Message = "Tạo product thành công.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> DeleteProductAsync(Guid id)
        {
            ResponseDTO res = new();

            try
            {
                var product = await _productRepo.GetById(id);

                if (product == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Không tìm thấy sản phẩm.";
                    return res;
                }

                product.IsDeleted = true;
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepo.Update(product);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;
                res.Message = "Xóa sản phẩm thành công.";
                res.Data = new
                {
                    ProductId = product.Id,
                    product.Name
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

        public async Task<ResponseDTO> DeleteProductImageAsync(Guid imageId)
        {
            ResponseDTO res = new();

            var image = await _imageRepo.GetById(imageId);

            if (image == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                return res;
            }

            await _imageRepo.Delete(image);
            await _unitOfWork.SaveChangeAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;

            return res;
        }

        public async Task<ResponseDTO> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            ResponseDTO res = new();

            var db = _productRepo.GetDbContext();

            var query = db.Set<Product>()
                .Include(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.Variants)
                .Where(x => !x.IsDeleted);

            var totalItems = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = data.Select(p => new ReadProductDTO
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Variants.Any() ? p.Variants.Min(v => v.Price) : 0,
                StockQuantity = p.Variants.Any() ? p.Variants.Sum(v => v.StockQuantity) : 0,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive,
                Images = p.Images.Select(i => i.ImageUrl).ToList()
            });

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            res.Data = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = mapped
            };

            return res;
        }

        public async Task<ResponseDTO> GetProductDetailAsync(Guid id)
        {
            ResponseDTO res = new();

            try
            {
                var db = _productRepo.GetDbContext();

                var product = await db.Set<Product>()
                    .Include(x => x.Category)
                    .Include(x => x.Images)
                    .Include(x => x.Variants)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (product == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Không tìm thấy sản phẩm.";
                    return res;
                }

                var data = new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    Variants = product.Variants.Select(v => new ProductVariantDTO
                    {
                        Id = v.Id,
                        Gram = v.Gram,
                        Price = v.Price,
                        StockQuantity = v.StockQuantity
                    }),
                    Category = product.Category.Name,
                    product.IsActive,
                    Images = product.Images.Select(x => new
                    {
                        x.Id,
                        x.ImageUrl,
                        x.IsMain
                    })
                };

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                res.Data = data;
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> SetMainImageAsync(Guid imageId)
        {
            ResponseDTO res = new();

            var image = await _imageRepo.GetById(imageId);

            if (image == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                return res;
            }

            var db = _imageRepo.GetDbContext();

            var images = await db.Set<Image>()
                .Where(x => x.ProductId == image.ProductId)
                .ToListAsync();

            foreach (var img in images)
                img.IsMain = false;

            image.IsMain = true;

            await _unitOfWork.SaveChangeAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            res.Message = "Đặt ảnh chính thành công.";
            res.Data = new
            {
                ImageId = image.Id,
                ProductId = image.ProductId
            };
            return res;
        }

        public async Task<ResponseDTO> UpdateProductAsync(Guid id, UpdateProductDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                var product = await _productRepo.GetById(id);

                if (product == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Không tìm thấy sản phẩm.";
                    return res;
                }

                if (dto.Name != null)
                    product.Name = dto.Name;

                if (dto.Description != null)
                    product.Description = dto.Description;

               

                if (dto.CategoryId != null)
                    product.CategoryId = dto.CategoryId.Value;

                if (dto.IsActive != null)
                    product.IsActive = dto.IsActive.Value;

                product.UpdatedAt = DateTime.UtcNow;

                await _productRepo.Update(product);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                res.Message = "Cập nhật sản phẩm thành công.";
                res.Data = new
                {
                    ProductId = product.Id,
                    product.Name
                    
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

        public async Task<ResponseDTO> UpdateVariantAsync(Guid variantId, UpdateVariantDTO dto)
        {
            ResponseDTO res = new();

            var variant = await _variantRepo.GetById(variantId);

            if (variant == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                res.Message = "Variant không tồn tại.";
                return res;
            }

            variant.Price = dto.Price;
            variant.StockQuantity = dto.StockQuantity;
            variant.UpdatedAt = DateTime.UtcNow;

            await _variantRepo.Update(variant);
            await _unitOfWork.SaveChangeAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
            res.Message = "Cập nhật variant thành công.";

            return res;
        }
    }
}
