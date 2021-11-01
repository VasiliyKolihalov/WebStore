using System;
using WebStoreAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebStoreAPI
{
    public class StartRoleInitializer
    {
        public static void Initialize(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration appConfiguration, ILogger<Program> logger)
        {
            if(roleManager.FindByNameAsync("admin").Result == null)
            {
                roleManager.CreateAsync(new IdentityRole("admin")).Wait();
            }

            if(roleManager.FindByNameAsync("user").Result == null)
            {
                roleManager.CreateAsync(new IdentityRole("user")).Wait();
            }

            if (userManager.FindByNameAsync("admin").Result == null)
            {
                string name = appConfiguration["AdminData:Name"];
                string email = appConfiguration["AdminData:Email"];
                string password = appConfiguration["AdminData:Password"];

                var admin = new User() {UserName = name, Email = email };
                var result = userManager.CreateAsync(admin, password).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(admin, "admin").Wait();
                }
                else
                {
                    foreach(var error in result.Errors)
                    {
                        logger.LogError(error.Description);
                    }
                }
            }
        }
    }
}
