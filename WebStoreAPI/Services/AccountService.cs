using System;
using System.Collections.Generic;
using System.Security.Policy;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Scriban;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace WebStoreAPI.Services
{
    public class AccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }
        
        public User User { private get; set; }
        
        public UserViewModel Get()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var userViewModel = mapper.Map<User, UserViewModel>(User);

            return userViewModel;
        }
        
        public UserViewModel Register(RegisterUserModel registerModel)
        {
            var user = new User {Email = registerModel.Email, UserName = registerModel.Name, RegionalCurrency = AvailableCurrencies.Rub};
            IdentityResult result = _userManager.CreateAsync(user, registerModel.Password).Result;

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserViewModel>().ForMember(
                nameof(UserViewModel.Name), opt =>
                    opt.MapFrom(x => x.UserName)));

            var mapper = new Mapper(mapperConfig);

            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(user, "user").Wait();
                _signInManager.SignInAsync(user, false).Wait();
                var userViewModel = mapper.Map<User, UserViewModel>(user);
                return userViewModel;
            }
            else
            {
                string errors = "failed to create account. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }

       
        public SignInResult Login(LoginUserModel loginModel)
        {
            SignInResult result = _signInManager
                .PasswordSignInAsync(loginModel.Name, loginModel.Password, loginModel.RememberMe, false).Result;

            if (result.Succeeded)
            {
                return result;
            }
            else
            {
                throw new BadRequestException("Incorrect login or password");
            }
        }
        
        public void Logout()
        {
            _signInManager.SignOutAsync().Wait();
        }

        public AvailableCurrencies ChangeRegionalСurrency(AvailableCurrencies currency)
        {
            User.RegionalCurrency = currency;
            IdentityResult result = _userManager.UpdateAsync(User).Result;
            if (result.Succeeded)
            {
                return User.RegionalCurrency;
            }
            else
            {
                string errors = "failed to change regional currency. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
        
        public void SendConfirmationCode(string callbackUrl)
        {
            if (callbackUrl == null)
                throw new BadRequestException("callbackUrl is null");
            
            var htmlString = System.IO.File.ReadAllText("Views/ConfirmEmail.html");
            Template template = Template.Parse(htmlString);
            string message = template.Render(new {callback_url = callbackUrl});

            var emailService = new EmailService(_configuration);
            emailService.SendEmail(User.Email, "Подтверждение почты", message);
        }
        
        public IdentityResult ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                throw new BadRequestException("user id or confirm code is null");
            }
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
            {
                throw new NotFoundException("user not found");
            }

            IdentityResult result = _userManager.ConfirmEmailAsync(user, code).Result;
            if (result.Succeeded)
            {
                return result;
            }
            else
            {
                string errors = "failed to confirm email. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }
                throw  new Exception(errors);
            }
        }
    
    }
}