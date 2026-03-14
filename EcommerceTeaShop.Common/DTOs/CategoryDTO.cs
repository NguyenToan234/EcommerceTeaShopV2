using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CreateCategoryDTO
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile Image { get; set; }
    }

    public class UpdateCategoryDTO
    {
        public string? Name { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class ReadCategoryDTO
    {
        public Guid CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ImageUrl { get; set; }

        public string Description { get; set; } = string.Empty;

    }
}