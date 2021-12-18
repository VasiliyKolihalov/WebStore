using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System.Net.Mail;
using Scriban;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using AutoMapper;
using System.Net;
using Microsoft.AspNetCore.Html;

namespace WebStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsCartController : ControllerBase
    {
        private ProductsCart _productsCart;
        private User _user;
        private readonly IConfiguration _appConfiguration;
        private readonly ApplicationContext _applicationDB;
        private readonly UserManager<User> _userManager;

        public ProductsCartController(ApplicationContext productsContext, UserManager<User> userManager, IConfiguration appConfiguration)
        {
            _userManager = userManager;
            _applicationDB = productsContext;
            _appConfiguration = appConfiguration;
        }

        private void InitializeProductsCart()
        {
            _productsCart = _applicationDB.ProductsCarts.Single(x => x.UserId == _user.Id);

            _productsCart.ProductsInCart = _applicationDB.ProductsInCarts
                                                         .Include(x => x.Product.Store)
                                                         .Include(x => x.ProductsCarts)
                                                         .Where(x => x.ProductsCarts.FirstOrDefault(x => x.Id == _productsCart.Id) != null)
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
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsCartViewModel = mapper.Map<ProductsCart, ProductsCartViewModel>(_productsCart);

            return productsCartViewModel;
        }


        [HttpPost("{id}")]
        public ActionResult<ProductInCartViewModel> AddProduct(long id)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            if (product.QuantityInStock < 1)
                return NotFound();

            SetUser();
            InitializeProductsCart();
            ProductInCart productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Product.Id == id);

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
                _applicationDB.ProductsInCarts.Update(productInCart);
                _applicationDB.SaveChanges();

                var productInCatrViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
                return Ok(productInCatrViewModel);
            }
            else
            {
                productInCart = new ProductInCart() {  Cost = product.Cost, Product = product};
                _applicationDB.ProductsInCarts.Add(productInCart);
                _productsCart.ProductsInCart.Add(productInCart);
                _applicationDB.SaveChanges();

                var productInCatrViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
                return Ok(productInCatrViewModel);

            }
        }

        [HttpDelete("{id}")]
        public ActionResult<ProductInCartViewModel> DeleteProduct(long id)
        {
            SetUser();
            InitializeProductsCart();

            ProductInCart productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Product.Id == id);

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
                _applicationDB.ProductsInCarts.Update(productInCart);
                _applicationDB.SaveChanges();

                var productInCatrView = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
                return Ok(productInCatrView);
            }
            else
            {
                _applicationDB.ProductsInCarts.Remove(productInCart);
                _applicationDB.SaveChanges();

                var productInCatrViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
                return Ok(productInCatrViewModel);
            }

        }

        [Route("select/{id}")]
        [HttpPut]
        public ActionResult<ProductInCartViewModel> SelectProduct(long id)
        {
            SetUser();
            InitializeProductsCart();

            var productInCart = _applicationDB.ProductsInCarts.FirstOrDefault(x => x.Product.Id == id &&
                                                                                   x.ProductsCarts.FirstOrDefault(x => x.Id == _productsCart.Id) != null);

            if (productInCart == null)
                return NotFound();

            if (productInCart.CanBuy == false)
                return BadRequest();

            productInCart.Selected = !productInCart.Selected;
            _applicationDB.ProductsInCarts.Update(productInCart);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            { 
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();
                cfg.CreateMap<Product, ProductViewModel>(); 
                cfg.CreateMap<Store, StorePutModel>(); 
            });
            var mapper = new Mapper(mapperConfig);

            var productInCatrViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            return Ok(productInCatrViewModel);
        }

        [Route("buy")]
        [HttpPost]
        public ActionResult<IEnumerable<ProductInCartViewModel>> BuySelectedProducts()
        {
            SetUser();
            InitializeProductsCart();

            var selectedProductsInCart = _productsCart.ProductsInCart.Where(x => x.Selected == true).ToList();

            if (selectedProductsInCart.Count < 1 ||
                selectedProductsInCart.FirstOrDefault(x => x.CanBuy == false) != null)
                return BadRequest();

            string companyName = _appConfiguration["CompanyData:Name"];
            string companyEmail = _appConfiguration["CompanyData:Email"];
            string companyPassword = _appConfiguration["CompanyData:EmailPassword"];

            MailAddress from = new MailAddress(companyEmail, companyName);
            MailAddress to = new MailAddress(_user.Email);
         
            string htmlString = System.IO.File.ReadAllText("Views/PurchaseEmail.html");
            var template = Template.Parse(htmlString);
            string result = template.Render(new
            {
                cartcost = _productsCart.ProductsCartCost,
                username = _user.UserName,
                productsincart = selectedProductsInCart
            });

            MailMessage message = new MailMessage(from, to)
            {
                Subject = companyName,
                IsBodyHtml = true,
                Body = result
            };

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(companyEmail, companyPassword),
                EnableSsl = true
            };
            smtp.SendMailAsync(message).Wait();
            foreach (var productInCart in selectedProductsInCart)
            {
                productInCart.Product.QuantityInStock -= productInCart.Count;
                _applicationDB.ProductsInCarts.Remove(productInCart);
            }

            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => 
            { cfg.CreateMap<ProductInCart, ProductInCartViewModel>(); 
                cfg.CreateMap<Product, ProductViewModel>(); 
                cfg.CreateMap<Store, StorePutModel>(); 
            });
            var mapper = new Mapper(mapperConfig);

            var selectedProductInCarViewModels = mapper.Map<List<ProductInCart>, List<ProductInCartViewModel>>(selectedProductsInCart);

            return Ok(selectedProductInCarViewModels);
        }

    }
}
