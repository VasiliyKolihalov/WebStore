using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using WebStoreAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsContext _productsDB;

        public ProductsController(ProductsContext productsContext)
        {
            _productsDB = productsContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get()
        {
            return _productsDB.Products.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = _productsDB.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();
            return new ObjectResult(product);
        }

        [HttpPost] 
        public ActionResult<Product> Post(Product product)
        {
            if (product == null) return BadRequest();
            _productsDB.Products.Add(product);
            _productsDB.SaveChanges();
            return Ok(product);
        }

        [HttpPut]
        public ActionResult<Product> Put(Product product)
        {
            if (product == null) return BadRequest();

            if(!_productsDB.Products.Any(x => x.Id == product.Id))
            {
                return NotFound();
            }

            _productsDB.Update(product);
            _productsDB.SaveChanges();
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public ActionResult<Product> Delete(int id)
        {
            var product = _productsDB.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();
            _productsDB.Remove(product);
            _productsDB.SaveChanges();
            return Ok(product);
        }
    }
}
