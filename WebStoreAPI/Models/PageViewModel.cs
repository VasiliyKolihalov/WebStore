using System.Collections.Generic;

namespace WebStoreAPI.Models
{
    public class PageViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; }
        public PageData PageData { get; set; }
    }
}