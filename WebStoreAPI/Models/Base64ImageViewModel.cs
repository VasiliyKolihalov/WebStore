using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class Base64ImageViewModel
    {
        
        public long Id { get; set; }
        
        public string Name { get; set; }
       
        public string ImageData { get; set; }

        public UserViewModel User { get; set; }

    }
}
