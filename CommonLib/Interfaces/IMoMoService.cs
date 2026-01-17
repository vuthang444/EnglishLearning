namespace CommonLib.Interfaces
{
    public interface IMoMoService
    {
        Task<(bool ok, string? payUrl, string? message)> CreatePaymentAsync(string orderId, string requestId, long amountVnd, string orderInfo, string returnUrl, string ipnUrl);
    }
}

