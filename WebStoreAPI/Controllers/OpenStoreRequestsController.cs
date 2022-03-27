using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebStoreAPI.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Scriban;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OpenStoreRequestsController : ControllerBase
    {
        private readonly OpenStoreRequestService _openStoreRequestService;
        private readonly UserManager<User> _userManager;

        public OpenStoreRequestsController(OpenStoreRequestService openStoreRequestService,
            UserManager<User> userManager)
        {
            _openStoreRequestService = openStoreRequestService;
            _userManager = userManager;
        }

        private User GetUser() => _userManager.GetUserAsync(User).Result;

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpGet]
        public ActionResult<IEnumerable<OpenStoreRequestViewModel>> GetAll()
        {
            _openStoreRequestService.User = GetUser();
            List<OpenStoreRequestViewModel> requestViews = _openStoreRequestService.GetAll() as List<OpenStoreRequestViewModel>;

            return Ok(requestViews);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpGet("{requestId}")]
        public ActionResult<OpenStoreRequestViewModel> Get(int requestId)
        {
            _openStoreRequestService.User = GetUser();
            OpenStoreRequestViewModel requestView = _openStoreRequestService.Get(requestId);

            return Ok(requestView);
        }

        [HttpPost]
        public ActionResult<OpenStoreRequestViewModel> Post(OpenStoreRequestAddModel requestAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _openStoreRequestService.User = GetUser();
            OpenStoreRequestViewModel requestView = _openStoreRequestService.Post(requestAddModel);

            return Ok(requestView);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [Route("{requestId}/accept")]
        [HttpPost]
        public ActionResult<OpenStoreRequestViewModel> Accept(int requestId)
        {
            _openStoreRequestService.User = GetUser();
            OpenStoreRequestViewModel requestView = _openStoreRequestService.Accept(requestId);

            return Ok(requestView);
        }
    }
}