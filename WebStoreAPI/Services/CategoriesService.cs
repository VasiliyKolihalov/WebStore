using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class CategoriesService
    {
        private readonly IApplicationContext _applicationDb;

        public CategoriesService(IApplicationContext applicationContext)
        {
            _applicationDb = applicationContext;
        }

        public IEnumerable<CategoryViewModel> GetAll()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModels =
                mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(
                    _applicationDb.Categories.Include(x => x.Parent));
            return categoryViewModels;
        }

        public CategoryViewModel Get(int id)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == id);

            if (category == null)
                throw new NotFoundException("category not found");

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return categoryViewModel;
        }

        public IEnumerable<CategoryViewModel> GetChildren(int id)
        {
            if (!_applicationDb.Categories.Any(x => x.Id == id))
                throw new NotFoundException("category not found");

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categories = _applicationDb.Categories.Include(x => x.Parent).Where(x => x.Parent.Id == id);

            var categoryViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoryViewModels;
        }

        public CategoryViewModel Post(CatergoryAddModel categoryAddModel)
        {
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
                    throw new NotFoundException("category not found");

                category.Parent = categoryParent;
            }

            _applicationDb.Categories.Add(category);
            _applicationDb.SaveChanges();

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return categoryViewModel;
        }

        public CategoryViewModel Put(CategoryViewModel categoryViewModel)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent)
                .FirstOrDefault(x => x.Id == categoryViewModel.Id);

            if (category == null)
            {
                throw new NotFoundException("category not found");
            }

            category.Name = categoryViewModel.Name;

            if (categoryViewModel.ParentId != 0)
            {
                Category newParentCategory =
                    _applicationDb.Categories.FirstOrDefault(x => x.Id == categoryViewModel.ParentId);

                if (newParentCategory == null)
                    throw new NotFoundException("category not found");

                category.Parent = newParentCategory;
            }
            else
            {
                category.Parent = null;
            }

            _applicationDb.Categories.Update(category);
            _applicationDb.SaveChanges();

            return categoryViewModel;
        }
        
        public CategoryViewModel Delete(int id)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == id);

            if (category == null)
                throw new NotFoundException("category not found");

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

            return categoryViewModel;
        }
    }
}