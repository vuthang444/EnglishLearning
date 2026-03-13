namespace CommonLib.Interfaces
{
    public interface IVnPayService
    {
        /// <summary>
        /// Tạo URL thanh toán VNPay (redirect).
        /// </summary>
        string? CreatePaymentUrl(string orderRef, long amountVnd, string orderInfo, string returnUrl, string clientIp);

        /// <summary>
        /// Kiểm tra chữ ký trả về từ VNPay. queryParams: các cặp key-value (không gồm vnp_SecureHash).
        /// </summary>
        bool ValidateReturnSignature(IEnumerable<KeyValuePair<string, string>> queryParams, string? vnpSecureHash);
    }
}
