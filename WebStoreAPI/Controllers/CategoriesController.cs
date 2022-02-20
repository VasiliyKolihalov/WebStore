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
        private readonly IApplicationContext _applicationDb;

        public CategoriesController(IApplicationContext applicationContext)
        {
            _applicationDb = applicationContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetAll()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModels =
                mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(
                    _applicationDb.Categories.Include(x => x.Parent));
            return categoryViewModels;
        }

        [HttpGet("{id}")]
        public ActionResult<CategoryViewModel> Get(int id)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == id);

            if (category == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return categoryViewModel;
        }

        [HttpGet]
        [Route("{id}/getChildren")]
        public ActionResult<IEnumerable<CategoryViewModel>> GetChildren(int id)
        {
            if (!_applicationDb.Categories.Any(x => x.Id == id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categories = _applicationDb.Categories.Include(x => x.Parent).Where(x => x.Parent.Id == id);

            var categoryViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoryViewModels;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpPost]
        public ActionResult<CategoryViewModel> Post(CatergoryAddModel categoryAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CatergoryAddModel, Category>();
                cfg.CreateMap<Category, CategoryViewModel>().ForMember(nameof(CategoryViewModel.ParentId),
                    opt => opt.MapFrom(x => x.Parent.Id));
            });

            var mapper = new Mapper(mapperConfig);

            var category = mapper.Map<CatergoryAddModel, Category>(categoryAddModel);

            if (categoryAddModel.ParentId != 0)
            {
                Category categoryParent =
                    _applicationDb.Categories.FirstOrDefault(x => x.Id == categoryAddModel.ParentId);

                if (categoryParent == null)
                    return NotFound();

                category.Parent = categoryParent;
            }

            _applicationDb.Categories.Add(category);
            _applicationDb.SaveChanges();

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpPut]
        public ActionResult<CategoryViewModel> Put(CategoryViewModel categoryViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = _applicationDb.Categories.Include(x => x.Parent)
                .FirstOrDefault(x => x.Id == categoryViewModel.Id);

            if (category == null)
            {
                return BadRequest(ModelState);
            }

            category.Name = categoryViewModel.Name;

            if (categoryViewModel.ParentId != 0)
            {
                Category newParentCategory =
                    _applicationDb.Categories.FirstOrDefault(x => x.Id == categoryViewModel.ParentId);

                if (newParentCategory == null)
                    return NotFound();

                category.Parent = newParentCategory;
            }
            else
            {
                category.Parent = null;
            }

            _applicationDb.Categories.Update(category);
            _applicationDb.SaveChanges();

            return Ok(categoryViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [HttpDelete("{id}")]
        public ActionResult<CategoryViewModel> Delete(int id)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == id);

            if (category == null)
                return NotFound();

            var childCategories = _applicationDb.Categories.Where(x => x.Parent.Id == id);

            foreach (Category child in childCategories)
            {
                child.Parent = null;
            }

            _applicationDb.Categories.Remove(category);
            _applicationDb.Categories.UpdateRange(childCategories);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }
    }
}