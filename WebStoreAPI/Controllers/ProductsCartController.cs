using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

namespace WebStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsCartController : ControllerBase
    {
        private ProductsCart _productsCart;
        private readonly ApplicationContext _applicationDB;
        private readonly UserManager<User> _userManager;

        public ProductsCartController(ApplicationContext productsContext, UserManager<User> userManager)
        {
            _userManager = userManager;
            _applicationDB = productsContext;
        }

        private void SetProductsCart()
        {
            var user = _userManager.GetUserAsync(HttpContext.User).Result;
            _productsCart = _applicationDB.ProductsCarts.Single(x => x.UserId == user.Id);
        }

        private void InitializeProductsCart()
        {
            _productsCart.ProductsInCart = _applicationDB.ProductsInCarts.Where(x => x.ProductsCartId == _productsCart.Id).ToList();

            foreach(var productInCar in _productsCart.ProductsInCart)
            {
                productInCar.Product = _applicationDB.Products.Single(x => x.Id == productInCar.ProductId);
            }
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

    }
}
