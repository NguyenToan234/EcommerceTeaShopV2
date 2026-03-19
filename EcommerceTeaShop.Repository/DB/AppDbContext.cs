using BCrypt.Net;
using EcommerceTeaShop.Repository.Models;
using FirebaseAdmin.Auth.Hash;
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

        public DbSet<Addon> Addons { get; set; }
        public DbSet<ProductAddon> ProductAddons { get; set; }
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
        public DbSet<Banner> Banners { get; set; }

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

            string password = "Admin@123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "Super Admin",
                    Email = "admin@teashop.com",
                    PasswordHash = hash,
                    Role = "Admin",
                    EmailVerified = true,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Client
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "Second Admin",
                    Email = "admin2@teashop.com",
                    PasswordHash = hash,
                    Role = "Admin",
                    EmailVerified = true,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                }
            
             );
            modelBuilder.Entity<ProductAddon>()
        .HasKey(pa => new { pa.ProductId, pa.AddonId });
        }
    }
}
