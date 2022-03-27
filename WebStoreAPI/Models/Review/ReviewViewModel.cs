namespace WebStoreAPI.Models
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public UserViewModel User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}