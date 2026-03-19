using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminAddonService
    {
        Task<ResponseDTO> CreateAddonAsync(CreateAddonDTO dto);
        Task<ResponseDTO> GetAllAddonAsync(int pageNumber, int pageSize);
        Task<ResponseDTO> UpdateAddonAsync(Guid id, UpdateAddonDTO dto);
        Task<ResponseDTO> DeleteAddonAsync(Guid id);
        Task<ResponseDTO> AssignAddonToProduct(Guid productId, List<Guid> addonIds);
        Task<ResponseDTO> GetAddonByProductAsync(Guid productId);
    }

}
