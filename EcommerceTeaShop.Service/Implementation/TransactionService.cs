using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.Enums;
using EcommerceTeaShop.Repository.Contract;
using EcommerceTeaShop.Repository.Models;
using EcommerceTeaShop.Repository.Models.EnumModels;
using EcommerceTeaShop.Service.Contract;
using Microsoft.EntityFrameworkCore;

namespace EcommerceTeaShop.Service.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(
            IGenericRepository<Transaction> transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateTransactionAsync(
            Guid orderId,
            decimal amount,
            string transactionCode,
            Common.DTOs.Enums.PaymentMethod method,
            string gateway)
        {
            var transaction = new Transaction
            {
                OrderId = orderId,
                Amount = amount,
                TransactionCode = transactionCode,
                PaymentGateway = gateway,
                PaymentMethod = (Repository.Models.EnumModels.PaymentMethod)method,
                Status = PaymentStatus.Success,
                TransactionDate = DateTime.UtcNow
            };

            await _transactionRepository.Insert(transaction);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<ResponseDTO> GetTransactionsAsync()
        {
            ResponseDTO response = new();

            var transactions = await _transactionRepository
                .AsQueryable()
                .Include(x => x.Order)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            response.IsSucess = true;

            response.Data = transactions.Select(x => new
            {
                x.Id,
                x.OrderId,
                x.Amount,
                x.PaymentGateway,
                x.TransactionCode,
                x.TransactionDate,
                Status = x.Status.ToString(),
                PaymentMethod = x.PaymentMethod.ToString()
            });

            return response;
        }
    }
}