using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }
    }

    public class GoogleLoginRequest
    {
        public string Email { get; set; }

        public string Name { get; set; }
    }
}
