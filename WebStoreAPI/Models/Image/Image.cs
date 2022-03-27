using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class Image
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public byte[] ImageData { get; set; }
        public User User { get; set; }
    
    }
}
