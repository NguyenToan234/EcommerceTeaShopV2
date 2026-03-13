using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Rating : BaseModel
    {
        public Guid ProductId { get; set; }

        public Guid ClientId { get; set; }

        public int Star { get; set; }

        public string Comment { get; set; }

        public virtual Product Product { get; set; }
    }
}
