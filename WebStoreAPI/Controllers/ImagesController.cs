using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ImageService _imageService;
        private readonly UserManager<User> _userManager;

        public ImagesController(ImageService imageService, UserManager<User> userManager)
        {
            _imageService = imageService;
            _userManager = userManager;
        }

        private User GetUser()
        {
            return _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Base64ImageViewModel>> GetAll()
        {
            _imageService.User = GetUser();
            List<Base64ImageViewModel> imageViews = _imageService.GetAll() as List<Base64ImageViewModel>;
            return Ok(imageViews);
        }

        [HttpPost]
        public ActionResult<Base64ImageViewModel> Post(Base64ImageAddModel imageAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _imageService.User = GetUser();
            Base64ImageViewModel imageView = _imageService.Post(imageAddModel);
            return Ok(imageView);
        }

        [HttpPut]
        public ActionResult<Base64ImageViewModel> Put(Base64ImagePutModel imagePutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _imageService.User = GetUser();
            Base64ImageViewModel imageView = _imageService.Put(imagePutModel);
            return Ok(imageView);
        }

        [HttpDelete("{imageId}")]
        public ActionResult<Base64ImageViewModel> Delete(long imageId)
        {
            _imageService.User = GetUser();
            Base64ImageViewModel imageView = _imageService.Delete(imageId);
            return Ok(imageView);
        }
    }
}