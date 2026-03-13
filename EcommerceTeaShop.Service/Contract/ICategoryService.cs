using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface ICategoryService
    {
        Task<ResponseDTO> GetAllCategoriesAsync(int pageNumber, int pageSize);

        Task<ResponseDTO> GetCategoryByIdAsync(Guid id);

        Task<ResponseDTO> CreateCategoryAsync(CreateCategoryDTO request);

        Task<ResponseDTO> UpdateCategoryAsync(Guid id, UpdateCategoryDTO request);

        Task<ResponseDTO> DeleteCategoryAsync(Guid id);

        Task<ResponseDTO> SearchCategoriesAsync(string keyword, int pageNumber, int pageSize);
    }
}
