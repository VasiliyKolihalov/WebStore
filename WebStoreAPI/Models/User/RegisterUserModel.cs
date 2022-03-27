using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WebStoreAPI.Models
{
    public class RegisterUserModel
    {
        [Required] public string Name { get; set; }
        [EmailAddress] [Required] public string Email { get; set; }
        [Required] public string Password { get; set; }

    }
}