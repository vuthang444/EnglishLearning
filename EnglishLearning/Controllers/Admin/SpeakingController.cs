using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class SpeakingController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<SpeakingController> _logger;

        public SpeakingController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            IOpenAIService openAIService,
            ILogger<SpeakingController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        // Trang chủ Speaking - Dashboard
        [Route("Admin/Speaking")]
        [Route("Admin/Speaking/Index")]
        public async Task<IActionResult> Index()
        {
            var speakingSkill = await _skillRepository.GetByNameAsync("Speaking");
            if (speakingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Speaking";
                return RedirectToAction("Index", "Admin");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(speakingSkill.Id);
            
            // Thống kê
            ViewBag.TotalLessons = lessons.Count;
            ViewBag.ActiveLessons = lessons.Count(l => l.IsActive);

            ViewBag.Skill = speakingSkill;
            return View("~/Views/Admin/Speaking/Index.cshtml", lessons);
        }

        // Tạo bài nói mới
        [HttpGet]
        [Route("Admin/Speaking/Create")]
        public async Task<IActionResult> Create()
        {
            var speakingSkill = await _skillRepository.GetByNameAsync("Speaking");
            if (speakingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Speaking";
                return RedirectToAction("Index");
            }

            ViewBag.Skill = speakingSkill;
            ViewBag.DifficultyLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
            var dto = new SpeakingPassageDto 
            { 
                SkillId = speakingSkill.Id,
                Level = 1,
                IsActive = true,
                DifficultyLevel = "A1",
                TimeLimitSeconds = 60
            };
            return View("~/Views/Admin/Speaking/Create.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Speaking/Create")]
        public async Task<IActionResult> Create(SpeakingPassageDto dto)
        {
            var speakingSkill = await _skillRepository.GetByNameAsync("Speaking");
            if (speakingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Speaking";
                return RedirectToAction("Index");
            }

            dto.SkillId = speakingSkill.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.Skill = speakingSkill;
                ViewBag.DifficultyLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
                return View("~/Views/Admin/Speaking/Create.cshtml", dto);
            }

            try
            {
                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Topic = dto.Topic,
                    ReferenceText = dto.ReferenceText,
                    SpeakingLevel = dto.DifficultyLevel,
                    TimeLimitSeconds = dto.TimeLimitSeconds,
                    SkillId = dto.SkillId,
                    Level = dto.Level,
                    Order = dto.Order,
                    IsActive = dto.IsActive
                };

                await _lessonRepository.CreateAsync(lesson);

                TempData["SuccessMessage"] = "Tạo bài nói thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài nói");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài nói. Vui lòng thử lại.");
                ViewBag.Skill = speakingSkill;
                ViewBag.DifficultyLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
                return View("~/Views/Admin/Speaking/Create.cshtml", dto);
            }
        }

        // Tạo nội dung tự động bằng AI
        [HttpPost]
        [Route("Admin/Speaking/GenerateContent")]
        public async Task<IActionResult> GenerateContent([FromBody] GenerateContentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Topic))
                {
                    return Json(new { success = false, message = "Chủ đề không được để trống" });
                }

                var result = await _openAIService.GenerateSpeakingContentAsync(request.Topic, request.Level ?? "A1");

                return Json(new { 
                    success = true, 
                    title = result.Title,
                    passage = result.Passage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung tự động");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo nội dung. Vui lòng thử lại." });
            }
        }

        // Sửa bài nói
        [HttpGet]
        [Route("Admin/Speaking/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            if (lesson.Skill?.Name != "Speaking")
            {
                TempData["ErrorMessage"] = "Bài học không thuộc kỹ năng Speaking";
                return RedirectToAction("Index");
            }

            ViewBag.Skill = lesson.Skill;
            ViewBag.DifficultyLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
            var dto = new SpeakingPassageDto
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Topic = lesson.Topic ?? "",
                ReferenceText = lesson.ReferenceText,
                DifficultyLevel = lesson.SpeakingLevel,
                TimeLimitSeconds = lesson.TimeLimitSeconds,
                SkillId = lesson.SkillId,
                Level = lesson.Level,
                Order = lesson.Order,
                IsActive = lesson.IsActive
            };

            return View("~/Views/Admin/Speaking/Edit.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Speaking/Edit/{id}")]
        public async Task<IActionResult> Edit(int id, SpeakingPassageDto dto)
        {
            var speakingSkill = await _skillRepository.GetByNameAsync("Speaking");
            if (speakingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Speaking";
                return RedirectToAction("Index");
            }

            dto.SkillId = speakingSkill.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.Skill = speakingSkill;
                ViewBag.DifficultyLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
                return View("~/Views/Admin/Speaking/Edit.cshtml", dto);
            }

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    return NotFound();
                }

                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                lesson.Topic = dto.Topic;
                lesson.ReferenceText = dto.ReferenceText;
                lesson.SpeakingLevel = dto.DifficultyLevel;
                lesson.TimeLimitSeconds = dto.TimeLimitSeconds;
                lesson.Level = dto.Level;
                lesson.Order = dto.Order;
                lesson.IsActive = dto.IsActive;
                lesson.UpdatedAt = DateTime.UtcNow;

                await _lessonRepository.UpdateAsync(lesson);
                TempData["SuccessMessage"] = "Cập nhật bài nói thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài nói");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài nói. Vui lòng thử lại.");
                ViewBag.Skill = speakingSkill;
                ViewBag.DifficultyLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
                return View("~/Views/Admin/Speaking/Edit.cshtml", dto);
            }
        }

        // Xóa bài nói
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Speaking/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài học";
                    return RedirectToAction("Index");
                }

                if (lesson.Skill?.Name != "Speaking")
                {
                    TempData["ErrorMessage"] = "Bài học không thuộc kỹ năng Speaking";
                    return RedirectToAction("Index");
                }

                var result = await _lessonRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa bài nói thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa bài nói";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài nói");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bài nói.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class GenerateContentRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string? Level { get; set; }
    }
}

