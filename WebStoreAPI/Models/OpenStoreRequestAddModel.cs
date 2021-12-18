using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebStoreAPI.Models
{
    public class OpenStoreRequestAddModel
    {
        [Required]
        public string Message { get; set; }
        [Required]
        public string StoreName { get; set; }
    }
}
