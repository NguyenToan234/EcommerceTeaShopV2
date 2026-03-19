using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class ProductAddon
    {
        public Guid ProductId { get; set; }
        public Guid AddonId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Addon Addon { get; set; }
    }
}
