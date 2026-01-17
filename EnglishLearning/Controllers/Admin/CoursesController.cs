using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ICourseRepository courseRepository, IOpenAIService openAIService, ILogger<CoursesController> logger)
        {
            _courseRepository = courseRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpGet]
        [Route("Admin/Courses")]
        [Route("Admin/Courses/Index")]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAllAsync();
            return View("~/Views/Admin/Courses/Index.cshtml", courses);
        }

        [HttpGet]
        [Route("Admin/Courses/Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Courses/Create.cshtml");
        }

        [HttpPost]
        [Route("Admin/Courses/GenerateDesign")]
        public async Task<IActionResult> GenerateDesign([FromBody] GenerateCourseDesignRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Topic))
                    return Json(new { success = false, message = "Chủ đề không được để trống." });
                var result = await _openAIService.GenerateCourseDesignAsync(req.Topic.Trim(), req.Level ?? "B2");
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi AI GenerateCourseDesign");
                return Json(new { success = false, message = "Lỗi khi tạo thiết kế. Vui lòng thử lại." });
            }
        }

        [HttpPost]
        [Route("Admin/Courses/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] SaveCourseRequest form)
        {
            if (string.IsNullOrWhiteSpace(form.Title))
                ModelState.AddModelError("Title", "Tiêu đề là bắt buộc.");
            if (string.IsNullOrWhiteSpace(form.Topic))
                ModelState.AddModelError("Topic", "Chủ đề là bắt buộc.");
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Courses/Create.cshtml");
            }
            try
            {
                var course = new Course
                {
                    Title = form.Title.Trim(),
                    Topic = form.Topic.Trim(),
                    Level = form.Level ?? "B2",
                    Syllabus = string.IsNullOrWhiteSpace(form.Syllabus) ? null : form.Syllabus.Trim(),
                    TargetAudience = string.IsNullOrWhiteSpace(form.TargetAudience) ? null : form.TargetAudience.Trim(),
                    MarketingCopy = string.IsNullOrWhiteSpace(form.MarketingCopy) ? null : form.MarketingCopy.Trim(),
                    PriceUSD = form.PriceUSD,
                    IsActive = true
                };
                await _courseRepository.CreateAsync(course);
                TempData["SuccessMessage"] = "Đã lưu khóa học thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu khóa học");
                TempData["ErrorMessage"] = "Lỗi khi lưu. Vui lòng thử lại.";
                return View("~/Views/Admin/Courses/Create.cshtml");
            }
        }
    }

    public class GenerateCourseDesignRequest
    {
        public string Topic { get; set; } = "";
        public string? Level { get; set; } = "B2";
    }

    public class SaveCourseRequest
    {
        public string Title { get; set; } = "";
        public string Topic { get; set; } = "";
        public string? Level { get; set; } = "B2";
        public string? Syllabus { get; set; }
        public string? TargetAudience { get; set; }
        public string? MarketingCopy { get; set; }
        public decimal PriceUSD { get; set; }
    }
}

