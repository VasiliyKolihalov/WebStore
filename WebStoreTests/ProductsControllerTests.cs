using System;
using Xunit;
using WebStoreAPI.Models;
using WebStoreAPI.Controllers;
using Moq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Query;
using System.Threading;

namespace WebStoreTests
{
    public class ProductsControllerTests
    {
        /*[Fact]
        public void GetAllProducts_ShouldReturnAllProducts()
        {
                Mock<ApplicationContext> mockContext = new Mock<ApplicationContext>();
                //Mock<DbSet<Product>>
                mockContext.Setup(x => x.Products).Returns(It.IsAny<DbSet<Product>>());
                ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

                var result = productsController.GetAll().Value as List<ProductViewModel>;

                Assert.Equal(result.Count, GetTestProducts().Count);
            
        }*/

        [Fact]
        public void Test1()
        {
            var nock = new Mock<DbSet<Product>>();
            var mockContext = new Mock<AppTestContext>();
            mockContext.Setup(x => x.GetProducts()).Returns(nock.Object);
        }

        /*    [Fact]
            public void GetProduct_ShouldReturnCorrectProduct()
            {
                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Products.Add(GetTestProduct());
                    context.SaveChanges();
                }


                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.Get(GetTestProduct().Id).Value;

                    Assert.NotNull(result);
                    Assert.Equal(GetTestProduct().Id, result.Id);
                }
            }


            [Fact]
            public void GetProduct_ShouldNotFindProduct()
            {

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Products.Add(GetTestProduct());
                    context.SaveChanges();
                }


                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.Get(0);

                    Assert.IsType<NotFoundResult>(result.Result);

                }
            }

            [Fact]
            public void GetBasedStore_ShouldReturnAllStoreProducts()
            {
                var store = GetTestStore(GetTestUser());

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Products.AddRange(GetTestProducts(store));
                    context.SaveChanges();
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.GetBasedStore(store.Id).Value as List<ProductViewModel>;

                    Assert.Equal(result.Count, GetTestProducts(store).Count);
                }
            }

            [Fact]
            public void GetBasedStore_ShouldNotFindProducts()
            {
                var store = GetTestStore(GetTestUser());

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Products.AddRange(GetTestProducts(store));
                    context.SaveChanges();
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.GetBasedStore(0);

                    Assert.IsType<NotFoundResult>(result.Result);
                }
            }

            [Fact]
            public void GetBasedCategory_ShouldReturnAllCategoryProducts()
            {
                var category = GetTestCategory();

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Products.AddRange(GetTestProducts(category));
                    context.SaveChanges();
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.GetBasedCategory(category.Id).Value as List<ProductViewModel>;

                    Assert.Equal(result.Count, GetTestProducts(category).Count);
                }
            }

            [Fact]
            public void GetBasedCategory_ShouldNotFindProducts() 
            {
                var category = GetTestCategory();

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Products.AddRange(GetTestProducts(category));
                    context.SaveChanges();
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.GetBasedCategory(0);

                    Assert.IsType<NotFoundResult>(result.Result);
                }
            }

            [Fact]
            public void GetBasedTag_ShouldReturnAllCategoryProducts()
            {
                var tag = GetTestTag();

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Products.AddRange(GetTestProducts(tag));
                    context.SaveChanges();
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.GetBasedTag(tag.Id).Value as List<ProductViewModel>;

                    Assert.Equal(result.Count, GetTestProducts(tag).Count);
                }
            }

            [Fact]
            public void GetBasedTag_ShouldNotFindProducts()
            {
                var tag = GetTestTag();

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.Products.AddRange(GetTestProducts(tag));
                    context.SaveChanges();
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    ProductsController productsController = new ProductsController(context, GetMockUserManager().Object);

                    var result = productsController.GetBasedTag(0);

                    Assert.IsType<NotFoundResult>(result.Result);
                }
            }

            [Fact]
            public void Post_ShouldAddProductWithoutStore()
            {
                var testProduct = GetTestProductAddModel();
                var testUser = GetTestUser(); 

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    var userManagerMock = GetMockUserManager();
                    userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string> { RolesConstants.AdminRoleName });
                    userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);

                    ProductsController productsController = new ProductsController(context, userManagerMock.Object);
                    productsController.ControllerContext.HttpContext = new DefaultHttpContext();

                    var result = productsController.Post(testProduct);

                    Assert.IsType<OkObjectResult>(result.Result);
                    Assert.Null(((result.Result as OkObjectResult).Value as ProductViewModel).Store);
                }
            }

            [Fact]
            public void Post_ShouldAddProductWithStore()
            {
                var userManagerMock = GetMockUserManager();
                var testUser = GetTestUser();
                var testStore = GetTestStore(testUser);
                var testProduct = GetTestProductAddModel();

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Stores.Add(testStore);
                    context.SaveChanges();

                    userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
                }

                using (var context = new ApplicationContext(GetDbContextOptions()))
                {         
                    ProductsController productsController = new ProductsController(context, userManagerMock.Object);
                    productsController.ControllerContext.HttpContext = new DefaultHttpContext();

                    var result = productsController.Post(testProduct);

                    Assert.IsType<OkObjectResult>(result.Result);
                    Assert.NotNull(((result.Result as OkObjectResult).Value as ProductViewModel).Store);

                }

            }*/

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

        private List<Product> GetTestProducts(Store store)
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store}
            };

            return products;
        }

        private List<Product> GetTestProducts(Category category)
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1}
            };

            foreach(var product in products)
            {
                product.Categories.Add(category);
            }

            return products;
        }

        private List<Product> GetTestProducts(Tag tag)
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1}
            };

            foreach (var product in products)
            {
                product.Tags.Add(tag);
            }

            return products;
        }

        private Product GetTestProduct()
        {
            var testProduct = new Product { Id = 4, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1 };
            return testProduct;
        }

       

        private List<ProductAddModel> GetTestProductAddModels()
        {
            var testProductAddModels = new List<ProductAddModel>()
            {
                new ProductAddModel() { Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1 },
                new ProductAddModel() { Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1 },
                new ProductAddModel() { Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1 },
            };
            return testProductAddModels;

        }

        private ProductAddModel GetTestProductAddModel()
        {
            var testProductAddModel = new ProductAddModel() { Name = "TestProduct", Description = "Test", Cost = 1, QuantityInStock = 1 };
            return testProductAddModel;
        }

        private Store GetTestStore(User testSeller)
        {
            var store = new Store() { Name = "TestStore1", Description = "Test store", Id = 1, Seller = testSeller };
            return store;
        }

        private User GetTestUser()
        {
            var user = new User() { UserName = "TestUser1", Id = "testId" };
            return user;
        }

        private Category GetTestCategory()
        {
            var testCategory = new Category() { Id = 1, Name = "TestCategory1" };
            return testCategory;
        }

        private Tag GetTestTag()
        {
            var tag = new Tag() { Id = 1, Name = "TestTag1", Description = "test tag" };
            return tag;
        }

        private Mock<UserManager<User>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        private DbContextOptions<ApplicationContext> GetDbContextOptions()
        {

            var options = new DbContextOptionsBuilder<ApplicationContext>()
                                               .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                                               .Options;

            return options;

        }
    }
}
