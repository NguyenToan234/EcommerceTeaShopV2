using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceTeaShop.API.Controllers.ClientController
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _service;

        public BlogController(IBlogService service)
        {
            _service = service;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetBlogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetBlogsAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBlogDetail(Guid id)
        {
            var result = await _service.GetBlogDetailAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
