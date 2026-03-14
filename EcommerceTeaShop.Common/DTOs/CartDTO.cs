using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class AddToCartDTO
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDTO
    {
        public Guid CartItemId { get; set; }

        public int Quantity { get; set; }
    }

    public class ReadCartItemDTO
    {
        public Guid CartItemId { get; set; }

        public Guid ProductVariantId { get; set; }

        public string ProductName { get; set; }

        public int Gram { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
