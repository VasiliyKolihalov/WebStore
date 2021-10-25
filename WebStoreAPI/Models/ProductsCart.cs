using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class ProductsCart
    {
        private ProductsContext _productsDb;
        private readonly List<Product> _products;
        public ProductsCart()
        {
            _products = new List<Product>();
        }

        public IEnumerable<Product> Get()
        {
            return _products;
        }

        public void Add(int id, ProductsContext productsContext)
        {
            _productsDb = productsContext;
            var product = _productsDb.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                throw new Exception("product not found in BD");

            _products.Add(product);
        }

        public void Delete(int id)
        {          
            var product = _products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                throw new Exception("no product in the cart");

            _products.Remove(product);
        }
    }
}
