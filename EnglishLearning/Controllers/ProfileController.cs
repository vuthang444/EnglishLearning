using CommonLib.Interfaces;
using EnglishLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            ILogger<ProfileController> logger)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpGet]
        [Route("Profile")]
        [Route("Profile/Index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var identityName = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(identityName))
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Thử tìm theo Username trước, nếu không có thì thử theo Email
                var user = await _userRepository.GetByUsernameAsync(identityName)
                           ?? await _userRepository.GetByEmailAsync(identityName);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản.";
                    return RedirectToAction("Index", "Home");
                }

                var orders = await _orderRepository.GetByUserIdAsync(user.Id);
                var paidOrders = orders.Where(o => o.Status == "Paid").ToList();

                var vm = new UserProfileViewModel
                {
                    User = user,
                    PaidOrders = paidOrders
                };

                return View("~/Views/Profile/Index.cshtml", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang hồ sơ người dùng");
                TempData["ErrorMessage"] = "Không tải được hồ sơ người dùng. Vui lòng thử lại.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}

