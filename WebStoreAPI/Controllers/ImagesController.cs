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

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = RolesConstants.AdminRoleName + ", " + RolesConstants.SellerRoleName)]
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;
        private UserManager<User> _userManager;
        private User _user;

        public ImagesController(ApplicationContext applicationContext, UserManager<User> userManager)
        {
            _applicationDB = applicationContext;
            _userManager = userManager;
        }

        private void SetUser()
        {
            _user = _userManager.GetUserAsync(HttpContext.User).Result;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Base64ImageViewModel>> GetAll()
        {
            SetUser();
            var images = _applicationDB.Images.Include(x => x.User).Where(x => x.User.Id == _user.Id);


            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt => 
                                                            opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));

                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt =>opt.MapFrom(x => x.UserName));

            });

            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModels = mapper.Map<IEnumerable<Image>, List<Base64ImageViewModel>>(images);

            return base64ImageViewModels;

        }

       
        [HttpPost]
        public ActionResult<Image> Post(Base64ImageAddModel imageAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base64ImageAddModel, Image>()
                                .ForMember(nameof(Image.ImageData), opt => opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));

                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                                                            opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));

                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });

            var mapper = new Mapper(mapperConfig);

            Image image;
            try
            {
                image = mapper.Map<Base64ImageAddModel, Image>(imageAddModel);
            }
            catch
            {
                return BadRequest("base64 conversion error");
            }

            SetUser();
            image.User = _user;

            _applicationDB.Images.Add(image);
            _applicationDB.SaveChanges();

            var base64ImageViewModel = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageViewModel);
        }

        [HttpPut]
        public ActionResult<Base64ImageViewModel> Put(Base64ImagePutModel imagePutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var image = _applicationDB.Images.Include(x => x.User).AsNoTracking().FirstOrDefault(x => x.Id == imagePutModel.Id);

            if (image == null)
                return NotFound();

            SetUser();
            if (image.User.Id != _user.Id)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base64ImagePutModel, Image>().ForMember(nameof(Image.ImageData), opt => opt
                                                           .MapFrom(x => Convert.FromBase64String(x.ImageData)));

                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                                                            opt.MapFrom(x => Convert.ToBase64String(x.ImageData))); ;

                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            try
            {
                image = mapper.Map<Base64ImagePutModel, Image>(imagePutModel);
            }
            catch
            {
                return BadRequest("base64 conversion error");
            }

            _applicationDB.Images.Update(image);
            _applicationDB.SaveChanges();

            var base64ImageViewModel = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageViewModel);
        }

        [HttpDelete("{imageId}")]
        public ActionResult<Base64ImageViewModel> Delete(long imageId)
        {
            var image = _applicationDB.Images.Include(x => x.User).FirstOrDefault(x => x.Id == imageId);

            if (image == null)
                return NotFound();

            SetUser();
            if (image.User.Id != _user.Id)
                return BadRequest();

            _applicationDB.Remove(image);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt => opt
                                                            .MapFrom(x => Convert.ToBase64String(x.ImageData)));

                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                                                            opt.MapFrom(x => Convert.ToBase64String(x.ImageData))); ;

                cfg.CreateMap<User, UserViewModel>().ForMember(nameof(UserViewModel.Name), opt => opt.MapFrom(x => x.UserName));
            });
            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModel = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageViewModel);
        }

    }
}
