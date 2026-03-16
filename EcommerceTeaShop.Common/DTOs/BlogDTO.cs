using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CreateBlogDTO
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime PublishDate { get; set; }

        public bool IsPublished { get; set; }

        public IFormFile Thumbnail { get; set; }
    }
    public class UpdateBlogDTO
    {
        public string? Title { get; set; }

        public string? Content { get; set; }

        public bool? IsPublished { get; set; }

        public IFormFile? Thumbnail { get; set; }
    }
}
