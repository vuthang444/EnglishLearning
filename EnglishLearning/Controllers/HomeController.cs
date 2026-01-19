using EnglishLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CommonLib.Interfaces;
using System.Security.Claims;

namespace EnglishLearning.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ILessonRepository _lessonRepository;
		private readonly ISubmissionRepository _submissionRepository;
		private readonly IUserRepository _userRepository;
		private readonly ICourseRepository _courseRepository;
		private readonly IOrderRepository _orderRepository;

		public HomeController(
			ILogger<HomeController> logger,
			ILessonRepository lessonRepository,
			ISubmissionRepository submissionRepository,
			IUserRepository userRepository,
			ICourseRepository courseRepository,
			IOrderRepository orderRepository)
		{
			_logger = logger;
			_lessonRepository = lessonRepository;
			_submissionRepository = submissionRepository;
			_userRepository = userRepository;
			_courseRepository = courseRepository;
			_orderRepository = orderRepository;
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

			// Lấy submissions của user
			var userSubmissions = await _submissionRepository.GetByUserIdAsync(userId) ?? new List<CommonLib.Entities.Submission>();
			
			// Lấy khóa học user đã mua (Paid orders)
			var userOrders = await _orderRepository.GetByUserIdAsync(userId);
			var paidOrders = userOrders.Where(o => o.Status == "Paid").ToList();
			var purchasedCourseIds = paidOrders.Select(o => o.CourseId).Distinct().ToList();
			
			// Lấy các khóa học đã mua
			var purchasedCourses = new List<CommonLib.Entities.Course>();
			if (purchasedCourseIds.Any())
			{
				var allCourses = await _courseRepository.GetAllAsync();
				purchasedCourses = allCourses.Where(c => purchasedCourseIds.Contains(c.Id)).ToList();
			}
			
			// Lấy lessons từ các khóa học đã mua hoặc free preview
			var allLessons = await _lessonRepository.GetAllAsync();
			var availableLessons = purchasedCourses.Any() 
				? allLessons.Where(l => l.IsActive).ToList() // Nếu đã mua khóa học thì xem được tất cả
				: allLessons.Where(l => l.IsActive && l.IsFreePreview).ToList(); // Nếu chưa mua thì chỉ xem Free Preview
			
			// Tìm bài học gần nhất (bài học user đã làm gần đây nhất hoặc bài học đầu tiên chưa làm)
			var latestSubmission = userSubmissions.OrderByDescending(s => s.CompletedAt ?? s.CreatedAt).FirstOrDefault();
			CommonLib.Entities.Lesson? latestLesson = null;
			int lessonNumber = 0;
			
			if (latestSubmission != null)
			{
				latestLesson = await _lessonRepository.GetByIdAsync(latestSubmission.LessonId);
				if (latestLesson != null)
				{
					// Tính số thứ tự của lesson trong cùng skill
					var sameSkillLessons = availableLessons
						.Where(l => l.SkillId == latestLesson.SkillId)
						.OrderBy(l => l.Order)
						.ThenBy(l => l.Id)
						.ToList();
					lessonNumber = sameSkillLessons.FindIndex(l => l.Id == latestLesson.Id) + 1;
					if (lessonNumber == 0) lessonNumber = 1;
				}
			}
			
			// Nếu chưa có bài học nào, lấy bài học đầu tiên theo thứ tự
			if (latestLesson == null && availableLessons.Any())
			{
				latestLesson = availableLessons.OrderBy(l => l.Order).ThenBy(l => l.Id).FirstOrDefault();
				if (latestLesson != null)
				{
					var sameSkillLessons = availableLessons
						.Where(l => l.SkillId == latestLesson.SkillId)
						.OrderBy(l => l.Order)
						.ThenBy(l => l.Id)
						.ToList();
					lessonNumber = sameSkillLessons.FindIndex(l => l.Id == latestLesson.Id) + 1;
					if (lessonNumber == 0) lessonNumber = 1;
				}
			}
			
			// Tính toán thống kê
			var totalDuration = userSubmissions.Sum(s => s.TimeSpentSeconds) / 3600.0; // Chuyển sang giờ
			var totalTests = userSubmissions.Count;
			var totalLessons = userSubmissions.Select(s => s.LessonId).Distinct().Count();
			var totalCups = userSubmissions.Count(s => s.Score >= 70); // Cúp khi điểm >= 70
			
			// Tính IELTS Level dựa trên điểm trung bình
			var averageScore = userSubmissions.Any() 
				? userSubmissions.Average(s => s.Score) 
				: 0;
			
			// Mapping điểm sang IELTS Level
			string ieltsLevel = "Entry";
			double progressPercentage = 0;
			if (averageScore >= 90) { ieltsLevel = "Target"; progressPercentage = 100; }
			else if (averageScore >= 80) { ieltsLevel = "Advanced"; progressPercentage = 85; }
			else if (averageScore >= 70) { ieltsLevel = "Intermediate"; progressPercentage = 70; }
			else if (averageScore >= 60) { ieltsLevel = "Beginner"; progressPercentage = 50; }
			else { ieltsLevel = "Entry"; progressPercentage = 25; }
			
			// Tính số ngày học liên tiếp (Streak)
			var submissionDates = userSubmissions
				.Where(s => s.CompletedAt.HasValue)
				.Select(s => s.CompletedAt!.Value.Date)
				.Distinct()
				.OrderByDescending(d => d)
				.ToList();
			
			int streak = 0;
			if (submissionDates.Any())
			{
				var today = DateTime.UtcNow.Date;
				var checkDate = submissionDates.Contains(today) ? today : today.AddDays(-1);
				
				for (int i = 0; i < submissionDates.Count; i++)
				{
					if (submissionDates[i] == checkDate)
					{
						streak++;
						checkDate = checkDate.AddDays(-1);
					}
					else if (submissionDates[i] < checkDate)
					{
						break;
					}
				}
			}
			
			// Lấy các bài làm gần đây (5 bài)
			var recentSubmissions = userSubmissions
				.OrderByDescending(s => s.CompletedAt ?? s.CreatedAt)
				.Take(5)
				.ToList();
			
			// Tạo dictionary mapping submission.LessonId -> Lesson cho Recent Activity
			var recentSubmissionLessons = new Dictionary<int, CommonLib.Entities.Lesson>();
			foreach (var submission in recentSubmissions)
			{
				if (!recentSubmissionLessons.ContainsKey(submission.LessonId))
				{
					var lesson = await _lessonRepository.GetByIdAsync(submission.LessonId);
					if (lesson != null)
					{
						recentSubmissionLessons[submission.LessonId] = lesson;
					}
				}
			}

			ViewBag.UserName = user.Username;
			ViewBag.LatestLesson = latestLesson;
			ViewBag.LatestSubmission = latestSubmission;
			ViewBag.LessonNumber = lessonNumber;
			ViewBag.TotalDuration = (int)totalDuration;
			ViewBag.TotalTests = totalTests;
			ViewBag.TotalLessons = totalLessons;
			ViewBag.TotalCups = totalCups;
			ViewBag.PurchasedCourses = purchasedCourses;
			ViewBag.IeltsLevel = ieltsLevel;
			ViewBag.ProgressPercentage = progressPercentage;
			ViewBag.Streak = streak;
			ViewBag.RecentSubmissions = recentSubmissions;
			ViewBag.AverageScore = Math.Round(averageScore, 1);
			ViewBag.IsPremium = await _orderRepository.HasActivePremiumAsync(userId);
			ViewBag.AvailableLessons = availableLessons;
			ViewBag.RecentSubmissionLessons = recentSubmissionLessons;

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
