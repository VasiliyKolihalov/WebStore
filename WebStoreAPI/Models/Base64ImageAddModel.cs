using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class Base64ImageAddModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ImageData { get; set; }
    }
}
