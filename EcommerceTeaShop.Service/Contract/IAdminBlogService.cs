using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminBlogService
    {
        Task<ResponseDTO> CreateBlogAsync(CreateBlogDTO dto);
        Task<ResponseDTO> UpdateBlogAsync(Guid id, UpdateBlogDTO dto);
        Task<ResponseDTO> GetAllBlogsAsync(int pageNumber, int pageSize);
        Task<ResponseDTO> DeleteBlogAsync(Guid id);

    }
}
