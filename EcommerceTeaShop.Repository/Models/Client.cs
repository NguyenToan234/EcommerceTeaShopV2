using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.Models
{
    public class Client : BaseModel
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Role { get; set; } = "User";

        public string? Phone { get; set; }

        public string PasswordHash { get; set; }

        public string? GoogleId { get; set; }

        public string? AvatarUrl { get; set; }

        public bool EmailVerified { get; set; } = false;

        public string? EmailOtp { get; set; }

        public DateTime? EmailOtpExpiry { get; set; }

        public string Status { get; set; } = "Active";

        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<Addresses> Addresses { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<Cart> Carts { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
