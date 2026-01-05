using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class ReadingController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogger<ReadingController> _logger;

        public ReadingController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            IExerciseRepository exerciseRepository,
            ILogger<ReadingController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _exerciseRepository = exerciseRepository;
            _logger = logger;
        }

        // Trang chủ Reading - Dashboard
        [Route("Admin/Reading")]
        [Route("Admin/Reading/Index")]
        public async Task<IActionResult> Index()
        {
            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Index", "Admin");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(readingSkill.Id);
            
            // Thống kê
            ViewBag.TotalLessons = lessons.Count;
            ViewBag.ActiveLessons = lessons.Count(l => l.IsActive);
            ViewBag.TotalExercises = 0;
            foreach (var lesson in lessons)
            {
                var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
                ViewBag.TotalExercises = (int)ViewBag.TotalExercises + exercises.Count;
            }

            ViewBag.Skill = readingSkill;
            return View("~/Views/Admin/Reading/Index.cshtml", lessons);
        }

        // Xem chi tiết bài học
        [Route("Admin/Reading/LessonDetail/{id}")]
        public async Task<IActionResult> LessonDetail(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            // Kiểm tra phải là Reading skill
            if (lesson.Skill?.Name != "Reading")
            {
                TempData["ErrorMessage"] = "Bài học không thuộc kỹ năng Reading";
                return RedirectToAction("Index");
            }

            var exercises = await _exerciseRepository.GetByLessonIdAsync(id);
            ViewBag.Exercises = exercises;
            ViewBag.ExerciseCount = exercises.Count;

            return View("~/Views/Admin/Reading/LessonDetail.cshtml", lesson);
        }

        // Tạo bài đọc mới với câu hỏi
        [HttpGet]
        [Route("Admin/Reading/CreateReadingPassage")]
        public async Task<IActionResult> CreateReadingPassage()
        {
            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Index");
            }

            ViewBag.Skill = readingSkill;
            ViewBag.ReadingLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
            var dto = new ReadingPassageDto 
            { 
                SkillId = readingSkill.Id,
                Level = 1,
                IsActive = true,
                ReadingLevel = "A1"
            };
            return View("~/Views/Admin/Reading/CreateReadingPassage.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Reading/CreateReadingPassage")]
        public async Task<IActionResult> CreateReadingPassage(ReadingPassageDto dto)
        {
            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Index");
            }

            dto.SkillId = readingSkill.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.Skill = readingSkill;
                ViewBag.ReadingLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
                return View("~/Views/Admin/Reading/CreateReadingPassage.cshtml", dto);
            }

            try
            {
                // Tạo Lesson với nội dung bài đọc
                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    ReadingContent = dto.ReadingContent,
                    ReadingLevel = dto.ReadingLevel,
                    SkillId = dto.SkillId,
                    Level = dto.Level,
                    Order = dto.Order,
                    IsActive = dto.IsActive
                };

                var createdLesson = await _lessonRepository.CreateAsync(lesson);

                // Tạo các câu hỏi trắc nghiệm
                if (dto.Questions != null && dto.Questions.Any())
                {
                    int questionOrder = 1;
                    foreach (var questionDto in dto.Questions)
                    {
                        var exercise = new Exercise
                        {
                            Question = questionDto.Question,
                            LessonId = createdLesson.Id,
                            Type = CommonLib.Entities.ExerciseType.MultipleChoice,
                            Order = questionOrder++,
                            IsActive = true
                        };

                        // Tạo các phương án trả lời
                        exercise.Answers = new List<Answer>
                        {
                            new Answer { Text = questionDto.OptionA, IsCorrect = questionDto.CorrectAnswer == "A", Order = 1 },
                            new Answer { Text = questionDto.OptionB, IsCorrect = questionDto.CorrectAnswer == "B", Order = 2 },
                            new Answer { Text = questionDto.OptionC, IsCorrect = questionDto.CorrectAnswer == "C", Order = 3 },
                            new Answer { Text = questionDto.OptionD, IsCorrect = questionDto.CorrectAnswer == "D", Order = 4 }
                        };

                        await _exerciseRepository.CreateAsync(exercise);
                    }
                }

                TempData["SuccessMessage"] = "Tạo bài đọc với câu hỏi thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài đọc");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài đọc. Vui lòng thử lại.");
                ViewBag.Skill = readingSkill;
                ViewBag.ReadingLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
                return View("~/Views/Admin/Reading/CreateReadingPassage.cshtml", dto);
            }
        }

        // Tạo bài học mới cho Reading (giữ lại để tương thích)
        [HttpGet]
        [Route("Admin/Reading/CreateLesson")]
        public async Task<IActionResult> CreateLesson()
        {
            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Index");
            }

            ViewBag.Skill = readingSkill;
            ViewBag.ReadingLevels = new List<string> { "A1", "A2", "B1", "B2", "C1", "C2" };
            var dto = new LessonDto 
            { 
                SkillId = readingSkill.Id,
                Level = 1,
                IsActive = true
            };
            return View("~/Views/Admin/Reading/CreateLesson.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Reading/CreateLesson")]
        public async Task<IActionResult> CreateLesson(LessonDto dto)
        {
            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Index");
            }

            // Đảm bảo SkillId là Reading
            dto.SkillId = readingSkill.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.Skill = readingSkill;
                return View("~/Views/Admin/Reading/CreateLesson.cshtml", dto);
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
                TempData["SuccessMessage"] = "Tạo bài học Reading thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài học Reading");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài học. Vui lòng thử lại.");
                ViewBag.Skill = readingSkill;
                return View("~/Views/Admin/Reading/CreateLesson.cshtml", dto);
            }
        }

        // Sửa bài học
        [HttpGet]
        [Route("Admin/Reading/EditLesson/{id}")]
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            if (lesson.Skill?.Name != "Reading")
            {
                TempData["ErrorMessage"] = "Bài học không thuộc kỹ năng Reading";
                return RedirectToAction("Index");
            }

            ViewBag.Skill = lesson.Skill;
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

            return View("~/Views/Admin/Reading/EditLesson.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Reading/EditLesson/{id}")]
        public async Task<IActionResult> EditLesson(int id, LessonDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Index");
            }

            dto.SkillId = readingSkill.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.Skill = readingSkill;
                return View("~/Views/Admin/Reading/EditLesson.cshtml", dto);
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
                lesson.ReadingContent = dto.ReadingContent;
                lesson.ReadingLevel = dto.ReadingLevel;
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
                ViewBag.Skill = readingSkill;
                return View("~/Views/Admin/Reading/EditLesson.cshtml", dto);
            }
        }

        // Xóa bài học
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Reading/DeleteLesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);
                if (lesson == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài học";
                    return RedirectToAction("Index");
                }

                if (lesson.Skill?.Name != "Reading")
                {
                    TempData["ErrorMessage"] = "Bài học không thuộc kỹ năng Reading";
                    return RedirectToAction("Index");
                }

                var result = await _lessonRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa bài học thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa bài học";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài học");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bài học.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Quản lý bài tập của bài học
        [Route("Admin/Reading/Exercises/{lessonId}")]
        public async Task<IActionResult> Exercises(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            if (lesson.Skill?.Name != "Reading")
            {
                TempData["ErrorMessage"] = "Bài học không thuộc kỹ năng Reading";
                return RedirectToAction("Index");
            }

            var exercises = await _exerciseRepository.GetByLessonIdAsync(lessonId);
            ViewBag.Lesson = lesson;
            return View("~/Views/Admin/Reading/Exercises.cshtml", exercises);
        }
    }
}

