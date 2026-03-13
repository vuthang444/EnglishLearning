using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CommonLib.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EnglishLearning.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Encode giống PHP urlencode: space thành '+' để chuỗi hash khớp VNPay.
        /// </summary>
        private static string UrlEncodeForVnPay(string value)
        {
            return Uri.EscapeDataString(value).Replace("%20", "+");
        }

        public string? CreatePaymentUrl(string orderRef, long amountVnd, string orderInfo, string returnUrl, string clientIp)
        {
            var tmnCode = _config["Vnpay:TmnCode"];
            var hashSecret = _config["Vnpay:HashSecret"];
            var baseUrl = _config["Vnpay:BaseUrl"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl))
                return null;

            var vnpAmount = amountVnd * 100;

            var timeZoneId = _config["TimeZoneId"] ?? "SE Asia Standard Time";
            TimeZoneInfo tz;
            try { tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
            catch
            {
                try { tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok"); }
                catch { tz = TimeZoneInfo.Utc; }
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var createDate = now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var expireDate = now.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            var dict = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = _config["Vnpay:Version"] ?? "2.1.0",
                ["vnp_Command"] = _config["Vnpay:Command"] ?? "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = vnpAmount.ToString(CultureInfo.InvariantCulture),
                ["vnp_CreateDate"] = createDate,
                ["vnp_CurrCode"] = _config["Vnpay:CurrCode"] ?? "VND",
                ["vnp_IpAddr"] = clientIp,
                ["vnp_Locale"] = _config["Vnpay:Locale"] ?? "vn",
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_ExpireDate"] = expireDate,
                ["vnp_TxnRef"] = orderRef
            };

            var hashData = new StringBuilder();
            var query = new StringBuilder();
            var first = true;
            foreach (var kv in dict)
            {
                if (first) first = false;
                else { hashData.Append('&'); query.Append('&'); }
                var keyEnc = UrlEncodeForVnPay(kv.Key);
                var valEnc = UrlEncodeForVnPay(kv.Value);
                hashData.Append(keyEnc).Append('=').Append(valEnc);
                query.Append(keyEnc).Append('=').Append(valEnc);
            }

            var secureHash = HmacSha512(hashSecret, hashData.ToString());
            query.Append("&vnp_SecureHash=").Append(secureHash);

            return baseUrl + "?" + query;
        }

        public bool ValidateReturnSignature(IQueryCollection query, string? vnpSecureHash)
        {
            if (string.IsNullOrEmpty(vnpSecureHash)) return false;
            var hashSecret = _config["Vnpay:HashSecret"];
            if (string.IsNullOrEmpty(hashSecret)) return false;

            var dict = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var key in query.Keys)
            {
                if (key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
                {
                    var val = query[key].ToString();
                    if (!string.IsNullOrEmpty(val)) dict[key] = val;
                }
            }

            return ValidateReturnSignature(dict, vnpSecureHash);
        }

        public bool ValidateReturnSignature(IEnumerable<KeyValuePair<string, string>> queryParams, string? vnpSecureHash)
        {
            if (string.IsNullOrEmpty(vnpSecureHash)) return false;
            var hashSecret = _config["Vnpay:HashSecret"];
            if (string.IsNullOrEmpty(hashSecret)) return false;

            var dict = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var kv in queryParams)
                if (!string.IsNullOrEmpty(kv.Value)) dict[kv.Key] = kv.Value;

            var hashData = new StringBuilder();
            var first = true;
            foreach (var kv in dict)
            {
                if (first) first = false;
                else hashData.Append('&');
                hashData.Append(UrlEncodeForVnPay(kv.Key)).Append('=').Append(UrlEncodeForVnPay(kv.Value));
            }

            var computed = HmacSha512(hashSecret, hashData.ToString());
            return string.Equals(computed, vnpSecureHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
