using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class ProductsService
    {
        private readonly IApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;
        private readonly ICurrencyService _currencyService;

        public ProductsService(IApplicationContext productsContext, UserManager<User> userManager,
            ICurrencyService currencyService)
        {
            _applicationDb = productsContext;
            _userManager = userManager;
            _currencyService = currencyService;
        }

        public User User { private get; set; }

        private void SetRegionalCost(ProductViewModel products)
        {
            products.Сurrency = User.RegionalCurrency;
            products.Cost = _currencyService.ConvertCurrency(products.Cost, User.RegionalCurrency);
        }

        private void SetRegionalCost(IEnumerable<ProductViewModel> products)
        {
            foreach (var product in products)
            {
                SetRegionalCost(product);
            }
        }
        
        public IEnumerable<ProductViewModel> GetAll()
        {
            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            SetRegionalCost(productViewModels);

            return productViewModels;
        }

        public ProductViewModel Get(long id)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == id);

            if (product == null)
                throw new NotFoundException("product not found");

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);
            var productViewModel = mapper.Map<Product, ProductViewModel>(product);
            SetRegionalCost(productViewModel);

            return productViewModel;
        }

        public PageViewModel GetBasedPage(PageGetModel pageGetModel)
        {
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
                    throw new NotFoundException("not found categories");
                selectBy += product => categories.Intersect(product.Categories).Any();
            }

            if (pageGetModel.StoresId.Any())
            {
                if (!stores.Any())
                    throw new NotFoundException("not found stores");
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
            SetRegionalCost(productsViewModel);

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


        public IEnumerable<ProductViewModel> GetBasedKeyword(string keyWord)
        {
            var loverKeyWord = keyWord.ToLower();
            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Name.ToLower().Contains(loverKeyWord) ||
                            x.Description.ToLower().Contains(loverKeyWord));
            if (!products.Any())
                throw new NotFoundException("products not found");

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            SetRegionalCost(productViewModels);

            return productViewModels;
        }

        public IEnumerable<ProductViewModel> GetBasedStore(int storeId)
        {
            var store = _applicationDb.Stores.FirstOrDefault(x => x.Id == storeId);

            if (store == null)
                throw new NotFoundException("shop not found");

            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Store.Id == store.Id);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            SetRegionalCost(productViewModels);

            return productViewModels;
        }

        public IEnumerable<ProductViewModel> GetBasedCategory(int categoryId)
        {
            var category = _applicationDb.Categories.FirstOrDefault(x => x.Id == categoryId);

            if (category == null)
                throw new NotFoundException("category not found");

            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Categories.FirstOrDefault(x => x.Id == categoryId) != null);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            SetRegionalCost(productsViewModels);

            return productsViewModels;
        }

        public IEnumerable<ProductViewModel> GetBasedTag(int tagId)
        {
            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                throw new NotFoundException("tag not found");

            var products = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .Where(x => x.Tags.FirstOrDefault(x => x.Id == tagId) != null);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsViewModels = mapper.Map<IEnumerable<Product>, List<ProductViewModel>>(products);
            SetRegionalCost(productsViewModels);

            return productsViewModels;
        }

        public ProductViewModel Post(ProductAddModel productAddModel)
        {
            var store = _applicationDb.Stores.Include(x => x.Seller).FirstOrDefault(x => x.Seller.Id == User.Id);

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

            return productViewModel;
        }


        public ProductViewModel Delete(long id)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == id);

            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            if (product == null || !userRoles.Contains(RolesConstants.AdminRoleName) &&
                product.Store.Seller.Id != User.Id)
                throw new NotFoundException("product not found");

            _applicationDb.Products.Remove(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productViewModel = mapper.Map<Product, ProductViewModel>(product);

            return productViewModel;
        }

        public ProductViewModel Put(ProductPutModel productPutModel)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == productPutModel.Id);

            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            if (product == null || !userRoles.Contains(RolesConstants.AdminRoleName) &&
                product.Store.Seller.Id != User.Id)
                throw new NotFoundException("product not found");

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
            return productViewModel;
        }

        #region product rating

        public IEnumerable<ReviewViewModel> GetReviews(long productId)
        {
            var reviews = _applicationDb.Reviews.Include(x => x.User).Where(x => x.Product.Id == productId);

            if (!reviews.Any())
            {
                throw new NotFoundException("no reviews found");
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

        public ReviewViewModel RateProduct(ReviewAddModel reviewAddModel)
        {
            var product = _applicationDb.Products.Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == reviewAddModel.ProductId);

            if (product == null)
                throw new NotFoundException("product not found");

            if (_applicationDb.Reviews.Any(x => x.Product.Id == product.Id && x.User.Id == User.Id))
                throw new BadRequestException("the product has already been rated");

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReviewAddModel, Review>();

                cfg.CreateMap<Review, ReviewViewModel>();
                cfg.CreateMap<User, UserViewModel>()
                    .ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var review = mapper.Map<ReviewAddModel, Review>(reviewAddModel);
            review.User = User;
            product.Reviews.Add(review);
            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var reviewViewModel = mapper.Map<Review, ReviewViewModel>(review);

            return reviewViewModel;
        }

        #endregion

        #region product categories

        public IEnumerable<CategoryViewModel> GetCategories(long productId)
        {
            var product = _applicationDb.Products.Include(x => x.Store).FirstOrDefault(x => x.Id == productId);

            if (product == null)
                throw new NotFoundException("product not found");

            var categories = _applicationDb.Categories.Include(x => x.Parent)
                .Include(x => x.Products)
                .Where(x => x.Products.FirstOrDefault(x => x.Id == productId) != null);

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModels = mapper.Map<IEnumerable<Category>, List<CategoryViewModel>>(categories);

            return categoryViewModels;
        }

        public CategoryViewModel AddCategory(int productId, int categoryId)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDb.Products.Include(x => x.Categories).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (category == null)
                throw new NotFoundException("category not found");

            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            if (product == null || !userRoles.Contains(RolesConstants.AdminRoleName) &&
                product.Store.Seller.Id != User.Id)
                throw new NotFoundException("product not found");

            if (product.Categories.Contains(category))
                throw new BadRequestException("the product already has this category");

            product.Categories.Add(category);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt =>
                    opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return categoryViewModel;
        }

        public CategoryViewModel RemoveCategory(int productId, int categoryId)
        {
            var category = _applicationDb.Categories.Include(x => x.Parent).FirstOrDefault(x => x.Id == categoryId);
            var product = _applicationDb.Products.Include(x => x.Categories).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            if (product == null || !userRoles.Contains(RolesConstants.AdminRoleName) &&
                product.Store.Seller.Id != User.Id)
                throw new NotFoundException("product not found");

            if (category == null || !product.Categories.Contains(category))
                throw new NotFoundException("category not found");

            product.Categories.Remove(category);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Category, CategoryViewModel>()
                .ForMember(nameof(CategoryViewModel.ParentId), opt => opt.MapFrom(x => x.Parent.Id)));
            var mapper = new Mapper(mapperConfig);

            var categoryViewModel = mapper.Map<Category, CategoryViewModel>(category);

            return categoryViewModel;
        }

        #endregion

        #region product images

        public IEnumerable<Base64ImagePutModel> GetImages(long productId)
        {
            var product = _applicationDb.Products.Include(x => x.Images).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (product == null)
                throw new NotFoundException("product not found");

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

        public Base64ImagePutModel AddImage(long productId, long imageId)
        {
            var image = _applicationDb.Images.Include(x => x.User).Where(x => x.User.Id == User.Id)
                .FirstOrDefault(x => x.Id == imageId);

            var product = _applicationDb.Products.Include(x => x.Images).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            if (image == null)
                throw new NotFoundException("image not found");

            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            if (product == null || !userRoles.Contains(RolesConstants.AdminRoleName) &&
                product.Store.Seller.Id != User.Id)
                throw new NotFoundException("product not found");

            if (product.Images.Contains(image))
                throw new BadRequestException("the product already contains this picture");

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

            return base64ImageViewModel;
        }


        public Base64ImagePutModel RemoveImage(long productId, long imageId)
        {
            var image = _applicationDb.Images.Include(x => x.User).Where(x => x.User.Id == User.Id)
                .FirstOrDefault(x => x.Id == imageId);
            var product = _applicationDb.Products.Include(x => x.Images).Include(x => x.Store)
                .FirstOrDefault(x => x.Id == productId);

            IList<string> userRoles = _userManager.GetRolesAsync(User).Result;

            if (product == null || !userRoles.Contains(RolesConstants.AdminRoleName) &&
                product.Store.Seller.Id != User.Id)
                throw new NotFoundException("product not found");

            if (image == null || !product.Images.Contains(image))
                throw new NotFoundException("image not found");

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

            return base64ImageViewModel;
        }

        #endregion

        #region product tags
        
        public IEnumerable<TagViewModel> GetTags(long productId)
        {
            var product = _applicationDb.Products.Include(x => x.Tags)
                .FirstOrDefault(x => x.Id == productId);

            if (product == null)
                throw new NotFoundException("product not found");

            var tags = product.Tags;

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<IEnumerable<Tag>, List<TagViewModel>>(tags);

            return tagViewModels;
        }
        
        public TagViewModel AddTag(long productId, int tagId)
        {
            var product = _applicationDb.Products.Include(x => x.Tags).FirstOrDefault(x => x.Id == productId);

            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (product == null || tag == null)
                throw new NotFoundException("product or tag not found");

            if (product.Tags.Contains(tag))
                throw new BadRequestException("the product already contains the tag");

            product.Tags.Add(tag);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<Tag, TagViewModel>(tag);

            return tagViewModels;
        }
        
        public TagViewModel RemoveTag(long productId, int tagId)
        {
            var product = _applicationDb.Products.Include(x => x.Tags).FirstOrDefault(x => x.Id == productId);

            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (product == null)
                throw new NotFoundException("product not found");

            if (tag == null || !product.Tags.Contains(tag))
                throw new NotFoundException("tag not found");

            product.Tags.Remove(tag);

            _applicationDb.Products.Update(product);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<Tag, TagViewModel>(tag);

            return tagViewModels;
        }

        #endregion
    }
}