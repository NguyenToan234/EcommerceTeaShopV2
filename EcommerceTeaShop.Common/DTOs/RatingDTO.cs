using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CreateRatingDTO
    {
        public Guid ProductId { get; set; }

        public int Star { get; set; }

        public string Comment { get; set; }
    }
}
