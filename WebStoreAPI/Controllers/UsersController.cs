﻿using Microsoft.AspNetCore.Http;
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

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserViewModel>> GetAll()
        {
            var users = _userManager.Users;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var userViewModels = mapper.Map<IEnumerable<User>, List<UserViewModel>>(users);
            return userViewModels;
        }

        [HttpGet("{userId}")]
        public ActionResult<UserViewModel> Get(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            
            if (user == null)
                return NotFound();
            
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var userViewModel = mapper.Map<User, UserViewModel>(user);
            return userViewModel;
        }
        
        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpDelete("{userId}")]
        public ActionResult<UserViewModel> Delete(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
                return NotFound();

            IdentityResult result = _userManager.DeleteAsync(user).Result;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            if (result.Succeeded)
            {
                var userViewModel = mapper.Map<User, UserViewModel>(user);
                return Ok(userViewModel);
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
        
        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpPost]
        [Route("{userId}/addRole/{roleName}")]
        public ActionResult<UserViewModel> AddRole(string userId, string roleName)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (user == null || role == null)
                return NotFound();

            IdentityResult result = _userManager.AddToRoleAsync(user, role.Name).Result;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            if (result.Succeeded)
            {
                _signInManager.RefreshSignInAsync(user).Wait();
                var userViewModel = mapper.Map<User, UserViewModel>(user);
                return Ok(userViewModel);
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

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpPost]
        [Route("{userId}/removeRole/{roleName}")]
        public ActionResult<UserViewModel> RemoveRole(string userId, string roleName)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (user == null || role == null)
                return NotFound();

            IdentityResult result = _userManager.RemoveFromRoleAsync(user, role.Name).Result;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            if (result.Succeeded)
            {
                _signInManager.RefreshSignInAsync(user).Wait();
                var userViewModel = mapper.Map<User, UserViewModel>(user);
                return Ok(userViewModel);
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