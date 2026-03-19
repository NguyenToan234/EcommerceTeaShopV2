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
    public class AdminAddonController : ControllerBase
    {
        private readonly IAdminAddonService _service;

        public AdminAddonController(IAdminAddonService service)
        {
            _service = service;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateAddonDTO dto)
        {
            var result = await _service.CreateAddonAsync(dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllAddonAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("update/{addonId:guid}")]
        public async Task<IActionResult> Update(Guid addonId, [FromForm] UpdateAddonDTO dto)
        {
            var result = await _service.UpdateAddonAsync(addonId, dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpDelete("delete/{addonId:guid}")]
        public async Task<IActionResult> Delete(Guid addonId)
        {
            var result = await _service.DeleteAddonAsync(addonId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPost("assign/{productId:guid}")]
        public async Task<IActionResult> Assign(Guid productId, [FromBody] AssignAddonDTO dto)
        {
            var result = await _service.AssignAddonToProduct(productId, dto.AddonIds);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetByProduct(Guid productId)
        {
            var result = await _service.GetAddonByProductAsync(productId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
