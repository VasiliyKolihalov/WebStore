using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;
        private User _user;

        public StoresController(IApplicationContext applicationContext, UserManager<User> userManager)
        {
            _applicationDb = applicationContext;
            _userManager = userManager;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StoreViewModel>> GetAll()
        {
            var stores = _applicationDb.Stores.Include(x => x.Seller);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Store, StoreViewModel>();

                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var storeViewModels = mapper.Map<IEnumerable<Store>, List<StoreViewModel>>(stores);

            return storeViewModels;
        }

        [HttpGet("{storeId}")]
        public ActionResult<StoreViewModel> Get(int storeId)
        {
            var store = _applicationDb.Stores.FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Store, StoreViewModel>();

                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var storeViewModel = mapper.Map<Store, StoreViewModel>(store);

            return storeViewModel;
        }


        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpDelete("{storeId}")]
        public ActionResult<StoreViewModel> Delete(int storeId)
        {
            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            var store = _applicationDb.Stores.Include(x => x.Seller).FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                return NotFound();

            if (!userRoles.Contains("admin") && store.Seller.Id != _user.Id)
                return BadRequest();

            _applicationDb.Stores.Remove(store);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Store, StoreViewModel>();

                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var storeViewModel = mapper.Map<Store, StoreViewModel>(store);

            return Ok(storeViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpPut]
        public ActionResult<StoreViewModel> Put(StorePutModel storePutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            var store = _applicationDb.Stores.Include(x => x.Seller)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == storePutModel.Id);

            if (store == null)
                return NotFound();

            if (!userRoles.Contains("admin") && store.Seller.Id != _user.Id)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cgf => cgf.CreateMap<StorePutModel, Store>());
            var mapper = new Mapper(mapperConfig);

            store = mapper.Map<StorePutModel, Store>(storePutModel);

            _applicationDb.Stores.Update(store);
            _applicationDb.SaveChanges();

            return Ok(storePutModel);
        }
    }
}