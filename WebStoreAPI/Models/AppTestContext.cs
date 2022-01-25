using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class AppTestContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductsCart> ProductsCarts { get; set; }
        public DbSet<ProductInCart> ProductsInCarts { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<OpenStoreRequest> OpenStoreRequests { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public AppTestContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Product> GetProducts()
        {
            return Products;
        }
    }
}
