using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Scriban;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class OpenStoreRequestService
    {
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public OpenStoreRequestService(IApplicationContext applicationContext, UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _applicationDb = applicationContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        
        public User User { private get; set; }
        
        public IEnumerable<OpenStoreRequestViewModel> GetAll()
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
        
        public OpenStoreRequestViewModel Get(int requestId)
        {
            var request = _applicationDb.OpenStoreRequests.FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                throw new NotFoundException("request not found");

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
        
        public OpenStoreRequestViewModel Post(OpenStoreRequestAddModel requestAddModel)
        {
            if (!User.EmailConfirmed)
            {
                throw new BadRequestException("email not confirmed");
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
            request.User = User;

            _applicationDb.OpenStoreRequests.Add(request);
            _applicationDb.SaveChanges();

            var requestViewModel = mapper.Map<OpenStoreRequest, OpenStoreRequestViewModel>(request);

            return requestViewModel;
        }
        
        public OpenStoreRequestViewModel Accept(int requestId)
        {
            var request = _applicationDb.OpenStoreRequests.Include(x => x.User).FirstOrDefault(x => x.Id == requestId);

            if (request == null)
                throw new NotFoundException("request not found");

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
            return requestViewModel;
        }
    }
}