using Microsoft.AspNetCore.Mvc;
using Bokado.Server.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bokado.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiqPayController : ControllerBase
    {
        private readonly ISubscribeRepository _subscribeRepository;
        private readonly IConfiguration _configuration;

        private string PublicKey  => Environment.GetEnvironmentVariable("LIQPAY_PUBLIC_KEY")  ?? "sandbox_i44274234488";
        private string PrivateKey => Environment.GetEnvironmentVariable("LIQPAY_PRIVATE_KEY") ?? "sandbox_V1qRaoPMIMdhYxpoygKvO62FaMN2zEIw7QZ4xxOi";

        public LiqPayController(ISubscribeRepository subscribeRepository, IConfiguration configuration)
        {
            _subscribeRepository = subscribeRepository;
            _configuration = configuration;
        }

        // POST /api/LiqPay/create-payment
        // Генерує data + signature для кнопки LiqPay на фронті
        [HttpPost("create-payment")]
        public IActionResult CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var paymentData = new
            {
                version    = 3,
                public_key = PublicKey,
                action     = "pay",
                amount     = "100",
                currency   = "UAH",
                description = "Bokado Premium підписка",
                order_id   = $"premium_{request.UserId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                result_url = "https://bokado.website/premium?success=true",
                server_url = "https://bokadoserver-production.up.railway.app/api/LiqPay/callback",
                sandbox    = PublicKey.StartsWith("sandbox") ? 1 : 0,
                info       = request.UserId.ToString(),
            };

            var json   = JsonSerializer.Serialize(paymentData);
            var data   = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            var sign   = GenerateSignature(data);

            return Ok(new { data, signature = sign });
        }

        // POST /api/LiqPay/callback
        // LiqPay надсилає сюди результат після оплати
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromForm] string data, [FromForm] string signature)
        {
            // Перевіряємо підпис
            var expectedSign = GenerateSignature(data);
            if (expectedSign != signature)
                return BadRequest("Invalid signature");

            // Декодуємо дані
            var json     = Encoding.UTF8.GetString(Convert.FromBase64String(data));
            var payment  = JsonSerializer.Deserialize<LiqPayCallbackData>(json);

            if (payment == null) return BadRequest("Invalid data");

            // Активуємо Premium тільки якщо статус success або sandbox
            if (payment.status == "success" || payment.status == "sandbox")
            {
                if (int.TryParse(payment.info, out int userId))
                {
                    await _subscribeRepository.GiveSubscribe(userId);
                }
            }

            return Ok();
        }

        private string GenerateSignature(string data)
        {
            var str  = PrivateKey + data + PrivateKey;
            var hash = SHA1.HashData(Encoding.UTF8.GetBytes(str));
            return Convert.ToBase64String(hash);
        }
    }

    public class CreatePaymentRequest
    {
        public int UserId { get; set; }
    }

    public class LiqPayCallbackData
    {
        public string? status   { get; set; }
        public string? order_id { get; set; }
        public string? info     { get; set; }
        public double  amount   { get; set; }
    }
}
