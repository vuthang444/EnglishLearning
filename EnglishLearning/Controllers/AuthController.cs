using Microsoft.AspNetCore.Mvc;
using CommonLib.DTOs;
using CommonLib.Interfaces;

namespace EnglishLearning.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.RegisterAsync(model);
                if (result == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authService.LoginAsync(model);
                if (result == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Lưu token vào cookie
                Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.ExpiresAt
                });

                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Phân biệt Admin và User
                if (result.User.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng nhập. Vui lòng thử lại.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login", "Auth");
        }
    }
}

