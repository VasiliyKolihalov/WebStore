using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class StoreService
    {
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;

        public StoreService(IApplicationContext applicationContext, UserManager<User> userManager)
        {
            _applicationDb = applicationContext;
            _userManager = userManager;
        }

        public User User { private get; set; }
        
        public IEnumerable<StoreViewModel> GetAll()
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
        
        public StoreViewModel Get(int storeId)
        {
            var store = _applicationDb.Stores.FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                throw new NotFoundException("store not found");

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
        
        public StoreViewModel Delete(int storeId)
        {
            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            var store = _applicationDb.Stores.Include(x => x.Seller).FirstOrDefault(x => x.Id == storeId);

            if (store == null || !userRoles.Contains("admin") && store.Seller.Id != User.Id)
                throw new NotFoundException("store not found");

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

            return storeViewModel;
        }
        
        public StoreViewModel Put(StorePutModel storePutModel)
        {
            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            var store = _applicationDb.Stores.Include(x => x.Seller)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == storePutModel.Id);

            if (store == null || !userRoles.Contains("admin") && store.Seller.Id != User.Id)
                throw new NotFoundException("store not found");
          
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StorePutModel, Store>();
                cfg.CreateMap<Store, StoreViewModel>();
            });
            var mapper = new Mapper(mapperConfig);

            store = mapper.Map<StorePutModel, Store>(storePutModel);

            _applicationDb.Stores.Update(store);
            _applicationDb.SaveChanges();

            var storeViewModel = mapper.Map<Store, StoreViewModel>(store);
            
            return storeViewModel;
        }
    }
}