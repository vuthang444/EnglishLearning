namespace CommonLib.Interfaces
{
    public interface IMoMoService
    {
        /// <param name="paymentMethod">
        /// captureWallet (ví MoMo), payWithATM (thẻ ATM), payWithCC (thẻ tín dụng/ghi nợ)
        /// </param>
        Task<(bool ok, string? payUrl, string? message)> CreatePaymentAsync(
            string orderId,
            string requestId,
            long amountVnd,
            string orderInfo,
            string returnUrl,
            string ipnUrl,
            string paymentMethod = "captureWallet");
    }
}

