using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public ActionResult<UserViewModel> Get()
        {
            User user = _userManager.GetUserAsync(HttpContext.User).Result;
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt =>
                                                                                                   opt.MapFrom(x => x.UserName)));
            var mapper = new Mapper(mapperConfig);
            var userViewModel = mapper.Map<User, UserViewModel>(user);
            return userViewModel;
        }

        [HttpPost]
        [Route("register")]
        public ActionResult<UserViewModel> Register(RegisterUserModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new User() { Email = registerModel.Email, UserName = registerModel.Name };
            var result = _userManager.CreateAsync(user, registerModel.Password).Result;

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt =>
                                                                                                   opt.MapFrom(x => x.UserName)));
            var mapper = new Mapper(mapperConfig);

            if (result.Succeeded)
            {
                 _userManager.AddToRoleAsync(user, "user").Wait();
                 _signInManager.SignInAsync(user, false).Wait();
                 var userViewModel = mapper.Map<User, UserViewModel>(user);
                 return Ok(userViewModel);
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
        [Route("login")]
        public ActionResult<SignInResult> Login(LoginUserModel loginModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result =  _signInManager.PasswordSignInAsync(loginModel.Name, loginModel.Password, loginModel.RememberMe, false).Result;

            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Incorrect login or password");
                return BadRequest(ModelState);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("logout")]
        public ActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return Ok();
        }
    }
}
