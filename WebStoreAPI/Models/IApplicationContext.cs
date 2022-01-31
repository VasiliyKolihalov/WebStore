using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public interface IApplicationContext
    {
        public DbSet<Product> Products { get; }
        public DbSet<ProductsCart> ProductsCarts { get;}
        public DbSet<ProductInCart> ProductsInCarts { get; }
        public DbSet<Image> Images { get; }
        public DbSet<Category> Categories { get; }
        public DbSet<Store> Stores { get; }
        public DbSet<OpenStoreRequest> OpenStoreRequests { get; }
        public DbSet<Tag> Tags { get; }

        public int SaveChanges();
    }
}
