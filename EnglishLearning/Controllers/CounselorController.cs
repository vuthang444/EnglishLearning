using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using System.Security.Claims;

namespace EnglishLearning.Controllers
{
    [Authorize]
    public class CounselorController : Controller
    {
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<CounselorController> _logger;

        public CounselorController(
            ISubmissionRepository submissionRepository,
            ICourseRepository courseRepository,
            IOpenAIService openAIService,
            ILogger<CounselorController> logger)
        {
            _submissionRepository = submissionRepository;
            _courseRepository = courseRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Counselor")]
        [Route("Counselor/Index")]
        public async Task<IActionResult> Index()
        {
            var (speaking, writing) = await GetUserScoresAsync();
            ViewBag.SpeakingScore = speaking;
            ViewBag.WritingScore = writing;
            ViewBag.Courses = await _courseRepository.GetActiveAsync();
            ViewBag.Recommendation = (string?)null;
            return View("~/Views/Counselor/Index.cshtml");
        }

        [HttpPost]
        [Route("Counselor/Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] string? userGoal)
        {
            var (speaking, writing) = await GetUserScoresAsync();
            ViewBag.SpeakingScore = speaking;
            ViewBag.WritingScore = writing;
            var courses = await _courseRepository.GetActiveAsync();
            ViewBag.Courses = courses;

            var goal = string.IsNullOrWhiteSpace(userGoal) ? "Cải thiện tiếng Anh tổng quát" : userGoal.Trim();
            ViewBag.UserGoal = goal;
            var courseListText = string.Join("\n", courses.Select(c => $"- {c.Title} ({c.Topic}, {c.Level})"));

            if (string.IsNullOrEmpty(courseListText))
            {
                ViewBag.Recommendation = "Hiện chưa có khóa học nào trong hệ thống. Admin cần tạo khóa học trước để AI tư vấn.";
            }
            else
            {
                try
                {
                    var rec = await _openAIService.GetCourseRecommendationsAsync(speaking, writing, goal, courseListText);
                    ViewBag.Recommendation = rec;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi AI Counselor");
                    ViewBag.Recommendation = "Đã xảy ra lỗi khi tạo tư vấn. Vui lòng thử lại.";
                }
            }
            return View("~/Views/Counselor/Index.cshtml");
        }

        private async Task<(int speaking, int writing)> GetUserScoresAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return (0, 0);

            var subs = await _submissionRepository.GetByUserIdAsync(userId);
            double speakSum = 0, speakCount = 0, writeSum = 0, writeCount = 0;
            foreach (var s in subs)
            {
                var name = s.Lesson?.Skill?.Name ?? "";
                if (s.MaxScore <= 0) continue;
                var pct = (double)s.Score / s.MaxScore * 100.0;
                if (string.Equals(name, "Speaking", StringComparison.OrdinalIgnoreCase)) { speakSum += pct; speakCount++; }
                else if (string.Equals(name, "Writing", StringComparison.OrdinalIgnoreCase)) { writeSum += pct; writeCount++; }
            }
            int speak = speakCount > 0 ? (int)Math.Round(speakSum / speakCount) : 0;
            int write = writeCount > 0 ? (int)Math.Round(writeSum / writeCount) : 0;
            return (speak, write);
        }
    }
}

