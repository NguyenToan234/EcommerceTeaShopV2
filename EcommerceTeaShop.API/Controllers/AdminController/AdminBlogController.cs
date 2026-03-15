using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceTeaShop.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminBlogController : ControllerBase
    {
        private readonly IAdminBlogService _service;

        public AdminBlogController(IAdminBlogService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateBlogDTO dto)
        {
            var result = await _service.CreateBlogAsync(dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllBlogsAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("update/{blogId:guid}")]
        public async Task<IActionResult> Update(Guid blogId, [FromForm] UpdateBlogDTO dto)
        {
            var result = await _service.UpdateBlogAsync(blogId, dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpDelete("delete/{blogId:guid}")]
        public async Task<IActionResult> Delete(Guid blogId)
        {
            var result = await _service.DeleteBlogAsync(blogId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
