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
    [Authorize(Roles = "admin")]
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
        public ActionResult<IEnumerable<IdentityRole>> Get()
        {
            return _roleManager.Roles.ToList();
        }


        [HttpPost("{name}")]
        public ActionResult<IdentityRole> Create(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var role = new IdentityRole(name);
            var result = _roleManager.CreateAsync(role).Result;

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

        [HttpDelete("{id}")]
        public ActionResult<IdentityRole> Delete(string id)
        {
            var role = _roleManager.FindByIdAsync(id).Result;

            if (role == null)
                return NotFound();

            var result = _roleManager.DeleteAsync(role).Result;

            if (result.Succeeded)
            {
                return Ok(role);
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

    }
}
