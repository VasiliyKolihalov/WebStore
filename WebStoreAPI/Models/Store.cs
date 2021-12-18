using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebStoreAPI.Models
{
    public class Store
    {   
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public User Seller { get; set; }
        public List<Product> Products { get; set; }
        public List<Image> Images { get; set; }

        public Store()
        {
            Products = new List<Product>();
            Images = new List<Image>();
        }
    }
}
