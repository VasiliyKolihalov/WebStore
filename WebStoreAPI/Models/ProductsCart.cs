using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class ProductsCart
    {
        public int Id { get; set; }
        public List<ProductInCart> ProductsInCart { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }

        public ProductsCart()
        {
            ProductsInCart = new List<ProductInCart>();
        }
    }
}
