using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminOrderService
    {
        Task<ResponseDTO> GetOrdersAsync(string? keyword, string sort, string type, int pageNumber, int pageSize);
        Task<ResponseDTO> GetOrderDetailAsync(Guid orderId);
        Task<ResponseDTO> GetOrderStatsAsync();
    }
}
