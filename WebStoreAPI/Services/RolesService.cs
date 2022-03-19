using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Exceptions;

namespace WebStoreAPI.Services
{
    public class RolesService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        
        public IEnumerable<IdentityRole> GetAll()
        {
            return _roleManager.Roles.ToList();
        }
        
        public IdentityRole Create(string roleName)
        {
            var role = new IdentityRole(roleName);
            IdentityResult result = _roleManager.CreateAsync(role).Result;

            if (result.Succeeded)
            {
                return role;
            }
            else
            {
                string errors = "failed to create role. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
        
        public IdentityRole Delete(string roleId)
        {
            var role = _roleManager.FindByIdAsync(roleId).Result;

            if (role == null)
                throw new NotFoundException("role not found");

            IdentityResult result = _roleManager.DeleteAsync(role).Result;

            if (result.Succeeded)
            {
                return role;
            }
            else
            {
                string errors = "failed to delete account. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
    }
}