using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class RegisterRequest
    {
        public string FullName { get; set; }

        public string Email { get; set; } = "string@gmail.com";

        public string Password { get; set; } = "Abc@123";
    }
}
