using EnglishLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CommonLib.Interfaces;
using CommonLib.Entities;
using System.Security.Claims;

namespace EnglishLearning.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ILessonRepository _lessonRepository;
		private readonly ISubmissionRepository _submissionRepository;
		private readonly IUserRepository _userRepository;
		private readonly ISkillRepository _skillRepository;

		public HomeController(
			ILogger<HomeController> logger,
			ILessonRepository lessonRepository,
			ISubmissionRepository submissionRepository,
			IUserRepository userRepository,
			ISkillRepository skillRepository)
		{
			_logger = logger;
			_lessonRepository = lessonRepository;
			_submissionRepository = submissionRepository;
			_userRepository = userRepository;
			_skillRepository = skillRepository;
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

		[Authorize]
		public async Task<IActionResult> Dashboard()
		{
			// Nếu là Admin thì redirect đến Admin
			if (User.IsInRole("Admin"))
			{
				return RedirectToAction("Index", "Admin");
			}

			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
			{
				return RedirectToAction("Login", "Auth");
			}

			// Lấy thông tin user
			var user = await _userRepository.GetByIdAsync(userId);
			if (user == null)
			{
				return RedirectToAction("Login", "Auth");
			}

			// Lấy tất cả lessons
			var allLessons = await _lessonRepository.GetAllAsync();
			
			// Lấy bài học gần nhất (ưu tiên Reading, sau đó Listening)
			var readingSkill = await _skillRepository.GetByNameAsync("Reading");
			var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
			
			Lesson? latestLesson = null;
			if (readingSkill != null)
			{
				var readingLessons = allLessons.Where(l => l.SkillId == readingSkill.Id && l.IsActive).OrderBy(l => l.Order).ToList();
				latestLesson = readingLessons.FirstOrDefault();
			}
			
			// Nếu không có Reading, lấy Listening
			if (latestLesson == null && listeningSkill != null)
			{
				var listeningLessons = allLessons.Where(l => l.SkillId == listeningSkill.Id && l.IsActive).OrderBy(l => l.Order).ToList();
				latestLesson = listeningLessons.FirstOrDefault();
			}

			// Lấy submissions của user
			var userSubmissions = await _submissionRepository.GetByUserIdAsync(userId) ?? new List<CommonLib.Entities.Submission>();
			
			// Tính toán thống kê
			var totalDuration = userSubmissions.Sum(s => s.TimeSpentSeconds) / 3600.0; // Chuyển sang giờ
			var totalTests = userSubmissions.Count;
			var totalLessons = userSubmissions.Select(s => s.LessonId).Distinct().Count();
			var totalCups = userSubmissions.Count(s => s.Score > 0); // Giả sử mỗi bài làm đúng có cup

			// Tìm submission gần nhất
			var latestSubmission = userSubmissions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();

			ViewBag.UserName = user.Username;
			ViewBag.LatestLesson = latestLesson;
			ViewBag.LatestSubmission = latestSubmission;
			ViewBag.TotalDuration = (int)totalDuration;
			ViewBag.TotalTests = totalTests;
			ViewBag.TotalLessons = totalLessons;
			ViewBag.TotalCups = totalCups;
			ViewBag.AllLessons = allLessons ?? new List<CommonLib.Entities.Lesson>();

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
