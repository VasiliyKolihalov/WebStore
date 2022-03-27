using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoriesService _categoriesService;

        public CategoriesController(CategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetAll()
        {
            List<CategoryViewModel> categoryViews = _categoriesService.GetAll() as List<CategoryViewModel>;
            return Ok(categoryViews);
        }

        [HttpGet("{id}")]
        public ActionResult<CategoryViewModel> Get(int id)
        {
            CategoryViewModel categoryView = _categoriesService.Get(id);
            return Ok(categoryView);
        }

        [HttpGet]
        [Route("{id}/getChildren")]
        public ActionResult<IEnumerable<CategoryViewModel>> GetChildren(int id)
        {
            List<CategoryViewModel> categoryViews = _categoriesService.GetChildren(id) as List<CategoryViewModel>;
            return Ok(categoryViews);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpPost]
        public ActionResult<CategoryViewModel> Post(CatergoryAddModel categoryAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            CategoryViewModel categoryView = _categoriesService.Post(categoryAddModel);
            return Ok(categoryView);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpPut]
        public ActionResult<CategoryViewModel> Put(CategoryViewModel categoryPutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            CategoryViewModel categoryView = _categoriesService.Put(categoryPutModel);
            return Ok(categoryView);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpDelete("{id}")]
        public ActionResult<CategoryViewModel> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            CategoryViewModel categoryView = _categoriesService.Delete(id);
            return Ok(categoryView);
        }
    }
}