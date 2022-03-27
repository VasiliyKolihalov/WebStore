using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _usersService;
        
        public UsersController(UsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserViewModel>> GetAll()
        {
            List<UserViewModel> userViews = _usersService.GetAll() as List<UserViewModel>;
            return Ok(userViews);
        }

        [HttpGet("{userId}")]
        public ActionResult<UserViewModel> Get(string userId)
        {
            UserViewModel userView = _usersService.Get(userId);
            return Ok(userView);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpDelete("{userId}")]
        public ActionResult<UserViewModel> Delete(string userId)
        {
            UserViewModel userView = _usersService.Delete(userId);
            return Ok(userView);
        }
        
        [HttpGet("{userId}/getRoles")]
        public ActionResult<IEnumerable<string>> GetRoles(string userId)
        {
            List<string> roles = _usersService.GetRoles(userId) as List<string>;
            return Ok(roles);
        }
        
        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpPost]
        [Route("{userId}/addRole/{roleName}")]
        public ActionResult<UserViewModel> AddRole(string userId, string roleName)
        {
            UserViewModel userView = _usersService.AddRole(userId, roleName);
            return Ok(userView);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpPost]
        [Route("{userId}/removeRole/{roleName}")]
        public ActionResult<UserViewModel> RemoveRole(string userId, string roleName)
        {
            UserViewModel userView = _usersService.RemoveRole(userId, roleName);
            return Ok(userView);
        }
    }
}