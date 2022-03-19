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
using Microsoft.Extensions.Caching.Memory;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ProductsService _productsService;

        public ProductsController(ProductsService productsService, UserManager<User> userManager)
        {
            _productsService = productsService;
            _userManager = userManager;
        }

        private User GetUser()
        {
            return _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetAll()
        {
            _productsService.User = GetUser();
            List<ProductViewModel> productViews = _productsService.GetAll() as List<ProductViewModel>;

            return Ok(productViews);
        }

        [HttpGet("{id}")]
        public ActionResult<ProductViewModel> Get(long id)
        {
            _productsService.User = GetUser();
            ProductViewModel productView = _productsService.Get(id);

            return Ok(productView);
        }

        [Route("getBasedPage")]
        [HttpGet]
        public ActionResult<PageViewModel> GetBasedPage(PageGetModel pageGetModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _productsService.User = GetUser();
            PageViewModel pageView = _productsService.GetBasedPage(pageGetModel);

            return Ok(pageView);
        }

        [Route("getBasedKeyword/{keyWord}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedKeyword(string keyWord)
        {
            _productsService.User = GetUser();
            List<ProductViewModel> productViews = _productsService.GetBasedKeyword(keyWord) as List<ProductViewModel>;

            return Ok(productViews);
        }

        [Route("getBasedStore/{storeId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedStore(int storeId)
        {
            _productsService.User = GetUser();
            List<ProductViewModel> productViews = _productsService.GetBasedStore(storeId) as List<ProductViewModel>;

            return Ok(productViews);
        }

        [Route("getBasedCategory/{categoryId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedCategory(int categoryId)
        {
            _productsService.User = GetUser();
            List<ProductViewModel> productsViews =
                _productsService.GetBasedCategory(categoryId) as List<ProductViewModel>;

            return Ok(productsViews);
        }

        [Route("getBasedTag/{tagId}")]
        [HttpGet]
        public ActionResult<IEnumerable<ProductViewModel>> GetBasedTag(int tagId)
        {
            _productsService.User = GetUser();
            List<ProductViewModel> productsViews = _productsService.GetBasedTag(tagId) as List<ProductViewModel>;

            return Ok(productsViews);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpPost]
        public ActionResult<ProductViewModel> Post(ProductAddModel productAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _productsService.User = GetUser();
            ProductViewModel productView = _productsService.Post(productAddModel);

            return Ok(productView);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpDelete("{id}")]
        public ActionResult<ProductViewModel> Delete(long id)
        {
            _productsService.User = GetUser();
            ProductViewModel productView = _productsService.Delete(id);

            return Ok(productView);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [HttpPut]
        public ActionResult<ProductViewModel> Put(ProductPutModel productPutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _productsService.User = GetUser();
            ProductViewModel productView = _productsService.Put(productPutModel);

            return Ok(productView);
        }

        #region product rating

        [Route("{productId}/getReviews")]
        [HttpGet]
        public ActionResult<IEnumerable<ReviewViewModel>> GetReviews(long productId)
        {
            _productsService.User = GetUser();
            List<ReviewViewModel> reviewViews = _productsService.GetReviews(productId) as List<ReviewViewModel>;

            return Ok(reviewViews);
        }

        [Authorize]
        [Route("rate")]
        [HttpPut]
        public ActionResult<ReviewViewModel> RateProduct(ReviewAddModel reviewAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = GetUser();

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "email not confirmed");
                return BadRequest(ModelState);
            }

            _productsService.User = user;
            ReviewViewModel reviewView = _productsService.RateProduct(reviewAddModel);

            return Ok(reviewView);
        }

        #endregion

        #region product categories

        [Route("{productId}/getCategories")]
        [HttpGet]
        public ActionResult<IEnumerable<CategoryViewModel>> GetCategories(long productId)
        {
            _productsService.User = GetUser();
            List<CategoryViewModel> categoryViews =
                _productsService.GetCategories(productId) as List<CategoryViewModel>;

            return Ok(categoryViews);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/addСategory/{categoryId}")]
        [HttpPut]
        public ActionResult<CategoryViewModel> AddCategory(int productId, int categoryId)
        {
            _productsService.User = GetUser();
            CategoryViewModel categoryView = _productsService.AddCategory(productId, categoryId);

            return Ok(categoryView);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/removeСategory/{categoryId}")]
        [HttpPut]
        public ActionResult<CategoryViewModel> RemoveCategory(int productId, int categoryId)
        {
            _productsService.User = GetUser();
            CategoryViewModel categoryView = _productsService.RemoveCategory(productId, categoryId);

            return Ok(categoryView);
        }

        #endregion

        #region product images

        [Route("{productId}/getImages")]
        [HttpGet]
        public ActionResult<IEnumerable<Base64ImagePutModel>> GetImages(long productId)
        {
            _productsService.User = GetUser();
            List<Base64ImagePutModel> base64ImageViews =
                _productsService.GetImages(productId) as List<Base64ImagePutModel>;

            return Ok(base64ImageViews);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/addImage/{imageId}")]
        [HttpPut]
        public ActionResult<Base64ImagePutModel> AddImage(long productId, long imageId)
        {
            _productsService.User = GetUser();
            Base64ImagePutModel base64ImageView = _productsService.AddImage(productId, imageId);

            return Ok(base64ImageView);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName + ", " + ApplicationConstants.SellerRoleName)]
        [Route("{productId}/removeImage/{imageId}")]
        [HttpPut]
        public ActionResult<Base64ImagePutModel> RemoveImage(long productId, long imageId)
        {
            _productsService.User = GetUser();
            Base64ImagePutModel base64ImageView = _productsService.RemoveImage(productId, imageId);

            return Ok(base64ImageView);
        }

        #endregion

        #region product tags

        [Route("{productId}/gettags")]
        [HttpGet]
        public ActionResult<IEnumerable<TagViewModel>> GetTags(long productId)
        {
            _productsService.User = GetUser();
            List<TagViewModel> tagViews = _productsService.GetTags(productId) as List<TagViewModel>;

            return tagViews;
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [Route("{productId}/addTag/{tagId}")]
        [HttpPut]
        public ActionResult<TagViewModel> AddTag(long productId, int tagId)
        {
            _productsService.User = GetUser();
            TagViewModel tagView = _productsService.AddTag(productId, tagId);

            return Ok(tagView);
        }

        [Authorize(Roles = ApplicationConstants.AdminRoleName)]
        [Route("{productId}/removeTag/{tagId}")]
        [HttpPut]
        public ActionResult<TagViewModel> RemoveTag(long productId, int tagId)
        {
            _productsService.User = GetUser();
            TagViewModel tagView = _productsService.RemoveTag(productId, tagId);

            return Ok(tagView);
        }

        #endregion
    }
}