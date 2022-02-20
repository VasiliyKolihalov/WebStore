using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Store Store { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Image> Images { get; set; }
        public List<Category> Categories { get; set; }
        public List<Tag> Tags { get; set; }

        [NotMapped]
        public double? Rating
        {
            get
            {
                if (Reviews.Count == 0)
                    return null;

                return (double) Reviews.Sum(x => x.Rating) / Reviews.Count;
            }
        }

        public Product()
        {
            Reviews = new List<Review>();
            Images = new List<Image>();
            Categories = new List<Category>();
            Tags = new List<Tag>();
        }
    }
}