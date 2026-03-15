using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface ICartService
    {
        Task<ResponseDTO> AddToCartAsync(Guid clientId, AddToCartDTO dto);

        Task<ResponseDTO> GetCartAsync(Guid clientId);

        Task<ResponseDTO> UpdateQuantityAsync(Guid clientId, UpdateCartItemDTO dto);

        Task<ResponseDTO> RemoveItemAsync(Guid clientId, Guid cartItemId);
        Task<ResponseDTO> ApplyCouponAsync(Guid clientId, string code);

        Task<ResponseDTO> RemoveCouponAsync(Guid clientId);
    }
}
