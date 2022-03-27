using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace WebStoreAPI.Models
{
    public class ReviewAddModel
    {
        [Required] public int ProductId { get; set; }

        [Required]
        [Min(RatingConstants.MinRating)]
        [Max(RatingConstants.MaxRating)]
        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}