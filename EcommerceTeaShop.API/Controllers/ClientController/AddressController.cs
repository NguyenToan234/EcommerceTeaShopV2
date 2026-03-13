using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[Route("api/address")]
[ApiController]
[Authorize(Roles = "User")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private Guid GetClientId()
    {
        var claim = User.Claims.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type.EndsWith("/nameidentifier"));

        return Guid.Parse(claim.Value);
    }

    // Thêm địa chỉ
    [HttpPost]
    public async Task<IActionResult> Add(CreateAddressDTO dto)
    {
        var clientId = GetClientId();

        var result = await _addressService.AddAddressAsync(clientId, dto);

        return StatusFromResult(result);
    }

    // Lấy danh sách địa chỉ
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var clientId = GetClientId();

        var result = await _addressService.GetMyAddressesAsync(clientId);

        return StatusFromResult(result);
    }

    // Xóa địa chỉ
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = GetClientId();

        var result = await _addressService.DeleteAddressAsync(clientId, id);

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