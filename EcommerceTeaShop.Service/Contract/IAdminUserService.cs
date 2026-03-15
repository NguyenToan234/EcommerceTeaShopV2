using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IAdminUserService
    {
        Task<ResponseDTO> GetUsersAsync(string keyword, int pageNumber, int pageSize);

        Task<ResponseDTO> GetUserDetailAsync(Guid id);

        Task<ResponseDTO> BlockUserAsync(Guid id);

        Task<ResponseDTO> UnblockUserAsync(Guid id);
        Task<ResponseDTO> GetUserReviewStatsAsync();
        Task<ResponseDTO> GetReviewsAsync(
    string keyword,
    int pageNumber,
    int pageSize);
        Task<ResponseDTO> ApproveReviewAsync(Guid id);
    }
}
