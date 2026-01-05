using EnglishLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EnglishLearning.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			// Nếu là Admin thì redirect đến Admin
			if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
			{
				return RedirectToAction("Index", "Admin");
			}
			// Nếu chưa đăng nhập thì redirect đến trang đăng nhập
			if (User.Identity?.IsAuthenticated != true)
			{
				return RedirectToAction("Login", "Auth");
			}
			// User thì hiển thị trang chủ
			return View();
		}

		public IActionResult Privacy()
		{
			// Nếu là Admin thì redirect đến Admin
			if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
			{
				return RedirectToAction("Index", "Admin");
			}
			// User thì hiển thị trang Privacy
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
