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

        public AddonController(IAddonService addonService)
        {
            _addonService = addonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAddons()
        {
            var result = await _addonService.GetAddonsAsync();
            return Ok(result);
        }
    }
}
