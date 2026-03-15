using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceTeaShop.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _service;

        public AdminDashboardController(IAdminDashboardService service)
        {
            _service = service;
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetDashboardAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueChart(string type = "week")
        {
            var result = await _service.GetRevenueChartAsync(type);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetRecentTransactions()
        {
            var result = await _service.GetRecentTransactionsAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
