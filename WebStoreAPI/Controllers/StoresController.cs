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
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly StoreService _storeService;
        private readonly UserManager<User> _userManager;
       
        public StoresController(StoreService storeService, UserManager<User> userManager)
        {
            _storeService = storeService;
            _userManager = userManager;
        }

        private User GetUser()
        {
            return _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StoreViewModel>> GetAll()
        {
            _storeService.User = GetUser();
            List<StoreViewModel> storeViews = _storeService.GetAll() as List<StoreViewModel>;

            return Ok(storeViews);
        }

        [HttpGet("{storeId}")]
        public ActionResult<StoreViewModel> Get(int storeId)
        {
            _storeService.User = GetUser();
            StoreViewModel storeView = _storeService.Get(storeId);

            return Ok(storeView);
        }


        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpDelete("{storeId}")]
        public ActionResult<StoreViewModel> Delete(int storeId)
        {
            _storeService.User = GetUser();
            StoreViewModel storeView = _storeService.Delete(storeId);

            return Ok(storeView);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpPut]
        public ActionResult<StoreViewModel> Put(StorePutModel storePutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _storeService.User = GetUser();
            StoreViewModel storeView = _storeService.Put(storePutModel);

            return Ok(storeView);
        }
    }
}