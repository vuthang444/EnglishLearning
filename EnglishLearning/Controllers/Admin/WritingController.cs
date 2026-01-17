using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;
using System.Linq;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class WritingController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<WritingController> _logger;

        public WritingController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            IOpenAIService openAIService,
            ILogger<WritingController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        // Trang chủ Writing - Dashboard
        [Route("Admin/Writing")]
        [Route("Admin/Writing/Index")]
        public async Task<IActionResult> Index()
        {
            var writingSkill = await _skillRepository.GetByNameAsync("Writing");
            if (writingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Writing";
                return RedirectToAction("Index", "Admin");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(writingSkill.Id);
            
            // Thống kê
            ViewBag.TotalLessons = lessons.Count;
            ViewBag.ActiveLessons = lessons.Count(l => l.IsActive);

            ViewBag.Skill = writingSkill;
            return View("~/Views/Admin/Writing/Index.cshtml", lessons);
        }

        // Xem chi tiết bài viết
        [HttpGet]
        [Route("Admin/Writing/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tập";
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/Writing/Detail.cshtml", lesson);
        }

        // Tạo bài viết mới - GET
        [HttpGet]
        [Route("Admin/Writing/Create")]
        public async Task<IActionResult> Create()
        {
            var writingSkill = await _skillRepository.GetByNameAsync("Writing");
            if (writingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Writing";
                return RedirectToAction("Index", "Admin");
            }

            // Tạo DTO với SkillId đã được set
            var dto = new WritingPassageDto
            {
                SkillId = writingSkill.Id,
                WritingLevel = "B2",
                WordLimit = 250,
                TimeLimitMinutes = 40,
                IsActive = true
            };

            ViewBag.SkillId = writingSkill.Id;
            ViewBag.Skill = writingSkill;
            return View("~/Views/Admin/Writing/Create.cshtml", dto);
        }

        // Tạo bài viết mới - POST
        [HttpPost]
        [Route("Admin/Writing/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] WritingPassageDto dto)
        {
            var writingSkill = await _skillRepository.GetByNameAsync("Writing");
            if (writingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Writing";
                return RedirectToAction("Index", "Admin");
            }

            // Đọc SkillId từ form, nếu không có hoặc = 0 thì dùng writingSkill.Id
            var skillIdStr = Request.Form["SkillId"].ToString();
            int skillId = 0;
            if (!string.IsNullOrWhiteSpace(skillIdStr) && int.TryParse(skillIdStr, out int parsedSkillId) && parsedSkillId > 0)
            {
                skillId = parsedSkillId;
            }
            else
            {
                skillId = writingSkill.Id;
            }

            // Tạo DTO mới từ form data để tránh lỗi binding
            var formDto = new WritingPassageDto
            {
                Title = Request.Form["Title"].ToString().Trim(),
                Description = Request.Form["Description"].ToString().Trim(),
                WritingTopic = Request.Form["WritingTopic"].ToString().Trim(),
                WritingPrompt = Request.Form["WritingPrompt"].ToString().Trim(),
                WritingHints = Request.Form["WritingHints"].ToString().Trim(),
                WritingLevel = Request.Form["WritingLevel"].ToString().Trim(),
                SkillId = skillId, // Đảm bảo SkillId luôn > 0
                IsActive = Request.Form["IsActive"].ToString() == "true" || Request.Form["IsActive"].ToString() == "on"
            };

            // Xử lý WordLimit từ form (tránh lỗi binding với empty string)
            var wordLimitStr = Request.Form["WordLimit"].ToString();
            if (string.IsNullOrWhiteSpace(wordLimitStr) || !int.TryParse(wordLimitStr, out int wordLimit) || wordLimit <= 0)
            {
                formDto.WordLimit = 250;
            }
            else
            {
                if (wordLimit < 50 || wordLimit > 1000)
                {
                    ModelState.AddModelError("WordLimit", "Giới hạn từ phải từ 50 đến 1000");
                }
                else
                {
                    formDto.WordLimit = wordLimit;
                }
            }

            // Xử lý TimeLimitMinutes từ form
            var timeLimitStr = Request.Form["TimeLimitMinutes"].ToString();
            if (string.IsNullOrWhiteSpace(timeLimitStr) || !int.TryParse(timeLimitStr, out int timeLimit) || timeLimit <= 0)
            {
                formDto.TimeLimitMinutes = 40;
            }
            else
            {
                if (timeLimit < 5 || timeLimit > 180)
                {
                    ModelState.AddModelError("TimeLimitMinutes", "Thời gian giới hạn phải từ 5 đến 180 phút");
                }
                else
                {
                    formDto.TimeLimitMinutes = timeLimit;
                }
            }

            if (string.IsNullOrWhiteSpace(formDto.WritingLevel))
            {
                formDto.WritingLevel = "B2";
            }

            // Xóa tất cả lỗi binding/formatting cho tất cả các fields
            // (lỗi "The value "" is invalid" thường xảy ra khi model binding fail)
            foreach (var key in ModelState.Keys.ToList())
            {
                if (ModelState[key] != null && ModelState[key].Errors.Count > 0)
                {
                    var errorsToRemove = ModelState[key].Errors
                        .Where(e => e.ErrorMessage.Contains("invalid") || 
                                    e.ErrorMessage.Contains("The value") ||
                                    e.ErrorMessage.Contains("format") ||
                                    (e.ErrorMessage == "" && e.Exception != null))
                        .ToList();
                    foreach (var error in errorsToRemove)
                    {
                        ModelState[key].Errors.Remove(error);
                    }
                }
            }

            // Validate SkillId
            if (formDto.SkillId <= 0)
            {
                _logger.LogWarning("SkillId không hợp lệ: {SkillId}, sử dụng writingSkill.Id: {WritingSkillId}", 
                    formDto.SkillId, writingSkill.Id);
                formDto.SkillId = writingSkill.Id;
            }

            // Validate các field required
            if (string.IsNullOrWhiteSpace(formDto.Title))
            {
                ModelState.AddModelError("Title", "Tiêu đề là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(formDto.WritingTopic))
            {
                ModelState.AddModelError("WritingTopic", "Chủ đề là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(formDto.WritingPrompt))
            {
                ModelState.AddModelError("WritingPrompt", "Đề bài là bắt buộc");
            }

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning("ModelState không hợp lệ. Số lỗi: {Count}, Chi tiết: {Errors}", 
                    allErrors.Count, string.Join("; ", allErrors));
                
                ViewBag.SkillId = writingSkill.Id;
                ViewBag.Skill = writingSkill;
                return View("~/Views/Admin/Writing/Create.cshtml", formDto);
            }

            try
            {
                // Lấy số lượng bài tập hiện có để set Order
                var existingLessons = await _lessonRepository.GetBySkillIdAsync(writingSkill.Id);
                var maxOrder = existingLessons.Any() ? existingLessons.Max(l => l.Order) : 0;

                var writingLevel = formDto.WritingLevel?.Trim() ?? "B2";
                
                var lesson = new Lesson
                {
                    Title = formDto.Title?.Trim() ?? "",
                    Description = formDto.Description?.Trim(),
                    WritingTopic = formDto.WritingTopic?.Trim() ?? "",
                    WritingPrompt = formDto.WritingPrompt?.Trim() ?? "",
                    WritingHints = formDto.WritingHints?.Trim(),
                    WritingLevel = writingLevel,
                    WordLimit = formDto.WordLimit ?? 250,
                    TimeLimitMinutes = formDto.TimeLimitMinutes ?? 40,
                    SkillId = formDto.SkillId,
                    Level = GetLevelFromString(writingLevel),
                    Order = maxOrder + 1,
                    IsActive = formDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _lessonRepository.CreateAsync(lesson);
                TempData["SuccessMessage"] = "Tạo bài viết thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài viết: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tạo bài viết: {ex.Message}";
                
                ViewBag.SkillId = writingSkill.Id;
                ViewBag.Skill = writingSkill;
                return View("~/Views/Admin/Writing/Create.cshtml", formDto);
            }
        }

        // Tạo đề bài tự động bằng AI - POST
        [HttpPost]
        [Route("Admin/Writing/GenerateContent")]
        public async Task<IActionResult> GenerateContent([FromBody] GenerateWritingContentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Topic) || string.IsNullOrEmpty(request.Level))
                {
                    return Json(new { success = false, message = "Chủ đề và cấp độ là bắt buộc" });
                }

                var result = await _openAIService.GenerateWritingPromptAsync(request.Topic, request.Level);

                return Json(new
                {
                    success = true,
                    title = result.Title,
                    prompt = result.Prompt,
                    hints = result.Hints
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đề bài Writing bằng AI");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo đề bài. Vui lòng thử lại." });
            }
        }

        // Sửa bài viết - GET
        [HttpGet]
        [Route("Admin/Writing/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tập";
                return RedirectToAction("Index");
            }

            var dto = new WritingPassageDto
            {
                Title = lesson.Title,
                Description = lesson.Description,
                WritingTopic = lesson.WritingTopic ?? "",
                WritingPrompt = lesson.WritingPrompt ?? "",
                WritingHints = lesson.WritingHints,
                WritingLevel = lesson.WritingLevel ?? "B2",
                WordLimit = lesson.WordLimit,
                TimeLimitMinutes = lesson.TimeLimitMinutes,
                SkillId = lesson.SkillId,
                IsActive = lesson.IsActive
            };

            ViewBag.Skill = lesson.Skill;
            return View("~/Views/Admin/Writing/Edit.cshtml", dto);
        }

        // Sửa bài viết - POST
        [HttpPost]
        [Route("Admin/Writing/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WritingPassageDto dto)
        {
            if (!ModelState.IsValid)
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                ViewBag.Skill = lesson?.Skill;
                return View("~/Views/Admin/Writing/Edit.cshtml", dto);
            }

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài tập";
                    return RedirectToAction("Index");
                }

                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                lesson.WritingTopic = dto.WritingTopic;
                lesson.WritingPrompt = dto.WritingPrompt;
                lesson.WritingHints = dto.WritingHints;
                lesson.WritingLevel = dto.WritingLevel;
                lesson.WordLimit = dto.WordLimit;
                lesson.TimeLimitMinutes = dto.TimeLimitMinutes;
                lesson.IsActive = dto.IsActive;
                lesson.Level = GetLevelFromString(dto.WritingLevel);
                lesson.UpdatedAt = DateTime.UtcNow;

                await _lessonRepository.UpdateAsync(lesson);
                TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài viết");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật bài viết";
                
                var lesson = await _lessonRepository.GetByIdAsync(id);
                ViewBag.Skill = lesson?.Skill;
                return View("~/Views/Admin/Writing/Edit.cshtml", dto);
            }
        }

        // Xóa bài viết
        [HttpPost]
        [Route("Admin/Writing/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài tập" });
                }

                await _lessonRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Xóa bài viết thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài viết");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa bài viết" });
            }
        }

        // Helper method
        private int GetLevelFromString(string? level)
        {
            return level?.ToUpper() switch
            {
                "A1" or "A2" => 1,
                "B1" or "B2" => 2,
                "C1" or "C2" => 3,
                _ => 2
            };
        }
    }

    public class GenerateWritingContentRequest
    {
        public string Topic { get; set; } = string.Empty;
        public string Level { get; set; } = "B2";
    }
}


