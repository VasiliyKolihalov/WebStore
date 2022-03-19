using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebStoreAPI.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebStoreAPI.Services;
using Scriban;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly UserManager<User> _userManager;

        public AccountController(AccountService accountService ,UserManager<User> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        private User GetUser()
        {
            return _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [Authorize]
        [HttpGet]
        public ActionResult<UserViewModel> Get()
        {
            _accountService.User = GetUser();
            UserViewModel userView = _accountService.Get();

            return Ok(userView);
        }

        [Route("register")]
        [HttpPost]
        public ActionResult<UserViewModel> Register(RegisterUserModel registerModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            _accountService.User = GetUser();
            UserViewModel userView = _accountService.Register(registerModel);
            return Ok(userView);
        }

        [Route("login")]
        [HttpPost]
        public ActionResult<SignInResult> Login(LoginUserModel loginModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            
            _accountService.User = GetUser();
            SignInResult signInResult = _accountService.Login(loginModel);
            return Ok(signInResult);
        }

        [Authorize]
        [Route("logout")]
        [HttpPost]
        public ActionResult Logout()
        {
            _accountService.User = GetUser();
            _accountService.Logout();
            return Ok();
        }

        [Authorize]
        [Route("changeRegionalСurrency/{currency}")]
        [HttpPut]
        public ActionResult<AvailableCurrencies> ChangeRegionalСurrency(AvailableCurrencies currency)
        {
            _accountService.User = GetUser();
            _accountService.ChangeRegionalСurrency(currency);
            return Ok(currency);
        }

        [Authorize]
        [Route("sendConfirmationCode")]
        [HttpGet]
        public ActionResult SendConfirmationCode()
        {
            var user = GetUser();
            string confirmCode = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;
            string callbackUrl = Url.Action(
                action: nameof(_accountService.ConfirmEmail),
                controller: "Account",
                values: new {userId = user.Id, code = confirmCode},
                protocol: HttpContext.Request.Scheme);

            _accountService.User = user;
            _accountService.SendConfirmationCode(callbackUrl);
            return Ok();
        }

        [AllowAnonymous]
        [Route("confirmEmail")]
        [HttpGet]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            _accountService.User = GetUser();
            _accountService.ConfirmEmail(userId, code);
            return Ok();
        }
    }
}