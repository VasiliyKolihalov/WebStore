using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class Product
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Min(0.0)]
        public decimal Cost { get; set; }
        [Required]
        public string Description { get; set; }
        [Min(0)]
        public int QuantityInStock { get; set; }

    }
}
