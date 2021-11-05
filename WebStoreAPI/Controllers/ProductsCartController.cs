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
        private User _currentUser;
        private readonly IConfiguration _appConfiguration;
        private readonly ApplicationContext _applicationDB;
        private readonly UserManager<User> _userManager;

        public ProductsCartController(ApplicationContext productsContext, UserManager<User> userManager, IConfiguration appConfiguration)
        {
            _userManager = userManager;
            _applicationDB = productsContext;
            _appConfiguration = appConfiguration;
        }

        private void SetProductsCart()
        {
            SetUser();
            _productsCart = _applicationDB.ProductsCarts.Single(x => x.UserId == _currentUser.Id);
        }

        private void InitializeProductsCart()
        {
            _productsCart.ProductsInCart = _applicationDB.ProductsInCarts.Where(x => x.ProductsCartId == _productsCart.Id).ToList();

            foreach(var productInCar in _productsCart.ProductsInCart)
            {
                productInCar.Product = _applicationDB.Products.Single(x => x.Id == productInCar.ProductId);
            }
        }

        private void SetUser()
        {
            _currentUser = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<ProductsCart> Get()
        {
            SetProductsCart();
            InitializeProductsCart();
            return _productsCart;
        }


        [HttpPost("{id}")]
        public ActionResult<ProductsCart> Post(int id)
        {
            SetProductsCart();
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            InitializeProductsCart();
            ProductInCart productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.ProductId == id);

            if (productInCart != null)
            {
                productInCart.Count++;
                productInCart.Cost += product.Cost;
                _applicationDB.ProductsInCarts.Update(productInCart);
                _applicationDB.SaveChanges();
                InitializeProductsCart();
                return _productsCart;
            }
            else
            {
                productInCart = new ProductInCart() { ProductId = id, ProductsCartId = _productsCart.Id, Cost = product.Cost };
                _applicationDB.ProductsInCarts.Add(productInCart);
                _applicationDB.SaveChanges();
                InitializeProductsCart();
                return _productsCart;

            }
        }

        [HttpDelete("{id}")]
        public ActionResult<ProductsCart> Delete(int id)
        {
            SetProductsCart();
            InitializeProductsCart();

            ProductInCart productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.ProductId == id);

            if (productInCart == null)
                return NotFound();

            if (productInCart.Count > 1)
            {
                productInCart.Count--;
                _applicationDB.ProductsInCarts.Update(productInCart);
                _applicationDB.SaveChanges();
                InitializeProductsCart();
                return _productsCart;
            }
            else
            {
                _applicationDB.ProductsInCarts.Remove(productInCart);
                _applicationDB.SaveChanges();
                InitializeProductsCart();
                return _productsCart;
            }

        }

        [Route("buy")]
        [HttpPost]
        public ActionResult BuyCart()
        {
            SetProductsCart();
            InitializeProductsCart();
            if (_productsCart.ProductsCount < 1)
                return BadRequest();

            string companyName = _appConfiguration["CompanyDate:Name"];
            string companyEmail = _appConfiguration["CompanyDate:Email"];
            string companyPassword = _appConfiguration["CompanyDate:EmailPassword"];

            MailAddress from = new MailAddress(companyEmail, companyName);
            MailAddress to = new MailAddress(_currentUser.Email);

            string htmlString = System.IO.File.ReadAllText("Views/PurchaseEmail.html");
            var template = Template.Parse(htmlString);
            string result = template.Render(new
            {
                cartcost = _productsCart.ProductsCartCost,
                username = _currentUser.UserName,
                productsincart = _productsCart.ProductsInCart
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
            _applicationDB.ProductsInCarts.RemoveRange(_productsCart.ProductsInCart);
            _applicationDB.SaveChanges();
            return Ok();
        }

    }
}
