using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Common.DTOs.BusinessCode;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/order")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid GetClientId()
    {
        return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var clientId = GetClientId(); // từ JWT
        var result = await _orderService.CheckoutAsync(clientId);

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders()
    {
        var clientId = GetClientId();

        var result = await _orderService.GetMyOrdersAsync(clientId);

        return StatusFromResult(result);
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var clientId = GetClientId();

        var result = await _orderService.GetOrderByIdAsync(clientId, orderId);

        return StatusFromResult(result);
    }

    private IActionResult StatusFromResult(ResponseDTO result)
    {
        if (result == null)
            return StatusCode(500, "Server error");

        return result.BusinessCode switch
        {
            BusinessCode.VALIDATION_FAILED or
            BusinessCode.INVALID_INPUT or
            BusinessCode.INVALID_DATA
                => BadRequest(result),

            BusinessCode.DATA_NOT_FOUND
                => NotFound(result),

            BusinessCode.EXCEPTION or
            BusinessCode.INTERNAL_ERROR
                => StatusCode(500, result),

            BusinessCode.INSERT_SUCESSFULLY or
            BusinessCode.CREATED_SUCCESSFULLY
                => StatusCode(201, result),

            BusinessCode.GET_DATA_SUCCESSFULLY or
            BusinessCode.UPDATE_SUCESSFULLY or
            BusinessCode.DELETE_SUCESSFULLY
                => Ok(result),

            _ => Ok(result)
        };
    }
}