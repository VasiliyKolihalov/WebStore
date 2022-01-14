using System;
using Xunit;
using WebStoreAPI.Models;
using WebStoreAPI.Controllers;
using Moq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore;

namespace WebStoreTests
{
    public class ProductsControllerTests
    {
        [Fact]
        public void GetAllProducts_ShouldReturnAllProducts()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                                                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                                .Options;

            using(var context = new ApplicationContext(options))
            {
                context.Products.AddRange(GetTestProducts());
                context.SaveChanges();
            }

            using (var context = new ApplicationContext(options))
            { 
                ProductsController productsController = new ProductsController(context);

                var result = productsController.GetAll().Value as List<ProductViewModel>;

                Assert.Equal(result.Count, GetTestProducts().Count);
            }
        }

        private List<Product> GetTestProducts()
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1}
            };

            return products;
        }
    }
}
