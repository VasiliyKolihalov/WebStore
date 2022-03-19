using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class UsersService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UsersService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        
        public IEnumerable<UserViewModel> GetAll()
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
        
        public UserViewModel Get(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
                throw new NotFoundException("user not found");
            
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var userViewModel = mapper.Map<User, UserViewModel>(user);
            return userViewModel;
        }
        
        public UserViewModel Delete(string userId)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
                throw new NotFoundException("user not found");

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
                return userViewModel;
            }
            else
            {
                string errors = "failed to delete user. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
        
        public UserViewModel AddRole(string userId, string roleName)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            var role = _roleManager.FindByNameAsync(roleName).Result;
            
            if (user == null || role == null)
                throw new NotFoundException("user not found");

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
                return userViewModel;
            }
            else
            {
                string errors = "failed to add role. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
        
        public UserViewModel RemoveRole(string userId, string roleName)
        {
            var user = _userManager.Users.FirstOrDefault(x => x.Id == userId);
            var role = _roleManager.FindByNameAsync(roleName).Result;
            if (user == null || role == null)
                throw new NotFoundException("user not found");
            
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
                return userViewModel;
            }
            else
            {
                string errors = "failed to remove role. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
    }
}