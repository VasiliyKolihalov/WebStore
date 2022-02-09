using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebStoreAPI.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using WebStoreAPI.Services;
using Scriban;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private User _user;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [Authorize]
        [HttpGet]
        public ActionResult<UserViewModel> Get()
        {
            SetUser();
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<User, UserViewModel>().ForMember(
                nameof(UserViewModel.Name), opt =>
                    opt.MapFrom(x => x.UserName)));
            var mapper = new Mapper(mapperConfig);

            var userViewModel = mapper.Map<User, UserViewModel>(_user);
            return userViewModel;
        }
        
        [Route("register")]
        [HttpPost]
        public ActionResult<UserViewModel> Register(RegisterUserModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new User() {Email = registerModel.Email, UserName = registerModel.Name};
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
        
        [Route("login")]
        [HttpPost]
        public ActionResult<SignInResult> Login(LoginUserModel loginModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            SignInResult result = _signInManager
                .PasswordSignInAsync(loginModel.Name, loginModel.Password, loginModel.RememberMe, false).Result;

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
        [Route("logout")]
        [HttpPost]
        public ActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return Ok();
        }

        [Authorize]
        [Route("sendConfirmationCode")]
        [HttpGet]
        public ActionResult SendConfirmationCode()
        {
            SetUser();
            string confirmCode = _userManager.GenerateEmailConfirmationTokenAsync(_user).Result;
            
            string callbackUrl = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Account",
                values: new {userId = _user.Id, code = confirmCode},
                protocol: HttpContext.Request.Scheme);

            var htmlString = System.IO.File.ReadAllText("Views/ConfirmEmail.html");
            Template template = Template.Parse(htmlString);
            string message = template.Render(new {callback_url = callbackUrl});

            var emailService = new EmailService(_configuration);
            emailService.SendEmail(_user.Email, "Подтверждение почты", message);
            return Ok();
        }

        [AllowAnonymous]
        [Route("confirmEmail")]
        [HttpGet]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }
            
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
            {
                return NotFound();
            }

            IdentityResult result = _userManager.ConfirmEmailAsync(user, code).Result;
            if (result.Succeeded)
            {
                return Ok(result);
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