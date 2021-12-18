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

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenStoreRequestsController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private User _user;

        public OpenStoreRequestsController(ApplicationContext applicationContext, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _applicationDB = applicationContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpGet]
        public ActionResult<IEnumerable<OpenStoreRequestViewModel>> GetAll()
        {
            var requests = _applicationDB.OpenStoreRequests.Include(x => x.User);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            var requestViewModels = mapper.Map<IEnumerable<OpenStoreRequest>, List<OpenStoreRequestViewModel>>(requests);

            return requestViewModels;
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpGet("{requestId}")]
        public ActionResult<OpenStoreRequestViewModel> Get(int requestId)
        {
            var request = _applicationDB.OpenStoreRequests.FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);

            return requestViewModel;
        }

        [Authorize]
        [HttpPost]
        public ActionResult<OpenStoreRequestViewModel> Post(OpenStoreRequestAddModel requestAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            SetUser();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequestAddModel, OpenStoreRequest>();
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var request = mapper.Map<OpenStoreRequestAddModel, OpenStoreRequest>(requestAddModel);
            request.User = _user;

            _applicationDB.OpenStoreRequests.Add(request);
            _applicationDB.SaveChanges();

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);

            return Ok(requestViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [Route("{requestId}/accept")]
        [HttpPost]
        public ActionResult<OpenStoreRequestViewModel> Accept(int requestId)
        {
            var request = _applicationDB.OpenStoreRequests.Include(x => x.User).FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                return NotFound();

            var sellerRole = _roleManager.FindByNameAsync("seller").Result;
            _userManager.AddToRoleAsync(request.User, sellerRole.Name);

            Store store = new Store()
            {
                Name = request.StoreName,
                Seller = request.User
            };

            _applicationDB.Stores.Add(store);
            _applicationDB.OpenStoreRequests.Remove(request);
            _applicationDB.SaveChanges();


            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OpenStoreRequest, OpenStoreRequestViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);
            return Ok(requestViewModel);
        }
    }
}
