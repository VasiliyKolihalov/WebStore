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

namespace WebStoreAPI.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;

        public CategoriesController(ApplicationContext applicationContext)
        {
            _applicationDB = applicationContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetAll()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                            .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoroyViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(_applicationDB.Categories.Include(x => x.Parent));
            return categoroyViewModels;
        } 

        [HttpGet("{id}")]
        public ActionResult<CategoryViewModel> Get(int id)
        {
            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == id);

            if (category == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                            .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var сategoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return сategoryViewModel;
        }

        [HttpGet]
        [Route("{id}/getchilds")]
        public ActionResult<IEnumerable<CategoryViewModel>> GetChilds(int id)
        {
            if (!_applicationDB.Categories.Any(x => x.Id == id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                           .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categories = _applicationDB.Categories.Include(x => x.Parent).Where(x => x.Parent.Id == id);

            var categoroyViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoroyViewModels;
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpPost]
        public ActionResult<CategoryViewModel> Post(CatergoryAddModel catergoryAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CatergoryAddModel, Category>();
                cfg.CreateMap<Category, CategoryViewModel>().ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id));
            });

            var mapper = new Mapper(mapperConfig);

            var category = mapper.Map<CatergoryAddModel, Category>(catergoryAddModel);

            if(catergoryAddModel.ParentId != 0)
            {
                var categoryParrent = _applicationDB.Categories.FirstOrDefault(x => x.Id == catergoryAddModel.ParentId);

                if (categoryParrent == null)
                    return NotFound();

                category.Parent = categoryParrent;
            }

            _applicationDB.Categories.Add(category);
            _applicationDB.SaveChanges();

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);

        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpPut]
        public ActionResult<CategoryViewModel> Put(CategoryViewModel categoryViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryViewModel.Id);

            category.Name = categoryViewModel.Name;

            if (categoryViewModel.ParentId != 0)
            {
                var newParentCategory = _applicationDB.Categories.FirstOrDefault(x => x.Id == categoryViewModel.ParentId);

                if (newParentCategory == null)
                    return NotFound();

                category.Parent = newParentCategory;
            }
            else
            {
                category.Parent = null;
            }

            _applicationDB.Update(category);
            _applicationDB.SaveChanges();

            return Ok(categoryViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [HttpDelete("{id}")]
        public ActionResult<CategoryViewModel> Delete(int id)
        {
            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == id);

            if (category == null)
                return NotFound();

            var childCategories = _applicationDB.Categories.Where(x => x.Parent.Id == id);

            foreach(var child in childCategories)
            {
                child.Parent = null;
            }

            _applicationDB.Categories.Remove(category);
            _applicationDB.Categories.UpdateRange(childCategories);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                          .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }

    }
}
