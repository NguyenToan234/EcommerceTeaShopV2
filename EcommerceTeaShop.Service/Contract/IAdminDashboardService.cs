using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminDashboardService
    {
        Task<ResponseDTO> GetDashboardAsync();

        Task<ResponseDTO> GetRevenueChartAsync(string type); // week / month

        Task<ResponseDTO> GetRecentTransactionsAsync();
    }
}
