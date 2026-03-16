using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CreateBannerDTO
    {
        public IFormFile ImageUrl { get; set; }

        public string RedirectUrl { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
    public class UpdateBannerDTO
    {
        public IFormFile? ImageUrl { get; set; }

        public string RedirectUrl { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
