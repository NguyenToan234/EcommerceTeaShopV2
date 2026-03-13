using EcommerceTeaShop.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Contract
{
    public interface IAdminRepository
    {
        Task<List<Client>> GetAllUsersAsync();
        Task<Client?> GetUserByIdAsync(Guid id);
        Task UpdateUserAsync(Client client);
        Task<List<Client>> GetUsersByRoleAsync(string roleName);

    }
}
