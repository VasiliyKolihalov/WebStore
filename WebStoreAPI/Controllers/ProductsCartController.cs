using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;


namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsCartController : ControllerBase
    {
        private readonly ProductsCart _productsCart;

        public ProductsCartController(ProductsCart productsCart)
        {
            _productsCart = productsCart;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get()
        {
            return _productsCart.Get().ToList();
        }

        [HttpPost("{id}")]
        public ActionResult<Product> Post(int id)
        {
            try
            {
                _productsCart.Add(id, HttpContext.RequestServices.GetService<ProductsContext>());
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<ProductsContext> Delete(int id)
        {
            try
            {
                _productsCart.Delete(id);
                return Ok();
            }
            catch
            {
                return NotFound();
            }
        }

    }
}
