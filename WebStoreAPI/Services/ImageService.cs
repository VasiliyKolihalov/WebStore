using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class ImageService
    {
        private readonly ApplicationContext _applicationDb;
        private readonly UserManager<User> _userManager;

        public ImageService(ApplicationContext applicationContext, UserManager<User> userManager)
        {
            _applicationDb = applicationContext;
            _userManager = userManager;
        }

       public User User { private get; set; }
       
        public IEnumerable<Base64ImageViewModel> GetAll()
        {
            var images = _applicationDb.Images.Include(x => x.User).Where(x => x.User.Id == User.Id);

            var mapperConfig = new MapperConfiguration(cfg =>
                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                    opt.MapFrom(x => Convert.ToBase64String(x.ImageData))));

            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModels = mapper.Map<IEnumerable<Image>, List<Base64ImageViewModel>>(images);

            return base64ImageViewModels;
        }

        public Base64ImageViewModel Post(Base64ImageAddModel imageAddModel)
        {
            if (!User.EmailConfirmed)
            {
                throw new BadRequestException("email not confirmed");
            }

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base64ImageAddModel, Image>()
                    .ForMember(nameof(Image.ImageData), opt => opt.MapFrom(x => Convert.FromBase64String(x.ImageData)));

                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                    opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
            });

            var mapper = new Mapper(mapperConfig);

            Image image;
            try
            {
                image = mapper.Map<Base64ImageAddModel, Image>(imageAddModel);
            }
            catch
            {
                throw new Exception("base64 converting error");
            }

            image.User = User;

            _applicationDb.Images.Add(image);
            _applicationDb.SaveChanges();

            var base64ImageViewModel = mapper.Map<Image, Base64ImageViewModel>(image);
            return base64ImageViewModel;
        }
        
        public Base64ImageViewModel Put(Base64ImagePutModel imagePutModel)
        {
            var image = _applicationDb.Images.Include(x => x.User).AsNoTracking()
                .FirstOrDefault(x => x.Id == imagePutModel.Id);

            if (image == null || image.User.Id != User.Id)
                throw new NotFoundException("image not found");
            
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base64ImagePutModel, Image>().ForMember(nameof(Image.ImageData), opt => opt
                    .MapFrom(x => Convert.FromBase64String(x.ImageData)));

                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                    opt.MapFrom(x => Convert.ToBase64String(x.ImageData)));
            });
            var mapper = new Mapper(mapperConfig);

            try
            {
                image = mapper.Map<Base64ImagePutModel, Image>(imagePutModel);
            }
            catch
            {
                throw new Exception("base64 converting error");
            }

            _applicationDb.Images.Update(image);
            _applicationDb.SaveChanges();

            var base64ImageViewModel = mapper.Map<Image, Base64ImageViewModel>(image);
            return base64ImageViewModel;
        }
        
        public Base64ImageViewModel Delete(long imageId)
        {
            var image = _applicationDb.Images.Include(x => x.User).FirstOrDefault(x => x.Id == imageId);

            if (image == null || image.User.Id != User.Id)
                throw new NotFoundException("image not found");

            _applicationDb.Remove(image);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
                cfg.CreateMap<Image, Base64ImageViewModel>().ForMember(nameof(Base64ImageViewModel.ImageData), opt =>
                    opt.MapFrom(x => Convert.ToBase64String(x.ImageData))));

            var mapper = new Mapper(mapperConfig);

            var base64ImageViewModel = mapper.Map<Image, Base64ImageViewModel>(image);

            return base64ImageViewModel;
        }
    }
}