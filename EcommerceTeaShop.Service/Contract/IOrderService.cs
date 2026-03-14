using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IOrderService
    {
        Task<ResponseDTO> CheckoutAsync(Guid clientId, Guid addressId);
            Task<ResponseDTO> GetMyOrdersAsync(Guid clientId);

        Task<ResponseDTO> GetOrderByCodeAsync(long orderCode);

        Task ConfirmPayment(long orderCode);

    }
}
