using System;
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
using Microsoft.Extensions.Caching.Memory;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsCartController : ControllerBase
    {
        private readonly ProductsCartService _productsCartService;
        private readonly UserManager<User> _userManager;

        public ProductsCartController(ProductsCartService productsCartService, UserManager<User> userManager)
        {
            _productsCartService = productsCartService;
            _userManager = userManager;
        }

        private User GetUser()
        {
            return _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<ProductsCartViewModel> Get()
        {
            _productsCartService.User = GetUser();
            ProductsCartViewModel productsCart = _productsCartService.Get();

            return Ok(productsCart);
        }


        [HttpPost("{productId}")]
        public ActionResult<ProductInCartViewModel> AddProduct(long productId)
        {
            _productsCartService.User = GetUser();
            ProductInCartViewModel productInCart = _productsCartService.AddProduct(productId);

            return Ok(productInCart);
        }

        [HttpDelete("{productId}")]
        public ActionResult<ProductInCartViewModel> DeleteProduct(long productId)
        {
            _productsCartService.User = GetUser();
            ProductInCartViewModel productInCart = _productsCartService.DeleteProduct(productId);

            return Ok(productInCart);
        }

        [Route("select/{id}")]
        [HttpPut]
        public ActionResult<ProductInCartViewModel> SelectProduct(long id)
        {
            _productsCartService.User = GetUser();
            ProductInCartViewModel productInCart = _productsCartService.SelectProduct(id);
            
            return Ok(productInCart);
        }

        [Route("buy")]
        [HttpPost]
        public ActionResult<IEnumerable<ProductInCartViewModel>> BuySelectedProducts()
        {
            _productsCartService.User = GetUser();
            List<ProductInCartViewModel> productsInCart = _productsCartService.BuySelectedProducts() as List<ProductInCartViewModel>;
            
            return Ok(productsInCart);
        }
    }
}