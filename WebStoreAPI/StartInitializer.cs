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
    public static class StartInitializer
    {
        public static void Initialize(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration appConfiguration, ILogger<Program> logger)
        {
            if(roleManager.FindByNameAsync(RolesConstants.AdminRoleName).Result == null)
            {
                roleManager.CreateAsync(new IdentityRole(RolesConstants.AdminRoleName)).Wait();
            }

            if(roleManager.FindByNameAsync(RolesConstants.UserRoleName).Result == null)
            {
                roleManager.CreateAsync(new IdentityRole(RolesConstants.UserRoleName)).Wait();
            }

            if (roleManager.FindByNameAsync(RolesConstants.SellerRoleName).Result == null)
            {
                roleManager.CreateAsync(new IdentityRole(RolesConstants.SellerRoleName)).Wait();
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
                    userManager.AddToRoleAsync(admin, RolesConstants.AdminRoleName).Wait();
                    userManager.AddToRoleAsync(admin, RolesConstants.SellerRoleName).Wait();
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
