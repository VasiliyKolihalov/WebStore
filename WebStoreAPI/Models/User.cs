using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebStoreAPI.Models
{
    public class User : IdentityUser
    {
        public ProductsCart ProductsCart { get; set; }
        public User()
        {
            ProductsCart = new ProductsCart();
        }
    }
}