using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceTeaShop.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : ControllerBase
    {
        private readonly IAdminOrderService _service;

        public AdminOrderController(IAdminOrderService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders(
       string? keyword,
       string sort = "newest",
       string type = "all",
       int pageNumber = 1,
       int pageSize = 10)
        {
            var result = await _service.GetOrdersAsync(keyword, sort, type, pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _service.GetOrderDetailAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _service.GetOrderStatsAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
