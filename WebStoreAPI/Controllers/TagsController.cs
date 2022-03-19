using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebStoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WebStoreAPI.Services;

namespace WebStoreAPI.Controllers
{
    [Authorize(Roles = ApplicationConstants.AdminRoleName)]
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly TagsService _tagsService;

        public TagsController(TagsService tagsService)
        {
            _tagsService = tagsService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TagViewModel>> GetAll()
        {
            List<TagViewModel> tagViews = _tagsService.GetAll() as List<TagViewModel>;
            return Ok(tagViews);
        }

        [HttpGet("{tagId}")]
        public ActionResult<TagViewModel> Get(int tagId)
        {
            TagViewModel tagView = _tagsService.Get(tagId);
            return Ok(tagView);
        }

        [HttpPost]
        public ActionResult<TagViewModel> Post(TagAddModel tagAddModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TagViewModel tagView = _tagsService.Post(tagAddModel);
            return Ok(tagView);
        }

        [HttpPut]
        public ActionResult<TagViewModel> Put(TagViewModel tagPutModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TagViewModel tagView = _tagsService.Put(tagPutModel);
            return Ok(tagView);
        }

        [HttpDelete("{tagId}")]
        public ActionResult<TagViewModel> Delete(int tagId)
        {
            TagViewModel tagView = _tagsService.Delete(tagId);
            return Ok(tagView);
        }
    }
}