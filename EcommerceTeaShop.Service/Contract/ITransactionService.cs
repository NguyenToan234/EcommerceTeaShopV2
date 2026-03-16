using EcommerceTeaShop.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.Enums;


namespace EcommerceTeaShop.Service.Contract
{
    public interface ITransactionService
    {
        Task CreateTransactionAsync(
            Guid orderId,
            decimal amount,
            string transactionCode,
            PaymentMethod method,
            string gateway);

        Task<ResponseDTO> GetTransactionsAsync();
    }
}
