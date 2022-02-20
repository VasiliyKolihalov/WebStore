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
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private User _user;

        public OpenStoreRequestsController(IApplicationContext applicationContext, UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _applicationDb = applicationContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpGet]
        public ActionResult<IEnumerable<OpenStoreRequestViewModel>> GetAll()
        {
            var requests = _applicationDb.OpenStoreRequests.Include(x => x.User);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            var requestViewModels =
                mapper.Map<IEnumerable<OpenStoreRequest>, List<OpenStoreRequestViewModel>>(requests);

            return requestViewModels;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpGet("{requestId}")]
        public ActionResult<OpenStoreRequestViewModel> Get(int requestId)
        {
            var request = _applicationDb.OpenStoreRequests.FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);

            return requestViewModel;
        }

        [HttpPost]
        public ActionResult<OpenStoreRequestViewModel> Post(OpenStoreRequestAddModel requestAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            SetUser();
            if (!_user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "email not confirmed");
                return BadRequest(ModelState);
            }

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequestAddModel, OpenStoreRequest>();

                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var request = mapper.Map<OpenStoreRequestAddModel, OpenStoreRequest>(requestAddModel);
            request.User = _user;

            _applicationDb.OpenStoreRequests.Add(request);
            _applicationDb.SaveChanges();

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);

            return Ok(requestViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [Route("{requestId}/accept")]
        [HttpPost]
        public ActionResult<OpenStoreRequestViewModel> Accept(int requestId)
        {
            var request = _applicationDb.OpenStoreRequests.Include(x => x.User).FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                return NotFound();

            IdentityRole sellerRole = _roleManager.FindByNameAsync("seller").Result;
            _userManager.AddToRoleAsync(request.User, sellerRole.Name);

            var store = new Store()
            {
                Name = request.StoreName,
                Seller = request.User
            };

            _applicationDb.Stores.Add(store);
            _applicationDb.OpenStoreRequests.Remove(request);
            _applicationDb.SaveChanges();

            var htmlString = System.IO.File.ReadAllText("Views/OpenStoreEmail.html");
            Template template = Template.Parse(htmlString);
            string message = template.Render(new {user_name = request.User.UserName, store_name = store.Name});
            
            var emailService = new EmailService(_configuration);
            emailService.SendEmail(request.User.Email, "Заявка принята", message);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);
            return Ok(requestViewModel);
        }
    }
}