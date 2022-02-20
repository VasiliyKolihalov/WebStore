using System.ComponentModel.DataAnnotations;

namespace WebStoreAPI.Models
{
    public class ReviewAddModel
    {
        [Required] public int ProductId { get; set; }
        [Required] public int Rating { get; set; }
        public string Comment { get; set; }
    }
}