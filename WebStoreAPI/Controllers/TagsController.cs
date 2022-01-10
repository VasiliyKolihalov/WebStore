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
    [Authorize(Roles = RolesConstants.AdminRoleName)]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ApplicationContext _applicationDB;

        public TagsController(ApplicationContext applicationContext)
        {
            _applicationDB = applicationContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TagViewModel>> GetAll()
        {
            var tags = _applicationDB.Tags.ToList();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModels = mapper.Map<IEnumerable<Tag>, List<TagViewModel>>(tags);

            return tagViewModels;
        }

        [HttpGet("{id}")]
        public ActionResult<TagViewModel> Get(int id)
        {
            var tag = _applicationDB.Tags.FirstOrDefault(x => x.Id == id);

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
                return BadRequest();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TagAddModel, Tag>();
                cfg.CreateMap<Tag, TagViewModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var tag = mapper.Map<TagAddModel, Tag>(tagAddModel);

            _applicationDB.Tags.Add(tag);
            _applicationDB.SaveChanges();

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return Ok(tagViewModel);
        }

        [HttpPut]
        public ActionResult<TagViewModel> Put(TagViewModel tagViewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!_applicationDB.Tags.Any(x => x.Id == tagViewModel.Id))
                return NotFound();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TagViewModel, Tag>());
            var mapper = new Mapper(mapperConfig);

            var tag = mapper.Map<TagViewModel, Tag>(tagViewModel);

            _applicationDB.Tags.Update(tag);
            _applicationDB.SaveChanges();

            return Ok(tag);
        }

        [HttpDelete("{id}")]
        public ActionResult<TagViewModel> Delete(int id)
        {
            var tag = _applicationDB.Tags.FirstOrDefault(x => x.Id == id);

            if (tag == null)
                return NotFound();

            _applicationDB.Tags.Remove(tag);
            _applicationDB.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Tag, TagViewModel>());
            var mapper = new Mapper(mapperConfig);

            var tagViewModel = mapper.Map<Tag, TagViewModel>(tag);

            return Ok(tagViewModel);
        }
    }
}
