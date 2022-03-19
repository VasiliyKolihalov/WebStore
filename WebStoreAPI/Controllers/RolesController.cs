using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = ApplicationConstants.AdminRoleName)]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RolesService _rolesService;

        public RolesController(RolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<IdentityRole>> GetAll()
        {
            return Ok(_rolesService.GetAll());
        }

        [HttpPost("{roleName}")]
        public ActionResult<IdentityRole> Create(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest();

            IdentityRole role = _rolesService.Create(roleName);
            return Ok(role);
        }

        [HttpDelete("{roleId}")]
        public ActionResult<IdentityRole> Delete(string roleId)
        {
            IdentityRole role = _rolesService.Delete(roleId);
            return Ok(role);
        }
    }
}