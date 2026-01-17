using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CommonLib.Interfaces;

namespace EnglishLearning.Services
{
    public class MoMoService : IMoMoService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _http;
        private readonly ILogger<MoMoService> _logger;

        public MoMoService(IConfiguration config, IHttpClientFactory http, ILogger<MoMoService> logger)
        {
            _config = config;
            _http = http;
            _logger = logger;
        }

        public async Task<(bool ok, string? payUrl, string? message)> CreatePaymentAsync(string orderId, string requestId, long amountVnd, string orderInfo, string returnUrl, string ipnUrl)
        {
            var partnerCode = _config["MoMo:PartnerCode"];
            var accessKey = _config["MoMo:AccessKey"];
            var secretKey = _config["MoMo:SecretKey"];
            var baseUrl = _config["MoMo:BaseUrl"] ?? "https://test-payment.momo.vn";

            if (string.IsNullOrEmpty(partnerCode) || string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("MoMo chưa cấu hình PartnerCode/AccessKey/SecretKey");
                return (false, null, "Chưa cấu hình cổng thanh toán MoMo.");
            }

            var requestType = "captureWallet";
            var lang = "vi";
            var extraData = "";

            var raw = $"accessKey={accessKey}&amount={amountVnd}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSha256(raw, secretKey);

            var body = new
            {
                partnerCode,
                partnerName = _config["MoMo:PartnerName"] ?? "EnglishLearning",
                storeId = _config["MoMo:StoreId"] ?? "",
                requestId,
                amount = amountVnd,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                lang,
                extraData,
                requestType,
                signature
            };

            try
            {
                var client = _http.CreateClient();
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await client.PostAsync($"{baseUrl}/v2/gateway/api/create", content);
                var str = await res.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(str);
                var root = doc.RootElement;
                var resultCode = root.TryGetProperty("resultCode", out var rc) ? rc.GetInt32() : -1;
                var message = root.TryGetProperty("message", out var m) ? m.GetString() : "";
                var payUrl = root.TryGetProperty("payUrl", out var u) ? u.GetString() : null;

                if (resultCode == 0 && !string.IsNullOrEmpty(payUrl))
                    return (true, payUrl, null);
                return (false, null, message ?? "Lỗi MoMo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MoMo CreatePayment");
                return (false, null, ex.Message);
            }
        }

        private static string HmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

