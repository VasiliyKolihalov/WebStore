using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class OpenStoreRequestViewModel
    {
        public int Id { get; set; }
        public UserViewModel User { get; set; }
        public string Message { get; set; }
        public string StoreName { get; set; }
    }
}