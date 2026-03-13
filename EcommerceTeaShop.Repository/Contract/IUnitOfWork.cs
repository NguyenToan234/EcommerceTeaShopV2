using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace EcommerceTeaShop.Repository.Contract
{
    public interface IUnitOfWork
    {

        public Task<int> SaveChangeAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        void ClearChangeTracker();
        DbContext GetDbContext();
    }

}
