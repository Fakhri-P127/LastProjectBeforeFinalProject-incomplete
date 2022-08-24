using LastGrind.Application.Contracts.v1.Requests.Post;
using LastGrind.Application.Contracts.v1.Responses.Post;
using LastGrind.Application.Interfaces;
using LastGrind.Domain.Entities;
using LastGrind.WebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastGrind.WebApi.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles ="Admin,User")]
    public class PostsController : ControllerBase
    {
        private readonly IUnitOfWork _unit;

        public PostsController(IUnitOfWork unit)
        {
            _unit = unit;
          
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _unit.PostRepository.GetAllAsync());
        }
        [HttpGet("{id}")]
        [ResponseCache(Duration = 10)]
        [Authorize(Roles ="Admin")]

        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            var result = await _unit.PostRepository.GetByIdAsync(id);
            if (result is null) return BadRequest(new
            {
                code = "Id",
                desc= "Entered Id is invalid"
            });
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Policy = "MustHaveCompanyDomain")]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
        {
            Post post = new()
            {
                Name = request.Name,
                UserId = HttpContext.GetUserId()
            };
            var result = await _unit.PostRepository.AddAsync(post);
            if (!result) return BadRequest();
            var response = new PostResponse()
            {
                Id = post.Id,
                Name = post.Name,
                UserId = HttpContext.GetUserId()
            };
            return CreatedAtAction(nameof(GetById),routeValues: new {id=response.Id}, new { post= response });

        }

        [HttpPut("{id}")]
        //[Authorize(Roles ="Admin")]
        //[Authorize(Policy = "TagViewer")]
        public async Task<IActionResult> Update( [FromRoute]Guid id, [FromBody] CreatePostRequest request)
        {
            bool userOwnsPost = await _unit.PostRepository.UserOwnsPostAsync(id, HttpContext.GetUserId());
            if (!userOwnsPost) return BadRequest(new {error="This post isn't yours."});

            Post post = await _unit.PostRepository.GetByIdAsync(id);
            if (post == null) return NotFound();

            await _unit.PostRepository.Update(post);
            post.Name = request.Name;
            
            await _unit.SaveChangesAsync();
            
            return Ok(post);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            bool userOwnsPost = await _unit.PostRepository.UserOwnsPostAsync(id, HttpContext.GetUserId());
            if (!userOwnsPost) return BadRequest(new { error = "This post isn't yours." });

            var result = await _unit.PostRepository.RemoveAsync(id);
            if (!result) return BadRequest();

            return NoContent();
        }
    }
}
