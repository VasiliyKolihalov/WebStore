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
        private readonly IApplicationContext _applicationDB;
        private readonly UserManager<User> _userManager;
        private User _user;

        public StoresController(IApplicationContext applicationContext, UserManager<User> userManager)
        {
            _applicationDB = applicationContext;
            _userManager = userManager;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }
        
        [HttpGet]
        public ActionResult<IEnumerable<StoreViewModel>> GetAll()
        {
            var stores = _applicationDB.Stores.Include(x => x.Seller);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Store, StoreViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var storeViewModels = mapper.Map<IEnumerable<Store>, List<StoreViewModel>>(stores);

            return storeViewModels;
        }

        [HttpGet("{storeId}")]
        public ActionResult<StoreViewModel> Get(int storeId)
        {
            var store = _applicationDB.Stores.FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Store, StoreViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var storeViewModel = mapper.Map<Store,StoreViewModel>(store);

            return storeViewModel;
        }




        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpDelete("{storeId}")]
        public ActionResult<StoreViewModel> Delete(int storeId)
        {
            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            var store = _applicationDB.Stores.Include(x => x.Seller).FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                return NotFound();

            if (!userRoles.Contains("admin") && store.Seller.Id != _user.Id)               
                    return BadRequest();

            _applicationDB.Stores.Remove(store);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Store, StoreViewModel>();
                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var storeViewModel = mapper.Map<Store, StoreViewModel>(store);

            return Ok(storeViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpPut]
        public ActionResult<StoreViewModel> Put(StorePutModel storePutModel)
        {
            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            var store = _applicationDB.Stores.Include(x => x.Seller)
                                             .AsNoTracking()
                                             .FirstOrDefault(x => x.Id == storePutModel.Id);
            if (store == null)
                return NotFound();

            if (!userRoles.Contains("admin") && store.Seller.Id != _user.Id)
                    return BadRequest();

            var mapperConfig = new MapperConfiguration(cgf => cgf.CreateMap<StorePutModel, Store>());
            var mapper = new Mapper(mapperConfig);

            store = mapper.Map<StorePutModel, Store>(storePutModel);

            _applicationDB.Stores.Update(store);
            _applicationDB.SaveChanges();

            return Ok(storePutModel);
        }

    }
}
