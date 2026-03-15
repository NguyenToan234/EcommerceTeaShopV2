using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Cart : BaseModel
    {
        public Guid ClientId { get; set; }

        public Guid? CouponId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal FinalAmount { get; set; }

        public virtual Client Client { get; set; }

        public virtual Coupon Coupon { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
