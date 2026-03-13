using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[Route("api/cart")]
[ApiController]
[Authorize(Roles = "User")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetClientId()
    {
        var claim = User.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type.EndsWith("/nameidentifier"));

        return Guid.Parse(claim.Value);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(AddToCartDTO dto)
    {
        var clientId = GetClientId();

        var result = await _cartService.AddToCartAsync(clientId, dto);

        return StatusFromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var clientId = GetClientId();

        var result = await _cartService.GetCartAsync(clientId);

        return StatusFromResult(result);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateQuantity(UpdateCartItemDTO dto)
    {
        var clientId = GetClientId();

        var result = await _cartService.UpdateQuantityAsync(clientId, dto);

        return StatusFromResult(result);
    }

    [HttpDelete("remove/{cartItemId}")]
    public async Task<IActionResult> Remove(Guid cartItemId)
    {
        var clientId = GetClientId();

        var result = await _cartService.RemoveItemAsync(clientId, cartItemId);

        return StatusFromResult(result);
    }

    private IActionResult StatusFromResult(ResponseDTO result)
    {
        return result.BusinessCode switch
        {
            BusinessCode.VALIDATION_FAILED => BadRequest(result),

            BusinessCode.DATA_NOT_FOUND => NotFound(result),

            BusinessCode.EXCEPTION => StatusCode(500, result),

            BusinessCode.INSERT_SUCESSFULLY => StatusCode(201, result),

            BusinessCode.GET_DATA_SUCCESSFULLY or
            BusinessCode.UPDATE_SUCESSFULLY or
            BusinessCode.DELETE_SUCESSFULLY
                => Ok(result),

            _ => Ok(result)
        };
    }
}