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
        public ActionResult<IEnumerable<ProductViewModel>> Get()
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
        [HttpPut]
        public ActionResult<ProductViewModel> Put(ProductViewModel productView)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!_applicationDB.Products.Any(x => x.Id == productView.Id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>cfg.CreateMap<ProductViewModel, Product>());
            var mapper = new Mapper(mapperConfig);

            var product = mapper.Map<ProductViewModel, Product>(productView);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();
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
        [Route("{productId}/getimages")]
        [HttpGet]
        public ActionResult<IEnumerable<Base64ImageViewModel>> GetImages(long productId)
        {
            var Images = _applicationDB.Images.Include(x => x.Products);

            var base64ImageViews = new List<Base64ImageViewModel>();
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                  .ForMember("ImageData", opt => opt.MapFrom(x => Convert.ToBase64String(x.ImageData))));

            var mapper = new Mapper(mapperConfig);

            foreach (var image in Images)
            {
                if (image.Products.FirstOrDefault(x => x.Id == productId) != null)
                {
                    base64ImageViews.Add(mapper.Map<Image, Base64ImageViewModel>(image));
                }
            }

            return base64ImageViews;
        }

        [Authorize(Roles = "admin")]
        [Route("{productId}/addimage/{imageId}")]
        [HttpPost]
        public ActionResult<Base64ImageViewModel> AddImage(long productId, long imageId)
        {
            var image = _applicationDB.Images.FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDB.Products.FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null)
                return NotFound();

            product.Images.Add(image);

            _applicationDB.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                   .ForMember("ImageData", opt => opt.MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageView);
        }

        [Authorize(Roles = "admin")]
        [Route("{productId}/deleteimage/{imageId}")]
        [HttpDelete]
        public ActionResult<Base64ImageViewModel> DeleteImage(long productId, long imageId)
        {
            var image = _applicationDB.Images.FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDB.Products.Include(x => x.Images).FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null || !product.Images.Contains(image))
                return NotFound();

            product.Images.Remove(image);
            _applicationDB.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                   .ForMember("ImageData", opt => opt.MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageView);
        }
    }

}
