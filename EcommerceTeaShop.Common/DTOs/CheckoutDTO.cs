using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CheckoutDTO
    {
        public Guid AddressId { get; set; }
        public List<Guid>? CartItemIds { get; set; }

    }
}
