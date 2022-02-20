using Microsoft.AspNetCore.Mvc;
using System;
using WebStoreAPI.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;
        private User _user;

        public ProductsController(IApplicationContext productsContext, UserManager<User> userManager)
        {
            _applicationDb = productsContext;
            _userManager = userManager;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetAll()
        {
            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productViewModels;
        }

        [HttpGet("{id}")]
        public ActionResult<ProductViewModel> Get(long id)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == id);

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

        [Route("getBasedPage")]
        [HttpGet]
        public ActionResult<PageViewModel> GetBasedPage(PageGetModel pageGetModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categories = _applicationDb.Categories
                .Include(x => x.Products)
                .Include(x => x.Parent)
                .Where(x => pageGetModel.CategoriesId.Contains(x.Id)).ToList();
            var stores = _applicationDb.Stores
                .Include(x => x.Products)
                .Include(x => x.Seller)
                .Where(x => pageGetModel.StoresId.Contains(x.Id)).ToList();

            Func<Product, double> ordenBy = null;
            switch (pageGetModel.OrderBy)
            {
                case PageOrder.Cost:
                    ordenBy = x => (double) x.Cost;
                    break;
                case PageOrder.Rating:
                    ordenBy = x => x.Reviews.Sum(x => x.Rating) / (x.Reviews.Count + 1);
                    break;
            }

            Func<Product, bool> selectBy = null;
            if (pageGetModel.CategoriesId.Any())
            {
                if (!categories.Any())
                    return NotFound("not found categories");
                selectBy += product => categories.Intersect(product.Categories).Any();
            }

            if (pageGetModel.StoresId.Any())
            {
                if (!stores.Any())
                    return NotFound("not found stores");
                selectBy += product => stores.Contains(product.Store);
            }

            IEnumerable<Product> products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews);
            if (selectBy != null)
                products = products.Where(selectBy);
            if (ordenBy != null)
                products = pageGetModel.InAscending ? products.OrderBy(ordenBy) : products.OrderByDescending(ordenBy);

            int count = products.Count();
            var pageProducts = products.Skip((pageGetModel.PageNumber - 1) * pageGetModel.PageSize)
                .Take(pageGetModel.PageSize).ToList();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();

                cfg.CreateMap<Category, CategoryViewModel>()
                    .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id));
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModel = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(pageProducts);

            PageData pageData = new PageData(count, pageGetModel.PageNumber, pageGetModel.PageSize)
            {
                InAscending = pageGetModel.InAscending,
                OrderBy = pageGetModel.OrderBy,
                Categories = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories),
                Stores = mapper.Map<IEnumerable<Store>, List<StorePutModel>>(stores)
            };
            PageViewModel pageViewModel = new PageViewModel()
            {
                Products = productsViewModel,
                PageData = pageData
            };
            return pageViewModel;
        }

        [Route("getBasedKeyword/{keyWord}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedKeyword(string keyWord)
        {
            var loverKeyWord = keyWord.ToLower();
            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Name.ToLower().Contains(loverKeyWord) ||
                            x.Description.ToLower().Contains(loverKeyWord));
            if (!products.Any())
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            return productViewModels;
        }

        [Route("getBasedStore/{storeId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedStore(int storeId)
        {
            var store = _applicationDb.Stores.FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                return NotFound();

            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Store.Id == store.Id);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            return productViewModels;
        }

        [Route("getBasedCategory/{categoryId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedCategory(int categoryId)
        {
            var category = _applicationDb.Categories.FirstOrDefault(x => x.Id == categoryId);

            if (category == null)
                return NotFound();

            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Categories.FirstOrDefault(x => x.Id == categoryId) != null);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productsViewModels;
        }

        [Route("getBasedTag/{tagId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedTag(int tagId)
        {
            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                return NotFound();

            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Tags.FirstOrDefault(x => x.Id == tagId) != null);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);

            return productsViewModels;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpPost]
        public ActionResult<ProductViewModel> Post(ProductAddModel productAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            SetUser();

            var store = _applicationDb.Stores.Include(x => x.Seller).FirstOrDefault(x => x.Seller.Id == _user.Id);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductAddModel, Product>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });

            var mapper = new Mapper(mapperConfig);

            var product = mapper.Map<ProductAddModel, Product>(productAddModel);

            product.Store = store;
            _applicationDb.Products.Add(product);
            _applicationDb.SaveChanges();

            var productViewModel = mapper.Map<Product, ProductViewModel>(product);

            return Ok(productViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpDelete("{id}")]
        public ActionResult<ProductViewModel> Delete(long id)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
                return NotFound();

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(ApplicationConstants.AdminRoleName) && product.Store.Seller.Id != _user.Id)
                return BadRequest();

            _applicationDb.Products.Remove(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModel = mapper.Map<Product, ProductViewModel>(product);

            return Ok(productViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpPut]
        public ActionResult<ProductViewModel> Put(ProductPutModel productPutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == productPutModel.Id);

            if (product == null)
                return NotFound();

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(ApplicationConstants.AdminRoleName) && product.Store.Seller.Id != _user.Id)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductPutModel, Product>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            product = mapper.Map<ProductPutModel, Product>(productPutModel);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var productViewModel = mapper.Map<Product, ProductViewModel>(product);
            return Ok(productViewModel);
        }

        #region product rating

        [Route("{productId}/getReviews")]
        [HttpGet]
        public ActionResult<IEnumerable<ReviewViewModel>> GetReviews(long productId)
        {
            var reviews = _applicationDb.Reviews.Include(x => x.User).Where(x => x.Product.Id == productId);

            if (!reviews.Any())
            {
                return NotFound();
            }

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Review, ReviewViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var reviewViewModels = mapper.Map<IEnumerable<Review>, List<ReviewViewModel>>(reviews);
            return reviewViewModels;
        }

        [Authorize]
        [Route("rate")]
        [HttpPut]
        public ActionResult<ReviewViewModel> RateProduct(ReviewAddModel reviewAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            SetUser();
            if (!_user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "email not confirmed");
                return BadRequest(ModelState);
            }

            var product = _applicationDb.Products.Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == reviewAddModel.ProductId);

            if (product == null)
                return NotFound();

            if (_applicationDb.Reviews.Any(x => x.Product.Id == product.Id && x.User.Id == _user.Id))
                return BadRequest();

            if (reviewAddModel.Rating < ApplicationConstants.MinRating ||
                reviewAddModel.Rating > ApplicationConstants.MaxRating)
            {
                ModelState.AddModelError(string.Empty, "invalid rating");
                return BadRequest(ModelState);
            }

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReviewAddModel, Review>();

                cfg.CreateMap<Review, ReviewViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var review = mapper.Map<ReviewAddModel, Review>(reviewAddModel);
            review.User = _user;
            product.Reviews.Add(review);
            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var reviewViewModel = mapper.Map<Review, ReviewViewModel>(review);

            return Ok(reviewViewModel);
        }

        #endregion


        #region product categories

        [Route("{productId}/getCategories")]
        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetCategories(long productId)
        {
            var product = _applicationDb.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (product == null)
                return NotFound();

            var categories = _applicationDb.Categories.Include(x => x.Parent)
                .Include(x => x.Products)
                .Where(x => x.Products.FirstOrDefault(x => x.Id == productId) != null);

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoryViewModels;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/addСategory/{categoryId}")]
        [HttpPut]
        public ActionResult<CategoryViewModel> AddCategory(int productId, int categoryId)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDb.Products.Include(x => x.Categories).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (category == null || product == null)
                return NotFound();

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(ApplicationConstants.AdminRoleName) && product.Store.Seller.Id != _user.Id)
                return BadRequest();

            if (product.Categories.Contains(category))
                return BadRequest();

            product.Categories.Add(category);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt =>
                    opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/removeСategory/{categoryId}")]
        [HttpPut]
        public ActionResult<CategoryViewModel> RemoveCategory(int productId, int categoryId)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDb.Products.Include(x => x.Categories).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (category == null || product == null || !product.Categories.Contains(category))
                return NotFound();

            SetUser();
            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(ApplicationConstants.AdminRoleName) && product.Store.Seller.Id != _user.Id)
                return BadRequest();

            product.Categories.Remove(category);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return Ok(categoryViewModel);
        }

        #endregion

        #region product images

        [Route("{productId}/getImages")]
        [HttpGet]
        public ActionResult<IEnumerable<Base64ImagePutModel>> GetImages(long productId)
        {
            SetUser();
            var product = _applicationDb.Products.Include(x => x.Images).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);


            if (product == null)
                return NotFound();

            var images = product.Images;

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImagePutModel>().ForMember(nameof(Base64ImagePutModel.ImageData), opt => opt
                    .MapFrom(x => Convert.ToBase64String(x.ImageData)));
            });
            var mapper = new Mapper(mapperConfig);

            List<Base64ImagePutModel> base64ImageViewModels =
                mapper.Map<IEnumerable<Image>, List<Base64ImagePutModel>>(images);
            return base64ImageViewModels;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/addImage/{imageId}")]
        [HttpPut]
        public ActionResult<Base64ImagePutModel> AddImage(long productId, long imageId)
        {
            SetUser();

            var image = _applicationDb.Images.Include(x => x.User).Where(x => x.User.Id == _user.Id)
                .FirstOrDefault(x => x.Id == imageId);

            var product = _applicationDb.Products.Include(x => x.Images).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null)
                return NotFound();

            if (product.Images.Contains(image))
                return BadRequest();

            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(ApplicationConstants.AdminRoleName) && product.Store.Seller.Id != _user.Id)
                return BadRequest();

            product.Images.Add(image);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImagePutModel>().ForMember(nameof(Base64ImagePutModel.ImageData), opt => opt
                    .MapFrom(x => Convert.ToBase64String(x.ImageData)));
            });
            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModel = mapper.Map<Image, Base64ImagePutModel>(image);

            return Ok(base64ImageViewModel);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/removeImage/{imageId}")]
        [HttpPut]
        public ActionResult<Base64ImagePutModel> RemoveImage(long productId, long imageId)
        {
            SetUser();
            var image = _applicationDb.Images.Include(x => x.User).Where(x => x.User.Id == _user.Id)
                .FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDb.Products.Include(x => x.Images).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (image == null || product == null || !product.Images.Contains(image))
                return NotFound();

            IList<string> userRoles = _userManager.GetRolesAsync(_user).Result;

            if (!userRoles.Contains(ApplicationConstants.AdminRoleName) && product.Store.Seller.Id != _user.Id)
                return BadRequest();

            product.Images.Remove(image);
            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

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
            var product = _applicationDb.Products.Include(x => x.Tags)
                .FirstOrDefault(x => x.Id == productId);

            if (product == null)
                return NotFound();

            var tags = product.Tags;

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<IEnumerable<Tag>, List<TagViewModel>>(tags);

            return tagViewModels;
        }


        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [Route("{productId}/addTag/{tagId}")]
        [HttpPut]
        public ActionResult<TagViewModel> AddTag(long productId, int tagId)
        {
            var product = _applicationDb.Products.Include(x => x.Tags).FirstOrDefault(x => x.Id == productId);

            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (product == null || tag == null)
                return NotFound();

            if (product.Tags.Contains(tag))
                return BadRequest();

            product.Tags.Add(tag);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<Tag, TagViewModel>(tag);

            return Ok(tagViewModels);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [Route("{productId}/removeTag/{tagId}")]
        [HttpPut]
        public ActionResult<TagViewModel> RemoveTag(long productId, int tagId)
        {
            var product = _applicationDb.Products.Include(x => x.Tags).FirstOrDefault(x => x.Id == productId);

            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (product == null || tag == null)
                return NotFound();

            if (!product.Tags.Contains(tag))
                return BadRequest();

            product.Tags.Remove(tag);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<Tag, TagViewModel>(tag);

            return Ok(tagViewModels);
        }

        #endregion
    }
}