using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EcommerceTeaShop.Repository.Models
{
    public class Product : BaseModel
    {
        public string Name { get; set; }

        public string Description { get; set; }



        public Guid CategoryId { get; set; }

        public bool IsActive { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<Image> Images { get; set; }

        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<ProductVariant> Variants { get; set; }
        public virtual ICollection<ProductAddon> ProductAddons { get; set; }
    }
}
