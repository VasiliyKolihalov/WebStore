using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager) 
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public  ActionResult<IEnumerable<User>> Get()
        {
            return  _userManager.Users.ToList();
        }

        [HttpDelete("{id}")]
        public ActionResult<User> Delete(string id)
        {
            var user = _userManager.FindByIdAsync(id).Result;
            if (user == null)
                return NotFound();

            var result = _userManager.DeleteAsync(user).Result;
            if (result.Succeeded)
            {
                return Ok(user);
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Route("{userId}/addrole/{roleName}")]
        public ActionResult<User> AddRole(string userId, string roleName)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (user == null || role == null)
                return NotFound();

            var result = _userManager.AddToRoleAsync(user, role.Name).Result;

            if (result.Succeeded)
            {
                _signInManager.RefreshSignInAsync(user).Wait();
                return Ok(user);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Route("{userId}/removerole/{roleName}")]
        public ActionResult<User> RemoveRole(string userId, string roleName)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (user == null || role == null)
                return NotFound();

            var result = _userManager.RemoveFromRoleAsync(user, role.Name).Result;

            if (result.Succeeded)
            {
                _signInManager.RefreshSignInAsync(user).Wait();
                return Ok(user);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }



    }
}
