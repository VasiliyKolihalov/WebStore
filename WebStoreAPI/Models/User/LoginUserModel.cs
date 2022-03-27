using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class LoginUserModel
    {
        [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}