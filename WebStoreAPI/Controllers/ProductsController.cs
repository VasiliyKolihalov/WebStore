using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using WebStoreAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;

        public ProductsController(ApplicationContext productsContext)
        {
            _applicationDB = productsContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get()
        {
            return _applicationDB.Products.ToList();
        }

        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();
            return new ObjectResult(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPost] 
        public ActionResult<Product> Post(Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest();
         
            _applicationDB.Products.Add(product);
            _applicationDB.SaveChanges();
            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public ActionResult<Product> Put(Product product)
        {
            if (product == null) return BadRequest();

            if(!_applicationDB.Products.Any(x => x.Id == product.Id))
            {
                return NotFound();
            }

            _applicationDB.Update(product);
            _applicationDB.SaveChanges();
            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<Product> Delete(int id)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();
            _applicationDB.Remove(product);
            _applicationDB.SaveChanges();
            return Ok(product);
        }
    }
}
