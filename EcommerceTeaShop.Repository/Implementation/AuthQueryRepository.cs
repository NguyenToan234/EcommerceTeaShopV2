using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.DB;
using EcommerceTeaShop.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceTeaShop.Repository.Implementation
{
    public class AuthQueryRepository : IAuthQueryRepository
    {
        private readonly AppDbContext _context;

        public AuthQueryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetUserByEmail(string email)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}