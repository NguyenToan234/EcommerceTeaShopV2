using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CreateAddonDTO
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public IFormFile? Image { get; set; }
    }
    public class UpdateAddonDTO
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public IFormFile? Image { get; set; }

        public bool? IsActive { get; set; }
    }
    public class AssignAddonDTO
    {
        public List<Guid> AddonIds { get; set; }
    }
}
