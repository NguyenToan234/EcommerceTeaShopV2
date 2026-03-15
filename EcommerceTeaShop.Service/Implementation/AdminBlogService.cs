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
    public class AdminBlogService : IAdminBlogService
    {
        private readonly IGenericRepository<Blog> _blogRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinary;

        public AdminBlogService(
            IGenericRepository<Blog> blogRepo,
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinary)
        {
            _blogRepo = blogRepo;
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinary;
        }

        public async Task<ResponseDTO> CreateBlogAsync(CreateBlogDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                string thumbnailUrl = null;

                if (dto.Thumbnail != null)
                {
                    thumbnailUrl = await _cloudinary.UploadImageAsync(dto.Thumbnail, "tea-blog");
                }

                var blog = new Blog
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Content = dto.Content,
                    Thumbnail = thumbnailUrl,
                    PublishDate = dto.PublishDate.ToUniversalTime(),
                    IsPublished = dto.IsPublished,
                    CreatedAt = DateTime.UtcNow
                };

                await _blogRepo.Insert(blog);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.INSERT_SUCESSFULLY;
                res.Message = "Tạo blog thành công.";
            }
            catch (Exception ex)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.EXCEPTION;
                res.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return res;
        }

        public async Task<ResponseDTO> DeleteBlogAsync(Guid id)
        {
            ResponseDTO res = new();

            var blog = await _blogRepo.GetById(id);

            if (blog == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                return res;
            }

            blog.IsDeleted = true;
            blog.UpdatedAt = DateTime.UtcNow;

            await _blogRepo.Update(blog);
            await _unitOfWork.SaveChangeAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.DELETE_SUCESSFULLY;

            return res;
        }

        public async Task<ResponseDTO> GetAllBlogsAsync(int pageNumber, int pageSize)
        {
            ResponseDTO res = new();

            var db = _blogRepo.GetDbContext();

            var query = db.Set<Blog>()
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

            return res;
        }

        public async Task<ResponseDTO> UpdateBlogAsync(Guid id, UpdateBlogDTO dto)
        {
            ResponseDTO res = new();

            try
            {
                var blog = await _blogRepo.GetById(id);

                if (blog == null)
                {
                    res.IsSucess = false;
                    res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    res.Message = "Blog không tồn tại.";
                    return res;
                }

                if (dto.Title != null)
                    blog.Title = dto.Title;

                if (dto.Content != null)
                    blog.Content = dto.Content;

                if (dto.IsPublished != null)
                    blog.IsPublished = dto.IsPublished.Value;

                if (dto.Thumbnail != null)
                {
                    var url = await _cloudinary.UploadImageAsync(dto.Thumbnail, "tea-blog");
                    blog.Thumbnail = url;
                }

                blog.UpdatedAt = DateTime.UtcNow;

                await _blogRepo.Update(blog);
                await _unitOfWork.SaveChangeAsync();

                res.IsSucess = true;
                res.BusinessCode = BusinessCode.UPDATE_SUCESSFULLY;
                res.Message = "Cập nhật blog thành công.";
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
