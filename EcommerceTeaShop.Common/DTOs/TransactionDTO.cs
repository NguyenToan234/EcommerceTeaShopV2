using EcommerceTeaShop.Common.DTOs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace EcommerceTeaShop.Common.DTOs
    {
        public class TransactionDTO
        {
            public Guid OrderId { get; set; }

            public decimal Amount { get; set; }

            public string TransactionCode { get; set; }

            public string PaymentGateway { get; set; }

            public PaymentMethod PaymentMethod { get; set; }
        }
    }

