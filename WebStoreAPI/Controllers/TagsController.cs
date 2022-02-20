using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebStoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = ApplicationConstants.AdminRoleName)]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly IApplicationContext _applicationDb;

        public TagsController(IApplicationContext applicationContext)
        {
            _applicationDb = applicationContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TagViewModel>> GetAll()
        {
            var tags = _applicationDb.Tags.ToList();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<IEnumerable<Tag>, List<TagViewModel>>(tags);

            return tagViewModels;
        }

        [HttpGet("{tagId}")]
        public ActionResult<TagViewModel> Get(int tagId)
        {
            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return tagViewModel;
        }

        [HttpPost]
        public ActionResult<TagViewModel> Post(TagAddModel tagAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            return Ok(tagViewModel);
        }

        [HttpPut]
        public ActionResult<TagViewModel> Put(TagViewModel tagViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_applicationDb.Tags.Any(x => x.Id == tagViewModel.Id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TagViewModel, Tag>());
            var mapper = new Mapper(mapperConfig);

            var tag = mapper.Map<TagViewModel, Tag>(tagViewModel);

            _applicationDb.Tags.Update(tag);
            _applicationDb.SaveChanges();

            return Ok(tag);
        }

        [HttpDelete("{tagId}")]
        public ActionResult<TagViewModel> Delete(int tagId)
        {
            var tag = _applicationDb.Tags.FirstOrDefault(x => x.Id == tagId);

            if (tag == null)
                return NotFound();

            _applicationDb.Tags.Remove(tag);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return Ok(tagViewModel);
        }
    }
}