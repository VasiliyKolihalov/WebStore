using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class ProductInCartViewModel
    {
        public int Id { get; set; }
        public ProductViewModel Product { get; set; }
        public int Count { get; set; }
        public decimal Cost { get; set; }
        public bool Selected { get; set; }

        public bool CanBuy { get => Product.QuantityInStock >= Count; }

    }
}
