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

        [HttpGet]
        [Route("Admin/Courses/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khóa học.";
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/Courses/Edit.cshtml", course);
        }

        [HttpPost]
        [Route("Admin/Courses/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] SaveCourseRequest form)
        {
            if (id != form.Id)
            {
                TempData["ErrorMessage"] = "ID không khớp.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(form.Title))
                ModelState.AddModelError("Title", "Tiêu đề là bắt buộc.");
            if (string.IsNullOrWhiteSpace(form.Topic))
                ModelState.AddModelError("Topic", "Chủ đề là bắt buộc.");
            
            if (!ModelState.IsValid)
            {
                var course = await _courseRepository.GetByIdAsync(id);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }
                return View("~/Views/Admin/Courses/Edit.cshtml", course);
            }

            try
            {
                var existingCourse = await _courseRepository.GetByIdAsync(id);
                if (existingCourse == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học.";
                    return RedirectToAction("Index");
                }

                existingCourse.Title = form.Title.Trim();
                existingCourse.Topic = form.Topic.Trim();
                existingCourse.Level = form.Level ?? "B2";
                existingCourse.Syllabus = string.IsNullOrWhiteSpace(form.Syllabus) ? null : form.Syllabus.Trim();
                existingCourse.TargetAudience = string.IsNullOrWhiteSpace(form.TargetAudience) ? null : form.TargetAudience.Trim();
                existingCourse.MarketingCopy = string.IsNullOrWhiteSpace(form.MarketingCopy) ? null : form.MarketingCopy.Trim();
                existingCourse.PriceUSD = form.PriceUSD;
                existingCourse.IsActive = form.IsActive;
                existingCourse.UpdatedAt = DateTime.UtcNow;

                await _courseRepository.UpdateAsync(existingCourse);
                TempData["SuccessMessage"] = "Cập nhật khóa học thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật khóa học");
                TempData["ErrorMessage"] = "Lỗi khi cập nhật. Vui lòng thử lại.";
                var course = await _courseRepository.GetByIdAsync(id);
                return View("~/Views/Admin/Courses/Edit.cshtml", course);
            }
        }

        [HttpPost]
        [Route("Admin/Courses/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _courseRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa khóa học thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa khóa học");
                TempData["ErrorMessage"] = "Không thể xóa khóa học. Có thể khóa học đang được sử dụng trong đơn hàng.";
            }

            return RedirectToAction("Index");
        }
    }

    public class GenerateCourseDesignRequest
    {
        public string Topic { get; set; } = "";
        public string? Level { get; set; } = "B2";
    }

    public class SaveCourseRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Topic { get; set; } = "";
        public string? Level { get; set; } = "B2";
        public string? Syllabus { get; set; }
        public string? TargetAudience { get; set; }
        public string? MarketingCopy { get; set; }
        public decimal PriceUSD { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

