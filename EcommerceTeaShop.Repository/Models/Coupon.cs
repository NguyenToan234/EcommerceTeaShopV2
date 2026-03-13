using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Coupon : BaseModel
    {
        public string Code { get; set; }

        public int DiscountPercent { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public decimal? MinOrderValue { get; set; }

        public int UsageLimit { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
