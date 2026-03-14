using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminProductService
    {
        Task<ResponseDTO> CreateProductAsync(CreateProductDTO dto);

        Task<ResponseDTO> UpdateProductAsync(Guid id, UpdateProductDTO dto);

        Task<ResponseDTO> DeleteProductAsync(Guid id);

        Task<ResponseDTO> GetProductDetailAsync(Guid id);

        Task<ResponseDTO> GetAllProductsAsync(int pageNumber, int pageSize);

        Task<ResponseDTO> DeleteProductImageAsync(Guid imageId);

        Task<ResponseDTO> SetMainImageAsync(Guid imageId);
        Task<ResponseDTO> UpdateVariantAsync(Guid variantId, UpdateVariantDTO dto);
    }
}
