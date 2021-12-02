using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using WebStoreAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;

        public ProductsController(ApplicationContext productsContext)
        {
            _applicationDB = productsContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetAll()
        {
            var products = _applicationDB.Products;

            var productViews = new List<ProductViewModel>();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductViewModel>());
            var mapper = new Mapper(mapperConfig);

            productViews = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productViews;

        }

        [HttpGet("{id}")]
        public ActionResult<ProductViewModel> Get(long id)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductViewModel>());
            var mapper = new Mapper(mapperConfig);
            var productView = mapper.Map<Product, ProductViewModel>(product);

            return productView;

        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public ActionResult<ProductViewModel> Post(ProductAddModel productAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductAddModel, Product>();
                cfg.CreateMap<Product, ProductViewModel>();
            });

            var mapper = new Mapper(mapperConfig);

            var product = mapper.Map<ProductAddModel, Product>(productAddModel);

            _applicationDB.Products.Add(product);
            _applicationDB.SaveChanges();

            var productView = mapper.Map<Product, ProductViewModel>(product);

            return Ok(productView);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public ActionResult<ProductViewModel> Delete(long id)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            _applicationDB.Products.Remove(product);
            _applicationDB.SaveChanges();


            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductViewModel>());
            var mapper = new Mapper(mapperConfig);

            var productView = mapper.Map<Product, ProductViewModel>(product);

            return Ok(productView);
        }

        [Authorize(Roles = "admin")]
        [HttpPut]
        public ActionResult<ProductViewModel> Put(ProductViewModel productView)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!_applicationDB.Products.Any(x => x.Id == productView.Id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<ProductViewModel, Product>());
            var mapper = new Mapper(mapperConfig);

            var product = mapper.Map<ProductViewModel, Product>(productView);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            return Ok(productView);
        }

        #region product categories

        [Route("getbasedcategory/{categoryId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedCategory(int categoryId)
        {
            var category = _applicationDB.Categories.FirstOrDefault(x => x.Id == categoryId);

            if (category == null)
                return NotFound();

            var products = _applicationDB.Products.Where(x => x.Categories.FirstOrDefault(x => x.Id == categoryId) != null);

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductViewModel>());
            var mapper = new Mapper(mapperConfig);

            var productsViews = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productsViews;

        }

        [Route("{productId}/getcategories")]
        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetCategories(long productId)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == productId);

            if (product == null)
                return NotFound();

            var categories = _applicationDB.Categories.Include(x => x.Parent)
                                                      .Include(x => x.Products)
                                                      .Where(x => x.Products.FirstOrDefault(x => x.Id == productId) != null);

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                                 .ForMember("ParentId", opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViews = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoryViews;
        }

        [Authorize(Roles = "admin")]
        [Route("{productId}/addcategory/{categoryId}")]
        [HttpPost]
        public ActionResult<CategoryViewModel> AddCategory(int productId, int categoryId)
        {
            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDB.Products.Include(x => x.Categories).FirstOrDefault(x => x.Id == productId);

            if (category == null || product == null)
                return NotFound();

            if (product.Categories.Contains(category))
                return BadRequest();

            product.Categories.Add(category);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                                 .ForMember("ParentId", opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryView = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryView);
        }

        [Authorize(Roles = "admin")]
        [Route("{productId}/removecategory/{categoryId}")]
        [HttpDelete]
        public ActionResult<CategoryViewModel> RemoveCategory(int productId, int categoryId)
        {
            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDB.Products.Include(x => x.Categories).FirstOrDefault(x => x.Id == productId);
            if (category == null || product == null || !product.Categories.Contains(category))
                return NotFound();

            product.Categories.Remove(category);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                                 .ForMember("ParentId", opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryView = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryView);
        }

        #endregion 

        #region product images

        [Route("{productId}/getimages")]
        [HttpGet]
        public ActionResult<IEnumerable<Base64ImageViewModel>> GetImages(long productId)
        {
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == productId);

            if (product == null)
                return NotFound();

            var images = _applicationDB.Images.Where(x => x.Products.FirstOrDefault(x => x.Id == productId) != null);
            var base64ImageViews = new List<Base64ImageViewModel>();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                   .ForMember("ImageData", opt => opt.MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            base64ImageViews = mapper.Map<IEnumerable<Image>, List<Base64ImageViewModel>>(images);
            return base64ImageViews;
        }

        [Authorize(Roles = "admin")]
        [Route("{productId}/addimage/{imageId}")]
        [HttpPost]
        public ActionResult<Base64ImageViewModel> AddImage(long productId, long imageId)
        {
            var image = _applicationDB.Images.FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDB.Products.Include(x => x.Images).FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null)
                return NotFound();

            if (product.Images.Contains(image))
                return BadRequest();

            product.Images.Add(image);

            _applicationDB.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                                                 .ForMember("ImageData", opt => opt
                                                                 .MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageView);
        }

        [Authorize(Roles = "admin")]
        [Route("{productId}/removeimage/{imageId}")]
        [HttpDelete]
        public ActionResult<Base64ImageViewModel> RemoveImage(long productId, long imageId)
        {
            var image = _applicationDB.Images.FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDB.Products.Include(x => x.Images).FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null || !product.Images.Contains(image))
                return NotFound();

            product.Images.Remove(image);
            _applicationDB.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                                                 .ForMember("ImageData", opt => opt
                                                                 .MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageView);
        }
        #endregion

    }

}
