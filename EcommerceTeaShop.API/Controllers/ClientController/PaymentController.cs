using EcommerceTeaShop.Common.DTOs;
using EcommerceTeaShop.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EcommerceTeaShop.API.Controllers.ClientController
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentController(
         PaymentService paymentService,
         IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(int amount)
        {
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var checkoutUrl = await _paymentService.CreatePaymentLink(orderCode, amount);

            return Ok(new
            {
                checkoutUrl
            });
        }

        // PayOS redirect khi thanh toán thành công
        [HttpGet("/success")]
        public async Task<IActionResult> Success(
      [FromQuery] string status,
      [FromQuery] long orderCode
  )
        {
            if (status == "PAID")
            {
                await _orderService.ConfirmPayment(orderCode);
            }

            return Ok(new
            {
                message = "Thanh toán thành công",
                status,
                orderCode
            });
        }

        // PayOS redirect khi cancel
        [HttpGet("/cancel")]
        public IActionResult Cancel()
        {
            return Ok(new
            {
                message = "Thanh toán đã bị huỷ"
            });
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] PayOSWebhookDTO payload)
        {
            try
            {
                if (payload?.data == null)
                    return BadRequest("Invalid webhook payload");

                long orderCode = payload.data.orderCode;
                string status = payload.data.status;

                Console.WriteLine($"[Webhook] OrderCode: {orderCode} - Status: {status}");

                if (orderCode == 0)
                    return BadRequest("Invalid orderCode");

                if (status == "PAID")
                {
                    await _orderService.ConfirmPayment(orderCode);
                }

                return Ok(new { message = "Webhook processed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Webhook error: " + ex.Message);
                return StatusCode(500);
            }
        }
    }
}
