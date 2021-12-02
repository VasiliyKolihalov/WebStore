using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;

        public ImagesController(ApplicationContext applicationContext)
        {
            _applicationDB = applicationContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Base64ImageViewModel>> Get()
        {
            var images = _applicationDB.Images;

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                                                 .ForMember("ImageData", opt => opt
                                                                 .MapFrom(x => Convert.ToBase64String(x.ImageData))));

            var mapper = new Mapper(mapperConfig);

            var base64Images = mapper.Map<IEnumerable<Image>, List<Base64ImageViewModel>>(images);
       
            return base64Images;
        }

        [HttpGet("{imageId}")]
        public ActionResult<Base64ImageViewModel> Get(long imageId)
        {
            var image = _applicationDB.Images.FirstOrDefault(x => x.Id == imageId);

            if (image == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                                                 .ForMember("ImageData", opt => opt
                                                                 .MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return base64ImageView;
        }

        [HttpPost]
        public ActionResult<Image> Post(Base64ImageAddModel imageAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base64ImageAddModel, Image>()
                                .ForMember("ImageData", opt => opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));

                cfg.CreateMap<Image, Base64ImageViewModel>()
                                .ForMember("ImageData", opt => opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
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

            _applicationDB.Images.Add(image);
            _applicationDB.SaveChanges();

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageView);
        }

        [HttpPut]
        public ActionResult<Image> Put(Base64ImageViewModel imageViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!_applicationDB.Images.Any(x => x.Id == imageViewModel.Id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Base64ImageViewModel, Image>()
                                                                 .ForMember("ImageData", opt => opt
                                                                 .MapFrom(x => Convert.FromBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            Image image;
            try
            {
                image = mapper.Map<Base64ImageViewModel, Image>(imageViewModel);
            }
            catch
            {
                return BadRequest("base64 conversion error");
            }

            _applicationDB.Images.Update(image);
            _applicationDB.SaveChanges();

            return Ok(imageViewModel);
        }

        [HttpDelete("{imageId}")]
        public ActionResult<Image> Delete(long imageId)
        {
            var image = _applicationDB.Images.FirstOrDefault(x => x.Id == imageId);

            if (image == null)
                return NotFound();

            _applicationDB.Remove(image);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Image, Base64ImageViewModel>()
                                                                 .ForMember("ImageData", opt => opt
                                                                 .MapFrom(x => Convert.ToBase64String(x.ImageData))));
            var mapper = new Mapper(mapperConfig);

            var base64ImageView = mapper.Map<Image, Base64ImageViewModel>(image);

            return Ok(base64ImageView);
        }

    }
}
