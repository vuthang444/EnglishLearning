using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.Entities;
using System.Security.Claims;

namespace EnglishLearning.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IMoMoService _momo;
        private readonly IConfiguration _config;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourseRepository courseRepo, IOrderRepository orderRepo, IMoMoService momo, IConfiguration config, ILogger<CoursesController> logger)
        {
            _courseRepo = courseRepo;
            _orderRepo = orderRepo;
            _momo = momo;
            _config = config;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Courses")]
        [Route("Courses/Index")]
        public async Task<IActionResult> Index()
        {
            var list = await _courseRepo.GetActiveAsync();
            return View("~/Views/Courses/Index.cshtml", list);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Courses/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var c = await _courseRepo.GetByIdAsync(id);
            if (c == null) { TempData["ErrorMessage"] = "Không tìm thấy khóa học."; return RedirectToAction("Index"); }
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var paid = await _orderRepo.GetPaidByUserAndCourseAsync(userId.Value, id);
                ViewBag.AlreadyPurchased = paid.Any();
            }
            return View("~/Views/Courses/Detail.cshtml", c);
        }

        [Authorize]
        [HttpPost]
        [Route("Courses/Checkout/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int id, string paymentMethod = "payWithATM")
        {
            var userId = GetUserId();
            if (!userId.HasValue) { TempData["ErrorMessage"] = "Vui lòng đăng nhập."; return RedirectToAction("Index", "Home"); }

            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null) { TempData["ErrorMessage"] = "Khóa học không tồn tại."; return RedirectToAction("Index"); }

            var already = await _orderRepo.GetPaidByUserAndCourseAsync(userId.Value, id);
            if (already.Any()) { TempData["InfoMessage"] = "Bạn đã sở hữu khóa học này."; return RedirectToAction("MyCourses"); }

            var rate = _config.GetValue<decimal>("ExchangeRateUsdToVnd", 25000);
            var amountVnd = (long)Math.Round((double)(course.PriceUSD * rate));

            if (amountVnd <= 0) { TempData["ErrorMessage"] = "Khóa học chưa có giá."; return RedirectToAction("Detail", new { id }); }

            // Validate paymentMethod
            if (string.IsNullOrEmpty(paymentMethod) ||
                (paymentMethod != "payWithATM" && paymentMethod != "payWithCC" && paymentMethod != "captureWallet"))
            {
                paymentMethod = "payWithATM";
            }

            var order = new Order
            {
                UserId = userId.Value,
                CourseId = id,
                Amount = amountVnd,
                Status = "Pending"
            };
            order = await _orderRepo.CreateAsync(order);
            order.MomoOrderId = "EL" + order.Id;
            order.MomoRequestId = Guid.NewGuid().ToString();
            await _orderRepo.UpdateAsync(order);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = _config["MoMo:ReturnUrl"]?.Trim();
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = baseUrl + "/Payment/MoMoReturn";
            var ipnUrl = _config["MoMo:IpnUrl"]?.Trim();
            if (string.IsNullOrEmpty(ipnUrl)) ipnUrl = baseUrl + "/Payment/MoMoIpn";

            var orderInfo = "Thanh toan khoa hoc " + course.Title;
            var (ok, payUrl, msg) = await _momo.CreatePaymentAsync(order.MomoOrderId!, order.MomoRequestId!, amountVnd, orderInfo, returnUrl, ipnUrl, paymentMethod);

            if (ok && !string.IsNullOrEmpty(payUrl))
                return Redirect(payUrl);

            TempData["ErrorMessage"] = msg ?? "Không tạo được link thanh toán MoMo.";
            return RedirectToAction("Detail", new { id });
        }

        [Authorize]
        [HttpGet]
        [Route("Courses/MyCourses")]
        public async Task<IActionResult> MyCourses()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToAction("Index", "Home");
            var orders = await _orderRepo.GetByUserIdAsync(userId.Value);
            var paid = orders.Where(o => o.Status == "Paid").ToList();
            return View("~/Views/Courses/MyCourses.cshtml", paid);
        }

        private int? GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var v) ? v : null;
        }
    }
}

