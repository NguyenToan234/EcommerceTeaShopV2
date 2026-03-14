using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using EcommerceTeaShop.Service.Contract;

public class PaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public PaymentService(
        HttpClient httpClient,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> CreatePaymentLink(long orderCode, int amount)
    {
        var description = $"TeaShop-{orderCode}";
        var returnUrl = "http://localhost:3000/payment/success";
        var cancelUrl = "http://localhost:3000/payment/cancel";

        var signature = CreateSignature(orderCode, amount, description, returnUrl, cancelUrl);

        var body = new
        {
            orderCode,
            amount,
            description,
            returnUrl,
            cancelUrl,
            items = new[]
            {
            new
            {
                name = "TeaShop",
                quantity = 1,
                price = amount
            }
        },
            signature
        };

        var jsonBody = JsonConvert.SerializeObject(body);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
    "https://api-merchant.payos.vn/v2/payment-requests"
        );

        request.Headers.Add("x-client-id", _config["PayOS:ClientId"]);
        request.Headers.Add("x-api-key", _config["PayOS:ApiKey"]);
        request.Headers.Add("Accept", "application/json");

        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine("==== PAYOS REQUEST ====");
        Console.WriteLine(jsonBody);

        Console.WriteLine("==== PAYOS RESPONSE ====");
        Console.WriteLine(content);

        var json = JObject.Parse(content);

        if (json["data"] == null)
            throw new Exception(content);

        return json["data"]["checkoutUrl"].ToString();
    }
    private string CreateSignature(long orderCode, int amount, string description, string returnUrl, string cancelUrl)
    {
        var rawData =
            $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";

        var key = _config["PayOS:ChecksumKey"];

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
    //public async Task HandleWebhook(JObject payload)
    //{
    //    var data = payload["data"];

    //    if (data == null)
    //        throw new Exception("Invalid webhook");

    //    long orderCode = data["orderCode"]?.ToObject<long>() ?? 0;

    //    string status = data["status"]?.ToString();

    //    if (status == "PAID")
    //    {
    //        await _orderService.ConfirmPayment(orderCode);
    //    }
    //}
}