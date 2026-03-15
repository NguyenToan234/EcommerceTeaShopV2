using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace EcommerceTeaShop.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _service;

        public AdminUserController(IAdminUserService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers(
        string? keyword = "",
        int pageNumber = 1,
        int pageSize = 10)
        {
            var result = await _service.GetUsersAsync(keyword, pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetail(Guid id)
        {
            var result = await _service.GetUserDetailAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("block/{id}")]
        public async Task<IActionResult> BlockUser(Guid id)
        {
            var result = await _service.BlockUserAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpPut("unblock/{id}")]
        public async Task<IActionResult> UnblockUser(Guid id)
        {
            var result = await _service.UnblockUserAsync(id);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var result = await _service.GetUserReviewStatsAsync();
            return Ok(result);
        }
        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews(
   string? keyword = "",
    int pageNumber = 1,
    int pageSize = 10)
        {
            var result = await _service.GetReviewsAsync(keyword, pageNumber, pageSize);
            return Ok(result);
        
        }
        [HttpPut("reviews/approve/{reviewId}")]
        public async Task<IActionResult> Approve(Guid reviewId)
        {
            var result = await _service.ApproveReviewAsync(reviewId);
            return Ok(result);
        }
    }
}
