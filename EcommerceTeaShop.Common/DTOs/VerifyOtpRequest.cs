using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }

        public string Otp { get; set; }
    }
    public class ResendOtpRequest
    {
        public string Email { get; set; }
    }
}
