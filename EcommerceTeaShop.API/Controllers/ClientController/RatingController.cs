using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateRatingDTO dto)
        {
            var clientId = Guid.Parse(User.FindFirst("id").Value);

            var result = await _service.CreateRatingAsync(clientId, dto);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("product/{productId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductRatings(Guid productId)
        {
            var result = await _service.GetProductRatingsAsync(productId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
