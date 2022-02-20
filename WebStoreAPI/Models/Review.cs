namespace WebStoreAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public User User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}