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
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Crypto.Engines;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Services;

namespace WebStoreTests
{
    public class ProductsControllerTests
    {
        [Fact]
        public void GetAllProducts_ShouldReturnAllProducts()
        {
            var testProducts = GetTestProducts();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(testProducts);

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object);

            var result = productsController.GetAll();
            var resultProducts = (result.Result as OkObjectResult).Value as List<ProductViewModel>;

            Assert.Equal(testProducts.Count, resultProducts.Count);
        }


        [Fact]
        public void GetProduct_ShouldReturnCorrectProduct()
        {
            var testProduct = GetTestProduct();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product> {testProduct});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object);

            var result = productsController.Get(testProduct.Id);
            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;

            Assert.Equal(testProduct.Id, resultProduct.Id);
        }


        [Fact]
        public void GetProduct_ShouldNotFindProduct()
        {
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product> {GetTestProduct()});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object);

            Action result = () => productsController.Get(0);
            Assert.Throws<NotFoundException>(result);
        }


        [Fact]
        public void GetBasedStore_ShouldReturnAllStoreProducts()
        {
            var store = GetTestStore(GetTestUser());
            var testProducts = GetTestProducts(store);
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(testProducts);
            Mock<DbSet<Store>> mockDbSetStores = GetQueryableMockDbSet(new List<Store> {store});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStores.Object);
            var productsController = GetProductsController(mockContext.Object);

            var result = productsController.GetBasedStore(store.Id);
            var resultProducts = (result.Result as OkObjectResult).Value as List<ProductViewModel>;

            Assert.Equal(testProducts.Count, resultProducts.Count);
            Assert.Equal(store.Id, resultProducts[0].Store.Id);
        }


        [Fact]
        public void GetBasedStore_ShouldNotFindProducts()
        {
            var store = GetTestStore(GetTestUser());
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(store));
            Mock<DbSet<Store>> mockDbSetStores = GetQueryableMockDbSet(new List<Store> {store});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStores.Object);
            var productsController = GetProductsController(mockContext.Object);

            Action result = () => productsController.GetBasedStore(0);

            Assert.Throws<NotFoundException>(result);
        }


        [Fact]
        public void GetBasedCategory_ShouldReturnAllCategoryProducts()
        {
            var category = GetTestCategory();
            var testProducts = GetTestProducts(category);
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(testProducts);
            Mock<DbSet<Category>> mockDbSetCategories = GetQueryableMockDbSet(new List<Category> {category});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Categories).Returns(mockDbSetCategories.Object);
            var productsController = GetProductsController(mockContext.Object);

            var result = productsController.GetBasedCategory(category.Id);

            var resultProducts = (result.Result as OkObjectResult).Value as List<ProductViewModel>;

            Assert.Equal(testProducts.Count, resultProducts.Count);
        }


        [Fact]
        public void GetBasedCategory_ShouldNotFindProducts()
        {
            var category = GetTestCategory();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(category));
            Mock<DbSet<Category>> mockDbSetCategories = GetQueryableMockDbSet(new List<Category> {category});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Categories).Returns(mockDbSetCategories.Object);
            var productsController = GetProductsController(mockContext.Object);

            Action result = () => productsController.GetBasedCategory(0);

            Assert.Throws<NotFoundException>(result);
        }

        [Fact]
        public void GetBasedTag_ShouldReturnAllTagProducts()
        {
            var tag = GetTestTag();
            var testProducts = GetTestProducts(tag);
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(testProducts);
            Mock<DbSet<Tag>> mockDbSetTags = GetQueryableMockDbSet(new List<Tag>() {tag});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Tags).Returns(mockDbSetTags.Object);
            var productsController = GetProductsController(mockContext.Object);

            var result = productsController.GetBasedTag(tag.Id);

            var resultProducts = (result.Result as OkObjectResult).Value as List<ProductViewModel>;

            Assert.Equal(testProducts.Count, resultProducts.Count);
        }


        [Fact]
        public void GetBasedTag_ShouldNotFindProducts()
        {
            var tag = GetTestTag();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(GetTestProducts(tag));
            Mock<DbSet<Tag>> mockDbSetTags = GetQueryableMockDbSet(new List<Tag> {tag});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Tags).Returns(mockDbSetTags.Object);

            var productsController = GetProductsController(mockContext.Object);

            Action result = () => productsController.GetBasedTag(0);

            Assert.Throws<NotFoundException>(result);
        }


        [Fact]
        public void Post_ShouldAddProductBasedOnAdminRole()
        {
            var testProduct = GetTestProductAddModel();
            var testUser = GetTestUser();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>());
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store>());
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

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
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>());
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store>() {testStore});
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

            var result = productsController.Post(testProduct);

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(testProduct.Name, resultProduct.Name);
            Assert.NotNull(resultProduct.Store);
        }

        [Fact]
        public void Post_ShouldReturnBadRequestBecauseFailValidation()
        {
            var mockContext = new Mock<IApplicationContext>();

            var productsController = GetProductsController(mockContext.Object);
            productsController.ModelState.AddModelError(string.Empty, string.Empty);

            var result = productsController.Post(new ProductAddModel());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void Delete_ShouldDeleteProductBasedOnAdminRole()
        {
            var testUser = GetTestUser();
            var testProduct = GetTestProduct();
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product> {testProduct});
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result)
                .Returns(new List<string> {ApplicationConstants.AdminRoleName});
            
            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

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
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product> {testProduct});
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store> {testStore});
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result)
                .Returns(new List<string> {ApplicationConstants.SellerRoleName});

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);

            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

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
            Mock<DbSet<Product>> mockDbSetProducts =
                GetQueryableMockDbSet(new List<Product> {GetTestProduct(testStore)});
            Mock<DbSet<Store>> mockDbSetStore = GetQueryableMockDbSet(new List<Store> {testStore});
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(new User());
            mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result)
                .Returns(new List<string> {ApplicationConstants.SellerRoleName});

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            mockContext.Setup(x => x.Stores).Returns(mockDbSetStore.Object);
            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

            Action result = () => productsController.Delete(GetTestProduct(testStore).Id);

            Assert.Throws<NotFoundException>(result);
        }

        [Fact]
        public void Delete_ShouldNotFindProduct()
        {
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>(GetTestProducts()));

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object);

            Action result = () => productsController.Delete(0);

            Assert.Throws<NotFoundException>(result);
        }

        [Fact]
        public void Put_ShouldUpdateProductBasedOnAdminRole()
        {
            var testProduct = GetTestProduct();
            var mockUserManager = GetMockUserManager();
            var testUser = GetTestUser();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result)
                .Returns(new List<string> {ApplicationConstants.AdminRoleName});
            
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() {testProduct});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

            var result = productsController.Put(GetProductPutModel(testProduct));

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(resultProduct.Id, testProduct.Id);
            Assert.NotEqual(resultProduct.Name, testProduct.Name);
        }

        [Fact]
        public void Put_ShouldUpdateProductBasedOnSellerRole()
        {
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var testProduct = GetTestProduct(testStore);
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() {testProduct});
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(testUser);
            mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result)
                .Returns(new List<string>() {ApplicationConstants.SellerRoleName});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

            var result = productsController.Put(GetProductPutModel(testProduct));

            var resultProduct = (result.Result as OkObjectResult).Value as ProductViewModel;

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(resultProduct.Id, testProduct.Id);
            Assert.NotEqual(resultProduct.Name, testProduct.Name);
        }

        [Fact]
        public void Put_ShouldReturnBadRequestBecauseNotEnoughRights()
        {
            var testUser = GetTestUser();
            var testStore = GetTestStore(testUser);
            var testProduct = GetTestProduct(testStore);
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product> {testProduct});
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(new User());
            mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()).Result)
                .Returns(new List<string> {ApplicationConstants.SellerRoleName});

            var mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);
            var productsController = GetProductsController(mockContext.Object, mockUserManager.Object);

            Action result = () => productsController.Put(GetProductPutModel(testProduct));

            Assert.Throws<NotFoundException>(result);
        }

        [Fact]
        public void Put_ShouldReturnBadRequestBecauseFailValidation()
        {
            var testProduct = GetTestProduct();

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();

            var productsController = GetProductsController(mockContext.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();
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
            Mock<DbSet<Product>> mockDbSetProducts = GetQueryableMockDbSet(new List<Product>() {testProduct});

            Mock<IApplicationContext> mockContext = new Mock<IApplicationContext>();
            mockContext.Setup(x => x.Products).Returns(mockDbSetProducts.Object);

            var productsController = GetProductsController(mockContext.Object);

            Action result = () => productsController.Put(new ProductPutModel() {Id = 0});

            Assert.Throws<NotFoundException>(result);
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
                new Product
                {
                    Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store
                },
                new Product
                {
                    Id = 2, Name = "TestProduct2", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store
                },
                new Product
                {
                    Id = 3, Name = "TestProduct3", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store
                }
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
            var testProduct = new Product
                {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1};
            return testProduct;
        }

        private static Product GetTestProduct(Store store)
        {
            var product = new Product
                {Id = 1, Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1, Store = store};
            return product;
        }

        private static ProductAddModel GetTestProductAddModel()
        {
            var testProductAddModel = new ProductAddModel()
                {Name = "TestProduct1", Description = "Test", Cost = 1, QuantityInStock = 1};
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
            var store = new Store {Id = 1, Name = "TestStore1", Description = "Test store", Seller = testSeller};
            return store;
        }

        private static User GetTestUser()
        {
            var user = new User {UserName = "TestUser1", Id = "testId"};
            return user;
        }

        private static Category GetTestCategory()
        {
            var testCategory = new Category {Id = 1, Name = "TestCategory1"};
            return testCategory;
        }

        private static Tag GetTestTag()
        {
            var tag = new Tag() {Id = 1, Name = "TestTag1", Description = "test tag"};
            return tag;
        }

        private static Mock<UserManager<User>> GetMockUserManager()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManager =
                new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()).Result).Returns(GetTestUser());
            return userManager;
        }

        private static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockDbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
            return mockDbSet;
        }

        private static Mock<ICurrencyService> GetMockCurrencyService()
        {
            Mock<ICurrencyService> mockCurrencyService = new Mock<ICurrencyService>();
            mockCurrencyService.Setup(x => x.ConvertCurrency(It.IsAny<decimal>(), It.IsAny<AvailableCurrencies>()))
                .Returns(It.IsAny<decimal>());
            return mockCurrencyService;
        }

        private static ProductsController GetProductsController(IApplicationContext context)
        {
            var mockUserManager = GetMockUserManager();
            ProductsService productsService = new ProductsService(context, mockUserManager.Object,
                GetMockCurrencyService().Object);

            ProductsController productsController = new ProductsController(productsService,
                mockUserManager.Object);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();
            return productsController;
        }

        private static ProductsController GetProductsController(IApplicationContext context, UserManager<User> userManager)
        {
            ProductsService productsService = new ProductsService(context, userManager,
                GetMockCurrencyService().Object);

            ProductsController productsController = new ProductsController(productsService,
                userManager);
            productsController.ControllerContext.HttpContext = new DefaultHttpContext();
            return productsController;
        }
    }
}