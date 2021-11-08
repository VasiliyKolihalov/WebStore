﻿using Newtonsoft.Json;
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
        public string UserId { get; set; }
        public List<ProductInCart> ProductsInCart { get; set; }

        [NotMapped]
        public List<ProductInCart> SelectedProductsInCart { get; set; }
        [NotMapped]
        public decimal ProductsCartCost { get => ProductsInCart.Sum(x => x.Cost); }

        public ProductsCart()
        {
            ProductsInCart = new List<ProductInCart>();
        }
    }
}
