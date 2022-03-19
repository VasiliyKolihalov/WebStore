using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class TagsService
    {
        private readonly IApplicationContext _applicationDb;

        public TagsService(IApplicationContext applicationContext)
        {
            _applicationDb = applicationContext;
        }

        public IEnumerable<TagViewModel> GetAll()
        {
            var tags = _applicationDb.Tags.ToList();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<IEnumerable<Tag>, List<TagViewModel>>(tags);

            return tagViewModels;
        }

        public TagViewModel Get(int tagId)
        {
            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                throw new NotFoundException("tag not found");

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return tagViewModel;
        }
        
        public TagViewModel Post(TagAddModel tagAddModel)
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TagAddModel, Tag>();
                cfg.CreateMap<Tag, TagViewModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var tag = mapper.Map<TagAddModel, Tag>(tagAddModel);

            _applicationDb.Tags.Add(tag);
            _applicationDb.SaveChanges();

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return tagViewModel;
        }

        public TagViewModel Put(TagViewModel tagViewModel)
        {
            if (!_applicationDb.Tags.Any(x => x.Id == tagViewModel.Id))
                throw new NotFoundException("tag not found");

            var mapperConfig = new MapperConfiguration(cfg =>
                cfg.CreateMap<TagViewModel, Tag>());
            var mapper = new Mapper(mapperConfig);

            var tag = mapper.Map<TagViewModel, Tag>(tagViewModel);

            _applicationDb.Tags.Update(tag);
            _applicationDb.SaveChanges();

            return tagViewModel;
        }
        
        public TagViewModel Delete(int tagId)
        {
            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                throw new NotFoundException("tag not found");

            _applicationDb.Tags.Remove(tag);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return tagViewModel;
        }
    }
}