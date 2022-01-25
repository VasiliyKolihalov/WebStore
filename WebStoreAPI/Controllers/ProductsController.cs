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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;
        private readonly UserManager<User> _userManager;
        private User _user;

        public ProductsController(ApplicationContext productsContext, UserManager<User> userManager)
        {
            _applicationDB = productsContext;
            _userManager = userManager;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetAll()
        {
            var products = _applicationDB.Products.Include(x => x.Store).ToListAsync().Result;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            List<ProductViewModel> productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productViewModels;

        }

        [HttpGet("{id}")]
        public ActionResult<ProductViewModel> Get(long id)
        {
            var product = _applicationDB.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);
            var productViewModel = mapper.Map<Product, ProductViewModel>(product);

            return productViewModel;

        }

        [Route("getbasedstore/{storeId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedStore(int storeId)
        {
            var store = _applicationDB.Stores.FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                return NotFound();

            var products = _applicationDB.Products.Include(x => x.Store).Where(x => x.Store.Id == store.Id);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModel = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            return productViewModel;
        }

        [Route("getbasedcategory/{categoryId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedCategory(int categoryId)
        {
            var category = _applicationDB.Categories.FirstOrDefault(x => x.Id == categoryId);

            if (category == null)
                return NotFound();

            var products = _applicationDB.Products.Include(x => x.Store).Where(x => x.Categories.FirstOrDefault(x => x.Id == categoryId) != null);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productsViewModels;

        }

        [Route("getbasedtag/{tagId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedTag(int tagId)
        {
            var tag = _applicationDB.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                return NotFound();

            var products = _applicationDB.Products.Include(x => x.Store).Where(x => x.Tags.FirstOrDefault(x => x.Id == tagId) != null);


            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productsViewModels;
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpPost]
        public ActionResult<ProductViewModel> Post(ProductAddModel productAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            SetUser();

            var store = _applicationDB.Stores.Include(x => x.Seller).FirstOrDefault(x => x.Seller.Id == _user.Id);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductAddModel, Product>();
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });

            var mapper = new Mapper(mapperConfig);

            var product = mapper.Map<ProductAddModel, Product>(productAddModel);

            product.Store = store;
            _applicationDB.Products.Add(product);
            _applicationDB.SaveChanges();

            var productViewModel = mapper.Map<Product, ProductViewModel>(product);

            return Ok(productViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpDelete("{id}")]
        public ActionResult<ProductViewModel> Delete(long id)
        {      
            var product = _applicationDB.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == id);
          
            if (product == null)
                return NotFound();

            SetUser();           
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(RolesConstants.AdminRoleName))
                if (product.Store.Seller.Id != _user.Id)
                    return BadRequest();
            
            _applicationDB.Products.Remove(product);
            _applicationDB.SaveChanges();


            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModel = mapper.Map<Product, ProductViewModel>(product);

            return Ok(productViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [HttpPut]
        public ActionResult<ProductViewModel> Put(ProductPutModel productPutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var product = _applicationDB.Products.Include(x => x.Store)
                                                 .AsNoTracking()
                                                 .FirstOrDefault(x => x.Id == productPutModel.Id);

            if (product == null)
                return NotFound();

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(RolesConstants.AdminRoleName))
                if (product.Store.Seller.Id != _user.Id)
                    return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<ProductPutModel, Product>());
            var mapper = new Mapper(mapperConfig);

            product = mapper.Map<ProductPutModel, Product>(productPutModel);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            return Ok(productPutModel);
        }

        #region product categories

        [Route("{productId}/getcategories")]
        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetCategories(long productId)
        {
            var product = _applicationDB.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (product == null)
                return NotFound();

            var categories = _applicationDB.Categories.Include(x => x.Parent)
                                                      .Include(x => x.Products)
                                                      .Where(x => x.Products.FirstOrDefault(x => x.Id == productId) != null);

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                                 .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoryViewModels;
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [Route("{productId}/addcategory/{categoryId}")]
        [HttpPost]
        public ActionResult<CategoryViewModel> AddCategory(int productId, int categoryId)
        {
            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDB.Products.Include(x => x.Categories).Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (category == null || product == null)
                return NotFound();

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(RolesConstants.AdminRoleName))
                if (product.Store.Seller.Id != _user.Id)
                    return BadRequest();


            if (product.Categories.Contains(category))
                return BadRequest();

            product.Categories.Add(category);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                                 .ForMember(nameof(CategoryViewModel.ParentId), opt => 
                                                                 opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [Route("{productId}/removecategory/{categoryId}")]
        [HttpDelete]
        public ActionResult<CategoryViewModel> RemoveCategory(int productId, int categoryId)
        {
            var category = _applicationDB.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDB.Products.Include(x => x.Categories).Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (category == null || product == null || !product.Categories.Contains(category))
                return NotFound();

            SetUser();            
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(RolesConstants.AdminRoleName))
                if (product.Store.Seller.Id != _user.Id)
                    return BadRequest();

            product.Categories.Remove(category);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                                                                 .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }

        #endregion 

        #region product images

        [Route("{productId}/getimages")]
        [HttpGet]
        public ActionResult<IEnumerable<Base64ImagePutModel>> GetImages(long productId)
        {
            SetUser();
            var product = _applicationDB.Products.Include(x => x.Images).Include(x => x.Store).FirstOrDefault(x => x.Id == productId);
            

            if (product == null)
                return NotFound();

            var images = product.Images;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImagePutModel>().ForMember(nameof(Base64ImagePutModel.ImageData), opt => opt
                                                            .MapFrom(x => Convert.ToBase64String(x.ImageData)));
            });
            var mapper = new Mapper(mapperConfig);

            List<Base64ImagePutModel> base64ImageViewModels = mapper.Map<IEnumerable<Image>, List<Base64ImagePutModel>>(images);
            return base64ImageViewModels;
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [Route("{productId}/addimage/{imageId}")]
        [HttpPost]
        public ActionResult<Base64ImagePutModel> AddImage(long productId, long imageId)
        {
            var image = _applicationDB.Images.Include(x => x.User).FirstOrDefault(x => x.Id == imageId);

            var product = _applicationDB.Products.Include(x => x.Images).Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null)
                return NotFound();

            if (product.Images.Contains(image))
                return BadRequest();

            SetUser();           
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(RolesConstants.AdminRoleName))
                if (product.Store.Seller.Id != _user.Id || image.User.Id != _user.Id)
                    return BadRequest();

            product.Images.Add(image);

            _applicationDB.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImagePutModel>().ForMember(nameof(Base64ImagePutModel.ImageData), opt => opt
                                                           .MapFrom(x => Convert.ToBase64String(x.ImageData)));

            });
            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModel = mapper.Map<Image, Base64ImagePutModel>(image);

            return Ok(base64ImageViewModel);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
        [Route("{productId}/removeimage/{imageId}")]
        [HttpDelete]
        public ActionResult<Base64ImagePutModel> RemoveImage(long productId, long imageId)
        {
            var image = _applicationDB.Images.Include(x => x.User).FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDB.Products.Include(x => x.Images).Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null || !product.Images.Contains(image))
                return NotFound();


            SetUser();           
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(RolesConstants.AdminRoleName))
                if (product.Store.Seller.Id != _user.Id)
                    return BadRequest();

            product.Images.Remove(image);
            _applicationDB.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImagePutModel>().ForMember(nameof(Base64ImagePutModel.ImageData), opt => opt
                                                           .MapFrom(x => Convert.ToBase64String(x.ImageData)));            

            });
            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModel = mapper.Map<Image, Base64ImagePutModel>(image);

            return Ok(base64ImageViewModel);
        }
        #endregion

        #region product tags

        [Route("{productId}/gettags")]
        [HttpGet]
        public ActionResult<IEnumerable<TagViewModel>> GetTags(long productId)
        {
            var product = _applicationDB.Products.Include(x => x.Tags).Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (product == null)
                return NotFound();

            var tags = product.Tags;

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<IEnumerable<Tag>, List<TagViewModel>>(tags);

            return tagViewModels;
        }


        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [Route("{productId}/addtag/{tagId}")]
        [HttpPost]
        public ActionResult<TagViewModel> AddTag(long productId, int tagId)
        {
            var product = _applicationDB.Products.Include(x => x.Tags).FirstOrDefault(x => x.Id == productId);

            var tag = _applicationDB.Tags.FirstOrDefault(x => x.Id == tagId);

            if (product == null || tag == null)
                return NotFound();

            if (product.Tags.Contains(tag))
                return BadRequest();

            product.Tags.Add(tag);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<Tag,TagViewModel>(tag);

            return Ok(tagViewModels);
        }

        [Authorize(Roles = RolesConstants.AdminRoleName)]
        [Route("{productId}/removetag/{tagId}")]
        [HttpDelete]
        public ActionResult<TagViewModel> RemoveTag(long productId, int tagId)
        {
            var product = _applicationDB.Products.Include(x => x.Tags).FirstOrDefault(x => x.Id == productId);

            var tag = _applicationDB.Tags.FirstOrDefault(x => x.Id == tagId);

            if (product == null || tag == null)
                return NotFound();

            if (!product.Tags.Contains(tag))
                return BadRequest();

            product.Tags.Remove(tag);

            _applicationDB.Products.Update(product);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<Tag, TagViewModel>(tag);

            return Ok(tagViewModels);
        }

        #endregion

    }

}
