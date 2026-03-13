using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IProductService
    {
        Task<ResponseDTO> GetProductsAsync(int pageNumber, int pageSize);

        Task<ResponseDTO> GetProductByIdAsync(Guid id);

        Task<ResponseDTO> SearchProductsAsync(string keyword, int pageNumber, int pageSize);

        Task<ResponseDTO> GetProductsByCategoryAsync(Guid categoryId, int pageNumber, int pageSize);

        Task<ResponseDTO> CreateProductAsync(CreateProductDTO dto);

        Task<ResponseDTO> UpdateProductAsync(Guid id, UpdateProductDTO dto);

        Task<ResponseDTO> DeleteProductAsync(Guid id);
    }
}
