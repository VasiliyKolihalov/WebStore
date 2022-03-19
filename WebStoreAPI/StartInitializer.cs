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
        public static void Initialize(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration appConfiguration, ILogger<Program> logger)
        {
            if (roleManager.FindByNameAsync(ApplicationConstants.AdminRoleName).Result == null)
            {
                roleManager.CreateAsync(new IdentityRole(ApplicationConstants.AdminRoleName)).Wait();
            }

            if (roleManager.FindByNameAsync(ApplicationConstants.UserRoleName).Result == null)
            {
                roleManager.CreateAsync(new IdentityRole(ApplicationConstants.UserRoleName)).Wait();
            }

            if (roleManager.FindByNameAsync(ApplicationConstants.SellerRoleName).Result == null)
            {
                roleManager.CreateAsync(new IdentityRole(ApplicationConstants.SellerRoleName)).Wait();
            }

            if (userManager.FindByNameAsync("admin").Result == null)
            {
                string name = appConfiguration["AdminData:Name"];
                string email = appConfiguration["AdminData:Email"];
                string password = appConfiguration["AdminData:Password"];

                var admin = new User {UserName = name, Email = email, RegionalCurrency = AvailableCurrencies.Rub};
                var result = userManager.CreateAsync(admin, password).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(admin, ApplicationConstants.AdminRoleName).Wait();
                    userManager.AddToRoleAsync(admin, ApplicationConstants.SellerRoleName).Wait();
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        logger.LogError(error.Description);
                    }
                }
            }
        }
    }
}