using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Image : BaseModel
    {
        public Guid ProductId { get; set; }

        public string ImageUrl { get; set; }

        public bool IsMain { get; set; }

        public virtual Product Product { get; set; }
    }
}
