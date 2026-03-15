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
    public class BlogService : IBlogService
    {
        private readonly IGenericRepository<Blog> _blogRepo;

        public BlogService(IGenericRepository<Blog> blogRepo)
        {
            _blogRepo = blogRepo;
        }

        public async Task<ResponseDTO> GetBlogDetailAsync(Guid id)
        {
            ResponseDTO res = new();

            var db = _blogRepo.GetDbContext();

            var blog = await db.Set<Blog>()
                .Where(x => x.Id == id && !x.IsDeleted && x.IsPublished)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Content,
                    x.Thumbnail,
                    x.PublishDate
                })
                .FirstOrDefaultAsync();

            if (blog == null)
            {
                res.IsSucess = false;
                res.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                res.Message = "Blog không tồn tại.";
                return res;
            }

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            res.Data = blog;

            return res;
        }
    
        

        public async Task<ResponseDTO> GetBlogsAsync(int pageNumber, int pageSize)
        {
            ResponseDTO res = new();

            var db = _blogRepo.GetDbContext();

            var query = db.Set<Blog>()
                .Where(x => !x.IsDeleted && x.IsPublished);

            var totalItems = await query.CountAsync();

            var blogs = await query
                .OrderByDescending(x => x.PublishDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Thumbnail,
                    x.PublishDate,
                    ShortContent = x.Content.Substring(0,
                        x.Content.Length > 120 ? 120 : x.Content.Length)
                })
                .ToListAsync();

            res.IsSucess = true;
            res.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
            res.Data = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = blogs
            };

            return res;
        }
    }
}
