using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceTeaShop.API.Controllers.ClientController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]

    public class AddonController : ControllerBase
    {
        private readonly IAddonService _addonService;
        private readonly IAdminAddonService _adminAddonService;

        public AddonController(IAddonService addonService, IAdminAddonService adminAddonService)
        {
            _addonService = addonService;
            _adminAddonService = adminAddonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAddons()
        {
            var result = await _addonService.GetAddonsAsync();
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetAddonByProduct(Guid productId)
        {
            var result = await _adminAddonService.GetAddonByProductAsync(productId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
