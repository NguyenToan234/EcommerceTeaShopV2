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

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }
}
