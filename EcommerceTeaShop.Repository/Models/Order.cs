using EcommerceTeaShop.Repository.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Order : BaseModel
    {
        public Guid ClientId { get; set; }

        public Guid? CouponId { get; set; }

        public long OrderCode { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime OrderDate { get; set; }

        public virtual Client Client { get; set; }

        public virtual Coupon Coupon { get; set; }
        public string FullName { get; set; }

        public string Phone { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string District { get; set; }

        public string Ward { get; set; }

        public string? CheckoutUrl { get; set; }

        public virtual ICollection<OrderDetails> OrderDetails { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
