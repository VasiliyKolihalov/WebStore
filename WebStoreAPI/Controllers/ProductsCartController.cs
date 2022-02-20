using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using Scriban;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsCartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;
        private ProductsCart _productsCart;
        private User _user;

        public ProductsCartController(IApplicationContext productsContext, UserManager<User> userManager,
            IConfiguration appConfiguration)
        {
            _userManager = userManager;
            _applicationDb = productsContext;
            _configuration = appConfiguration;
        }

        private void InitializeProductsCart()
        {
            _productsCart = _applicationDb.ProductsCarts.Single(x => x.UserId == _user.Id);

            _productsCart.ProductsInCart = _applicationDb.ProductsInCarts
                .Include(x => x.Product)
                .Include(x => x.Product.Store)
                .Include(x => x.Product.Reviews)
                .Include(x => x.ProductsCart)
                .Where(x => x.ProductsCart.Id == _productsCart.Id)
                .ToList();
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<ProductsCartViewModel> Get()
        {
            SetUser();
            InitializeProductsCart();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductsCart, ProductsCartViewModel>();
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsCartViewModel = mapper.Map<ProductsCart, ProductsCartViewModel>(_productsCart);

            return productsCartViewModel;
        }


        [HttpPost("{productId}")]
        public ActionResult<ProductInCartViewModel> AddProduct(long productId)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == productId);

            if (product == null || product.QuantityInStock < 1)
                return NotFound();

            SetUser();
            InitializeProductsCart();
            var productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Product.Id == productId);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            if (productInCart != null)
            {
                productInCart.Count++;
                productInCart.Cost += product.Cost;
                _applicationDb.ProductsInCarts.Update(productInCart);
                _applicationDb.SaveChanges();
            }
            else
            {
                productInCart = new ProductInCart() {Cost = product.Cost, Product = product};
                _productsCart.ProductsInCart.Add(productInCart);
                _applicationDb.ProductsCarts.Update(_productsCart);
                _applicationDb.SaveChanges();
            }

            var productInCartViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            return Ok(productInCartViewModel);
        }

        [HttpDelete("{productId}")]
        public ActionResult<ProductInCartViewModel> DeleteProduct(long productId)
        {
            SetUser();
            InitializeProductsCart();

            var productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Product.Id == productId);
            if (productInCart == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            if (productInCart.Count > 1)
            {
                productInCart.Count--;
                _applicationDb.ProductsInCarts.Update(productInCart);
                _applicationDb.SaveChanges();
            }
            else
            {
                _applicationDb.ProductsInCarts.Remove(productInCart);
                _applicationDb.SaveChanges();
            }

            var productInCartView = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            return Ok(productInCartView);
        }

        [Route("select/{id}")]
        [HttpPut]
        public ActionResult<ProductInCartViewModel> SelectProduct(long id)
        {
            SetUser();
            InitializeProductsCart();

            var productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Id == id);

            if (productInCart == null)
                return NotFound();

            if (productInCart.Product.QuantityInStock < productInCart.Count)
                return BadRequest();

            productInCart.Selected = !productInCart.Selected;
            _applicationDb.ProductsInCarts.Update(productInCart);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productInCartViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            return Ok(productInCartViewModel);
        }

        [Route("buy")]
        [HttpPost]
        public ActionResult<IEnumerable<ProductInCartViewModel>> BuySelectedProducts()
        {
            SetUser();
            if (!_user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "email not confirmed");
                return BadRequest(ModelState);
            }

            InitializeProductsCart();
            List<ProductInCart> selectedProductsInCart = _productsCart.ProductsInCart.Where(x => x.Selected).ToList();

            if (selectedProductsInCart.Count < 1)
                return BadRequest();

            var htmlString = System.IO.File.ReadAllText("Views/PurchaseEmail.html");
            Template template = Template.Parse(htmlString);
            string message = template.Render(new
            {
                cart_cost = selectedProductsInCart.Sum(x => x.Cost),
                user_name = _user.UserName,
                products_in_cart = selectedProductsInCart
            });

            var emailService = new EmailService(_configuration);
            emailService.SendEmail(_user.Email, "Спасибо за покупку", message);

            foreach (var productInCart in selectedProductsInCart)
            {
                productInCart.Product.QuantityInStock -= productInCart.Count;
                _applicationDb.ProductsInCarts.Remove(productInCart);
            }

            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var selectedProductInCarViewModels =
                mapper.Map<List<ProductInCart>, List<ProductInCartViewModel>>(selectedProductsInCart);
            return Ok(selectedProductInCarViewModels);
        }
    }
}