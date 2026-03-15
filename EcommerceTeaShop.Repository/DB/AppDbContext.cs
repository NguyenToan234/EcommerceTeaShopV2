using EcommerceTeaShop.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Repository.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Addresses> Addresses { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<Product>()
        //        .HasOne(p => p.Category)
        //        .WithMany(c => c.Products)
        //        .HasForeignKey(p => p.CategoryId);

        //    modelBuilder.Entity<Order>()
        //        .HasMany(o => o.OrderDetails)
        //        .WithOne(d => d.Order)
        //        .HasForeignKey(d => d.OrderId);

        //    modelBuilder.Entity<Client>()
        //        .HasMany(c => c.Orders)
        //        .WithOne(o => o.Client)
        //        .HasForeignKey(o => o.ClientId);


        //}




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductVariant>()
                .ToTable("ProductVariant");

            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "System Admin",
                    Email = "hibana664@gmail.com",
                    PasswordHash = "$2a$11$K9LQmT9J5tVn3h6r1mB6yOBXlJfK1z2Yq3cE7U8dR9sT0uV1wX2yG",
                    Role = "Admin",
                    EmailVerified = true
                }
            );
        }
    }
}
