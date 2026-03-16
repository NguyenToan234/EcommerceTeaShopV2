using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminBannerService
    {
        Task<ResponseDTO> CreateBannerAsync(CreateBannerDTO dto);

        Task<ResponseDTO> UpdateBannerAsync(Guid id, UpdateBannerDTO dto);

        Task<ResponseDTO> DeleteBannerAsync(Guid id);

        Task<ResponseDTO> GetAllBannersAsync();

        Task<ResponseDTO> GetActiveBannersAsync();

    }
}
