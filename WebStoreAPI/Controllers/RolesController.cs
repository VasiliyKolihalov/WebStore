using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = ApplicationConstants.AdminRoleName)]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<IdentityRole>> GetAll()
        {
            return _roleManager.Roles.ToList();
        }

        [HttpPost("{roleName}")]
        public ActionResult<IdentityRole> Create(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest();

            var role = new IdentityRole(roleName);
            IdentityResult result = _roleManager.CreateAsync(role).Result;

            if (result.Succeeded)
            {
                return Ok(role);
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

        [HttpDelete("{roleId}")]
        public ActionResult<IdentityRole> Delete(string roleId)
        {
            var role = _roleManager.FindByIdAsync(roleId).Result;

            if (role == null)
                return NotFound();

            IdentityResult result = _roleManager.DeleteAsync(role).Result;

            if (result.Succeeded)
            {
                return Ok(role);
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