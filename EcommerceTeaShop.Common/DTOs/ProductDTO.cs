using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class CreateProductDTO
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;



        public Guid CategoryId { get; set; }
        public List<IFormFile> Images { get; set; } = new();
        public string Variants { get; set; }
    }
    public class CreateProductVariantDTO
    {
        public int Gram { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
    }

    public class UpdateProductDTO
    {
        public string? Name { get; set; }

        public string? Description { get; set; }


        public Guid? CategoryId { get; set; }

        public bool? IsActive { get; set; }
        public List<IFormFile>? NewImages { get; set; }
    }

    public class ReadProductDTO
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string CategoryName { get; set; }
        public bool IsActive { get; set; }

        public List<string> Images { get; set; }
    }
    public class UpdateVariantDTO
    {
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
    }
    public class ProductVariantDTO
    {
        public Guid Id { get; set; }

        public int Gram { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
    }
}
