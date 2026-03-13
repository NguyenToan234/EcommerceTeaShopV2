namespace EcommerceTeaShop.Repository.Models
{
    public class RefreshToken : BaseModel
    {
        public Guid ClientId { get; set; }

        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }

        public virtual Client Client { get; set; }
    }
}