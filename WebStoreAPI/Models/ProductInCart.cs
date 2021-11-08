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
        private bool _selected;

        public int Id { get; set; }
        public int ProductsCartId { get; set; }
        public long ProductId { get; set; }
        public int Count { get; set; } 
        public decimal Cost { get; set; }
        public bool Selected
        {
            get => _selected;

            set
            {
                if (CanBuy == false)
                    _selected = false;

                _selected = value;
            }
        }

        [NotMapped]
        public Product Product { get; set; }
        [NotMapped]
        public bool CanBuy { get => Product.QuantityInStock >= 1; }

        public ProductInCart()
        {
            Count = 1;
        }
    }
}
