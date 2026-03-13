using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Role : BaseModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Client> Clients { get; set; }
    }
}
