using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAddressService
    {
        Task<ResponseDTO> AddAddressAsync(Guid clientId, CreateAddressDTO dto);

        Task<ResponseDTO> GetMyAddressesAsync(Guid clientId);

        Task<ResponseDTO> DeleteAddressAsync(Guid clientId, Guid addressId);
    }
}
