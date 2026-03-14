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
    public class AdminProductController : ControllerBase
    {
        private readonly IAdminProductService _service;

        public AdminProductController(IAdminProductService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO dto)
        {
            var result = await _service.CreateProductAsync(dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllProductsAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("detail/{productId:guid}")]
        public async Task<IActionResult> GetDetail(Guid productId)
        {
            var result = await _service.GetProductDetailAsync(productId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("update/{productId:guid}")]
        public async Task<IActionResult> Update(Guid productId, [FromForm] UpdateProductDTO dto)
        {
            var result = await _service.UpdateProductAsync(productId, dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpDelete("delete/{productId:guid}")]
        public async Task<IActionResult> Delete(Guid productId)
        {
            var result = await _service.DeleteProductAsync(productId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpDelete("delete-image/{imageId:guid}")]
        public async Task<IActionResult> DeleteImage(Guid imageId)
        {
            var result = await _service.DeleteProductImageAsync(imageId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("set-main-image/{imageId:guid}")]
        public async Task<IActionResult> SetMainImage(Guid imageId)
        {
            var result = await _service.SetMainImageAsync(imageId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
