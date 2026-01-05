using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class ExercisesController : Controller
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILogger<ExercisesController> _logger;

        public ExercisesController(
            IExerciseRepository exerciseRepository,
            ILessonRepository lessonRepository,
            ILogger<ExercisesController> logger)
        {
            _exerciseRepository = exerciseRepository;
            _lessonRepository = lessonRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.Lesson = lesson;
            var exercises = await _exerciseRepository.GetByLessonIdAsync(lessonId);
            return View(exercises);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.Lesson = lesson;
            var dto = new ExerciseDto { LessonId = lessonId };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExerciseDto dto)
        {
            if (!ModelState.IsValid)
            {
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                ViewBag.Lesson = lesson;
                return View(dto);
            }

            try
            {
                var exercise = new Exercise
                {
                    Question = dto.Question,
                    Content = dto.Content,
                    LessonId = dto.LessonId,
                    Type = dto.Type,
                    Order = dto.Order,
                    IsActive = dto.IsActive
                };

                // Thêm các câu trả lời
                if (dto.Answers != null && dto.Answers.Any())
                {
                    exercise.Answers = dto.Answers.Select((a, index) => new Answer
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        Order = a.Order > 0 ? a.Order : index + 1
                    }).ToList();
                }

                await _exerciseRepository.CreateAsync(exercise);
                TempData["SuccessMessage"] = "Tạo bài tập thành công!";
                return RedirectToAction(nameof(Index), new { lessonId = dto.LessonId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài tập");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài tập. Vui lòng thử lại.");
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                ViewBag.Lesson = lesson;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }

            ViewBag.Lesson = exercise.Lesson;
            var dto = new ExerciseDto
            {
                Id = exercise.Id,
                Question = exercise.Question,
                Content = exercise.Content,
                LessonId = exercise.LessonId,
                Type = exercise.Type,
                Order = exercise.Order,
                IsActive = exercise.IsActive,
                Answers = exercise.Answers?.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect,
                    Order = a.Order
                }).ToList() ?? new List<AnswerDto>()
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExerciseDto dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                ViewBag.Lesson = lesson;
                return View(dto);
            }

            try
            {
                var exercise = await _exerciseRepository.GetByIdAsync(id);
                if (exercise == null)
                {
                    return NotFound();
                }

                exercise.Question = dto.Question;
                exercise.Content = dto.Content;
                exercise.Type = dto.Type;
                exercise.Order = dto.Order;
                exercise.IsActive = dto.IsActive;

                // Cập nhật answers
                if (dto.Answers != null && dto.Answers.Any())
                {
                    exercise.Answers = dto.Answers.Select((answerDto, index) => new Answer
                    {
                        Text = answerDto.Text,
                        IsCorrect = answerDto.IsCorrect,
                        Order = answerDto.Order > 0 ? answerDto.Order : index + 1
                    }).ToList();
                }
                else
                {
                    exercise.Answers = new List<Answer>();
                }

                await _exerciseRepository.UpdateAsync(exercise);
                TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
                return RedirectToAction(nameof(Index), new { lessonId = dto.LessonId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài tập");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài tập. Vui lòng thử lại.");
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                ViewBag.Lesson = lesson;
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int lessonId)
        {
            try
            {
                var result = await _exerciseRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa bài tập thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài tập để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài tập");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bài tập.";
            }

            return RedirectToAction(nameof(Index), new { lessonId });
        }
    }
}

