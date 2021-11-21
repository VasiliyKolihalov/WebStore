using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebStoreAPI.Models
{
    public class ProductInCart
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public List<ProductsCart> ProductsCarts { get; set; }
        public int Count { get; set; } 
        public decimal Cost { get; set; }
        public bool Selected { get; set; }

        [NotMapped]
        public bool CanBuy { get => Product.QuantityInStock >= Count; }

        public ProductInCart()
        {
            ProductsCarts = new List<ProductsCart>();
            Count = 1;
        }
    }
}
