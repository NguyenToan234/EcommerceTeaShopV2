using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceTeaShop.API.Controllers.ClientController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class RatingController : ControllerBase
    {
        private readonly IUserRatingService _service;

        public RatingController(IUserRatingService service)
        {
            _service = service;
        }
        private Guid GetUserId()
        {
            var claim = User.FindFirst("id")
                     ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new UnauthorizedAccessException("Token không hợp lệ");

            return Guid.Parse(claim.Value);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateRatingDTO dto)
        {
            var result = await _service.CreateRatingAsync(GetUserId(), dto);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [AllowAnonymous]
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetProductRatings(Guid productId)
        {
            var result = await _service.GetProductRatingsAsync(productId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            try
            {
                var userId = GetUserId();

                var result = await _service.GetMyProductsForRatingAsync(userId);

                return StatusCode(result.IsSucess ? 200 : 400, result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new ResponseDTO
                {
                    IsSucess = false,
                    Message = ex.Message
                });
            }
        }
    }
}
