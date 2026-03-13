using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class ProductDetails : BaseModel
    {
        public Guid ProductId { get; set; }

        public string Weight { get; set; }

        public string Origin { get; set; }

        public string Ingredients { get; set; }

        public DateTime ExpiryDate { get; set; }

        public virtual Product Product { get; set; }
    }
}
