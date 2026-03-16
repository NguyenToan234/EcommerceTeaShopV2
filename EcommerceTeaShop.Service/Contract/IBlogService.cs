using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IBlogService
    {
        Task<ResponseDTO> GetBlogsAsync(int pageNumber, int pageSize);

        Task<ResponseDTO> GetBlogDetailAsync(Guid id);
    }
}
