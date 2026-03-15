using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class OrderDetails : BaseModel
    {
        public Guid OrderId { get; set; }

        public Guid ProductVariantId { get; set; }

        public Guid? AddonId { get; set; }   // thêm

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public virtual Order Order { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }

        public virtual Addon Addon { get; set; }  // thêm}
    }
}
