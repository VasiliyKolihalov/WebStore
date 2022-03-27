using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scriban;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace WebStoreAPI.Services
{
    public class AccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly IOptions<JwtAuthenticationOptions> _jwtAuthOptions;
        private readonly IConfiguration _configuration;

        public AccountService(UserManager<User> userManager, IOptions<JwtAuthenticationOptions> jwtAuthOptions,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtAuthOptions = jwtAuthOptions;
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

        public string Register(RegisterUserModel registerModel)
        {
            var user = new User
            {
                Email = registerModel.Email, UserName = registerModel.Name, RegionalCurrency = AvailableCurrencies.Rub
            };
            IdentityResult result = _userManager.CreateAsync(user, registerModel.Password).Result;


            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(user, "user").Wait();
                var token = GenerateJwt(user);
                return token;
            }
            else
            {
                string errors = "failed to create account. Errors: ";
                foreach (var error in result.Errors)
                {
                    errors += " " + error.Description;
                }

                throw new BadRequestException(errors);
            }
        }


        public string Login(LoginUserModel loginModel)
        {
            var user = _userManager.FindByEmailAsync(loginModel.Email).Result;
            if (!_userManager.CheckPasswordAsync(user, loginModel.Password).Result)
            {
                throw new BadRequestException("Incorrect login or password");
            }
            
            var token = GenerateJwt(user);
            return token;
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

                throw new BadRequestException(errors);
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

                throw new Exception(errors);
            }
        }
        
        private string GenerateJwt(User user)
        {
            JwtAuthenticationOptions jwtAuthenticationOptions = _jwtAuthOptions.Value;

            SymmetricSecurityKey securityKey = jwtAuthenticationOptions.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id)
            };

            IList<string> roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            var token = new JwtSecurityToken(
                jwtAuthenticationOptions.Issuer, 
                jwtAuthenticationOptions.Audience, 
                claims,
                expires: DateTime.Now.AddMinutes(jwtAuthenticationOptions.TokenMinuteLifetime),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}