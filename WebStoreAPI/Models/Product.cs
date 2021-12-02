using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; }
        public int QuantityInStock { get; set; }
        public List<Image> Images { get; set; }
        public List<Category> Categories { get; set; }

        public Product()
        {
            Images = new List<Image>();
            Categories = new List<Category>();
        }
    }
}
