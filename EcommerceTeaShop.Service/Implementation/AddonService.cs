using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Service.Implementation
{
    public class AddonService : IAddonService
    {
        private readonly IGenericRepository<Addon> _addonRepository;

        public AddonService(IGenericRepository<Addon> addonRepository)
        {
            _addonRepository = addonRepository;
        }

        public async Task<ResponseDTO> GetAddonsAsync()
        {
            ResponseDTO response = new();

            var addons = await _addonRepository
                .AsQueryable()
                .Where(x => x.IsActive)
                .ToListAsync();

            response.IsSucess = true;

            response.Data = addons.Select(x => new
            {
                x.Id,
                x.Name,
                x.Price
            });

            return response;
        }
    }
}
