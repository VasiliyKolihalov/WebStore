using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebStoreAPI.Models
{
    public class OpenStoreRequest
    {
        public int Id { get; set; }  
        public User User { get; set; }
        public string Message { get; set; }
        public string StoreName { get; set; }
    }
}
