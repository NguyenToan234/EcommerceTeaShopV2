using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Wishlist : BaseModel
    {
        public Guid ClientId { get; set; }

        public Guid ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
