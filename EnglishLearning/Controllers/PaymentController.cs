using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using EnglishLearning.Models;

namespace EnglishLearning.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IOrderRepository orderRepo, ILogger<PaymentController> logger)
        {
            _orderRepo = orderRepo;
            _logger = logger;
        }

        [HttpGet]
        [Route("Payment/MoMoReturn")]
        public async Task<IActionResult> MoMoReturn()
        {
            var orderId = Request.Query["orderId"].ToString();
            var resultCode = int.TryParse(Request.Query["resultCode"].ToString(), out var c) ? c : -1;
            var message = Request.Query["message"].ToString();

            if (string.IsNullOrEmpty(orderId))
            {
                return View("~/Views/Payment/Result.cshtml", new PaymentResultVm { Success = false, Message = "Thiếu tham số." });
            }

            var order = await _orderRepo.GetByMomoOrderIdAsync(orderId);
            if (order == null)
            {
                return View("~/Views/Payment/Result.cshtml", new PaymentResultVm { Success = false, Message = "Không tìm thấy đơn hàng." });
            }

            if (order.Status == "Paid")
            {
                return View("~/Views/Payment/Result.cshtml", new PaymentResultVm { Success = true, Message = "Bạn đã thanh toán khóa học này.", Order = order });
            }

            if (resultCode == 0 || resultCode == 9000)
            {
                order.Status = "Paid";
                order.MomoTransId = Request.Query["transId"].ToString();
                order.MomoResultCode = resultCode;
                order.MomoMessage = message;
                await _orderRepo.UpdateAsync(order);
                return View("~/Views/Payment/Result.cshtml", new PaymentResultVm { Success = true, Message = "Thanh toán thành công.", Order = order });
            }

            order.Status = "Failed";
            order.MomoResultCode = resultCode;
            order.MomoMessage = message;
            await _orderRepo.UpdateAsync(order);
            return View("~/Views/Payment/Result.cshtml", new PaymentResultVm { Success = false, Message = string.IsNullOrEmpty(message) ? "Thanh toán thất bại." : message, Order = order });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("Payment/MoMoIpn")]
        public async Task<IActionResult> MoMoIpn()
        {
            using var reader = new StreamReader(Request.Body);
            var raw = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(raw))
            {
                return Json(new { resultCode = 1, message = "No body" });
            }

            int resultCode = -1;
            string? orderId = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                var root = doc.RootElement;
                orderId = root.TryGetProperty("orderId", out var o) ? o.GetString() : null;
                resultCode = root.TryGetProperty("resultCode", out var r) ? r.GetInt32() : -1;
            }
            catch { return Json(new { resultCode = 1, message = "Invalid JSON" }); }

            var order = await _orderRepo.GetByMomoOrderIdAsync(orderId ?? "");
            if (order == null)
            {
                return Json(new { resultCode = 1, message = "Order not found" });
            }

            if (order.Status == "Paid")
            {
                return Json(new { resultCode = 0, message = "Success" });
            }

            if (resultCode == 0 || resultCode == 9000)
            {
                order.Status = "Paid";
                using var d = System.Text.Json.JsonDocument.Parse(raw);
                var r = d.RootElement;
                order.MomoTransId = r.TryGetProperty("transId", out var t) ? t.GetString() : null;
                order.MomoResultCode = resultCode;
                order.MomoMessage = r.TryGetProperty("message", out var m) ? m.GetString() : null;
                await _orderRepo.UpdateAsync(order);
            }
            else
            {
                order.Status = "Failed";
                using var d = System.Text.Json.JsonDocument.Parse(raw);
                order.MomoResultCode = resultCode;
                order.MomoMessage = d.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
                await _orderRepo.UpdateAsync(order);
            }

            return Json(new { resultCode = 0, message = "Success" });
        }
    }
}

