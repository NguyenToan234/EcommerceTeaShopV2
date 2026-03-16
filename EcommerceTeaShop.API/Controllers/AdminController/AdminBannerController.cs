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
    public class AdminBannerController : ControllerBase
    {
        private readonly IAdminBannerService _service;

        public AdminBannerController(IAdminBannerService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateBannerDTO dto)
        {
            var result = await _service.CreateBannerAsync(dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateBannerDTO dto)
        {
            var result = await _service.UpdateBannerAsync(id, dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteBannerAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllBannersAsync();
            return Ok(result);
        }
    }
}
