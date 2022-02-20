using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class ProductsCartViewModel
    {
        public int Id { get; set; }
        public List<ProductInCartViewModel> ProductsInCart { get; set; }
        public decimal ProductsCartCost => ProductsInCart.Sum(x => x.Cost);
    }
}