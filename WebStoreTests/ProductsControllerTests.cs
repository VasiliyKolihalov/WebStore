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
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace WebStoreTests
{
    public class ProductsControllerTests
    {
        [Fact]
        public void GetAllProducts_ShouldReturnAllProducts()
        {
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts());
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetAll().Value as List<ProductViewModel>;

            Assert.Equal(result.Count, GetTestProducts().Count);

        }


        [Fact]
        public void GetProduct_ShouldReturnCorrectProduct()
        {
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { GetTestProduct() });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.Get(GetTestProduct().Id).Value;

            Assert.Equal(GetTestProduct().Id, result.Id);

        }


        [Fact]
        public void GetProduct_ShouldNotFindProduct()
        {
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { GetTestProduct() });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.Get(0);

            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public void GetBasedStore_ShouldReturnAllStoreProducts()
        {
            var store = GetTestStore(GetTestUser());

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(store));
            Mock<DbSet<Store>> mockDbSetStores = GetQueryableMockDbSet(new List<Store>() { store });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStores.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetBasedStore(store.Id).Value as List<ProductViewModel>;

            Assert.Equal(result.Count, GetTestProducts(store).Count);
            Assert.Equal(result[0].Store.Id, store.Id);

        }


        [Fact]
        public void GetBasedStore_ShouldNotFindProducts()
        {
            var store = GetTestStore(GetTestUser());

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(store));
            Mock<DbSet<Store>> mockDbSetStores = GetQueryableMockDbSet(new List<Store>() { store });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStores.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetBasedStore(0);

            Assert.IsType<NotFoundResult>(result.Result);
        }


        [Fact]
        public void GetBasedCategory_ShouldReturnAllCategoryProducts()
        {
            var category = GetTestCategory();

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(category));
            Mock<DbSet<Category>> mockDbSetCategories = GetQueryableMockDbSet(new List<Category>() { category });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Categories).Returns(mockDbSetCategories.Object);
            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetBasedCategory(category.Id).Value as List<ProductViewModel>;

            Assert.Equal(result.Count, GetTestProducts(category).Count);
        }

        [Fact]
        public void GetBasedCategory_ShouldNotFindProducts()
        {
            var category = GetTestCategory();

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(category));
            Mock<DbSet<Category>> mockDbSetCategories = GetQueryableMockDbSet(new List<Category>() { category });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Categories).Returns(mockDbSetCategories.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetBasedCategory(0);

            Assert.IsType<NotFoundResult>(result.Result);

        }

        [Fact]
        public void GetBasedTag_ShouldReturnAllTagProducts()
        {
            var tag = GetTestTag();

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(tag));
            Mock<DbSet<Tag>> mockDbSetTags = GetQueryableMockDbSet(new List<Tag>() { tag });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Tags).Returns(mockDbSetTags.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetBasedTag(tag.Id).Value as List<ProductViewModel>;

            Assert.Equal(result.Count, GetTestProducts(tag).Count);

        }

        [Fact]
        public void GetBasedTag_ShouldNotFindProducts()
        {
            var tag = GetTestTag();

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(tag));
            Mock<DbSet<Tag>> mockDbSetTags = GetQueryableMockDbSet(new List<Tag>() { tag });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Tags).Returns(mockDbSetTags.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.GetBasedTag(0);

            Assert.IsType<NotFoundResult>(result.Result);

        }


        [Fact]
        public void Post_ShouldAddProductBasedOnAdminRole()
        {
            var testProduct = GetTestProductAddModel();
            var testUser = GetTestUser();
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>());
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store>());
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Post(testProduct);

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;
            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(testProduct.Name, resultProduct.Name);
            Assert.Null(resultProduct.Store);

        }


        [Fact]
        public void Post_ShouldAddProductBasedOnSellerRole()
        {
            var testProduct = GetTestProductAddModel();
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);

            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>());
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store>() { testStore });
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Post(testProduct);

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;
            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(testProduct.Name, resultProduct.Name);
            Assert.NotNull(resultProduct.Store);

        }

        [Fact]
        public void Post_ShouldReturnBadRequestBecauseFailValidation()
        {
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);
            productsController.ModelState.AddModelError(string.Empty, string.Empty);

            var result = productsController.Post(new ProductAddModel());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_ShouldDeleteProductBasedOnAdminRole()
        {
            var testUser = GetTestUser();
            var userManagerMock = GetMockUserManager();
            var testProduct = GetTestProduct();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string>() { RolesConstants.AdminRoleName });

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { testProduct });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Delete(testProduct.Id);

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;
            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(resultProduct.Name, testProduct.Name);
        }

        [Fact]
        public void Delete_ShouldDeleteProductBasedOnSellerRole()
        {
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var testProduct = GetTestProduct(testStore);
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string>() { RolesConstants.SellerRoleName });

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { testProduct });
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store>() { testStore });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Delete(testProduct.Id);

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;
            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(testProduct.Name, resultProduct.Name);
        }

        [Fact]
        public void Delete_ShouldReturnBadRequestBecauseNotEnoughRights()
        {
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var userManagerMock = GetMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(new User());
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string>() { RolesConstants.SellerRoleName });

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { GetTestProduct(testStore) });
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store>() { testStore });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Delete(GetTestProduct(testStore).Id);

            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public void Delete_ShouldNotFindProduct()
        {
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>(GetTestProducts()));
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.Delete(0);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Put_ShouldUpdateProductBasedOnAdminRole()
        {
            var testProduct = GetTestProduct();
            var userManagerMock = GetMockUserManager();
            var testUser = GetTestUser();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string>() { RolesConstants.AdminRoleName });

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { testProduct });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Put(GetProductPutModel(testProduct));

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;
            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(resultProduct.Id, testProduct.Id);
            Assert.NotEqual(resultProduct.Name, testProduct.Name);
        }

        [Fact]
        public void Put_ShouldUpdateProductBasedOnSellerRole()
        {
            var userManagerMock = GetMockUserManager();
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var testProduct = GetTestProduct(testStore);
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string>() { RolesConstants.SellerRoleName });

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { testProduct });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Put(GetProductPutModel(testProduct));

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;
            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(resultProduct.Id, testProduct.Id);
            Assert.NotEqual(resultProduct.Name, testProduct.Name);
        }

        [Fact]
        public void Put_ShouldReturnBadRequestBecauseNotEnoughRights()
        {
            var userManagerMock = GetMockUserManager();
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var testProduct = GetTestProduct(testStore);
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(new User());
            userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result).Returns(new List<string>() { RolesConstants.SellerRoleName });

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { testProduct });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();

            var result = productsController.Put(GetProductPutModel(testProduct));

            Assert.IsType<BadRequestResult>(result.Result);
            
        }

        [Fact]
        public void Put_ShouldReturnBadRequestBecauseFailValidation()
        {
            var userManagerMock = GetMockUserManager();
            var testProduct = GetTestProduct();

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            ProductsController productsController = new ProductsController(mockContext.Object, userManagerMock.Object);
            productsController.ModelState.AddModelError(string.Empty, string.Empty);

            var result = productsController.Put(GetProductPutModel(testProduct));

            Assert.IsType<BadRequestObjectResult>(result.Result);

        }

        [Fact]
        public void Put_ShouldNotFindProduct()
        {
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var testProduct = GetTestProduct(testStore);        

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() { testProduct });
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            ProductsController productsController = new ProductsController(mockContext.Object, GetMockUserManager().Object);

            var result = productsController.Put(new ProductPutModel() {Id = 0 });

            Assert.IsType<NotFoundResult>(result.Result);         
        }

        private static List<Product> GetTestProducts()
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1}
            };

            return products;
        }

        private static List<Product> GetTestProducts(Store store)
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store}
            };

            return products;
        }

        private static List<Product> GetTestProducts(Category category)
        {
            var products = new List<Product>()
            {
                new Product {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1},
                new Product {Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1}
            };

            foreach (var product in products)
            {
                product.Categories.Add(category);
            }

            return products;
        }

        private static List<Product> GetTestProducts(Tag tag)
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

        private static Product GetTestProduct()
        {
            var testProduct = new Product { Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1 };
            return testProduct;
        }

        private static Product GetTestProduct(Store store)
        {
            var product = new Product { Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store };
            return product;

        }

        private static ProductAddModel GetTestProductAddModel()
        {
            var testProductAddModel = new ProductAddModel() { Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1 };
            return testProductAddModel;
        }

        private static ProductPutModel GetProductPutModel(Product testProduct)
        {
            var productPutModel = new ProductPutModel()
            {
                Id = testProduct.Id,
                Name = "UpdateTestProduct",
                Description = "Update",
                Cost = 1,
                QuantityInStock = 1
            };
            return productPutModel;
        }

        private static Store GetTestStore(User testSeller)
        {
            var store = new Store() { Id = 1, Name = "TestStore1", Description = "Test store", Seller = testSeller };
            return store;
        }

        private static User GetTestUser()
        {
            var user = new User() { UserName = "TestUser1", Id = "testId" };
            return user;
        }

        private static Category GetTestCategory()
        {
            var testCategory = new Category() { Id = 1, Name = "TestCategory1" };
            return testCategory;
        }

        private static Tag GetTestTag()
        {
            var tag = new Tag() { Id = 1, Name = "TestTag1", Description = "test tag" };
            return tag;
        }

        private static Mock<UserManager<User>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var mockDBSet = new Mock<DbSet<T>>();
            mockDBSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDBSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDBSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDBSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockDBSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
            return mockDBSet;
        }
    }
}
