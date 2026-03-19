using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Addon : BaseModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
        public virtual ICollection<ProductAddon> ProductAddons { get; set; }
    }
}
