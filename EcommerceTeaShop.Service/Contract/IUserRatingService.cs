using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Contract
{
    public interface IUserRatingService
    {
        Task<ResponseDTO> CreateRatingAsync(Guid clientId, CreateRatingDTO dto);
        Task<ResponseDTO> GetProductRatingsAsync(Guid productId);
    }
}
