using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class LessonsController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            ILogger<LessonsController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? skillId)
        {
            ViewBag.Skills = await _skillRepository.GetAllAsync();
            
            List<Lesson> lessons;
            if (skillId.HasValue)
            {
                lessons = await _lessonRepository.GetBySkillIdAsync(skillId.Value);
                ViewBag.SelectedSkillId = skillId.Value;
            }
            else
            {
                lessons = await _lessonRepository.GetAllAsync();
            }

            return View(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Skills = await _skillRepository.GetAllAsync();
            return View(new LessonDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Skills = await _skillRepository.GetAllAsync();
                return View(dto);
            }

            try
            {
                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    SkillId = dto.SkillId,
                    Level = dto.Level,
                    Order = dto.Order,
                    IsActive = dto.IsActive
                };

                await _lessonRepository.CreateAsync(lesson);
                TempData["SuccessMessage"] = "Tạo bài học thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài học");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài học. Vui lòng thử lại.");
                ViewBag.Skills = await _skillRepository.GetAllAsync();
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.Skills = await _skillRepository.GetAllAsync();
            var dto = new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                SkillId = lesson.SkillId,
                Level = lesson.Level,
                Order = lesson.Order,
                IsActive = lesson.IsActive
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LessonDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Skills = await _skillRepository.GetAllAsync();
                return View(dto);
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
                lesson.SkillId = dto.SkillId;
                lesson.Level = dto.Level;
                lesson.Order = dto.Order;
                lesson.IsActive = dto.IsActive;

                await _lessonRepository.UpdateAsync(lesson);
                TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài học");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài học. Vui lòng thử lại.");
                ViewBag.Skills = await _skillRepository.GetAllAsync();
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _lessonRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa bài học thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài học để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài học");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bài học.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

