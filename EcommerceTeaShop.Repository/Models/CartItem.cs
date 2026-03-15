namespace EcommerceTeaShop.Repository.Models
{
    public class CartItem : BaseModel
    {
        public Guid CartId { get; set; }

        public Guid ProductVariantId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public virtual Cart Cart { get; set; }

        public virtual ProductVariant ProductVariant { get; set; }

    }
}