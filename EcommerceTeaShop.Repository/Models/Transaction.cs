using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceTeaShop.Repository.Models.EnumModels;

namespace EcommerceTeaShop.Repository.Models
{
    public class Transaction : BaseModel
    {
        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }

        public string PaymentGateway { get; set; }

        public string TransactionCode { get; set; }

        public DateTime TransactionDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public virtual Order Order { get; set; }
    }
}
