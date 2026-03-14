using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class ProductVariant : BaseModel
    {
        public Guid ProductId { get; set; }

        public int Gram { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public virtual Product Product { get; set; }
    }
}
