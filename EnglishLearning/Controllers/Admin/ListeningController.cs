using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class ListeningController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogger<ListeningController> _logger;

        public ListeningController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            IExerciseRepository exerciseRepository,
            ILogger<ListeningController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _exerciseRepository = exerciseRepository;
            _logger = logger;
        }

        // Trang chủ Listening - Dashboard
        [Route("Admin/Listening")]
        [Route("Admin/Listening/Index")]
        public async Task<IActionResult> Index()
        {
            var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
            if (listeningSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Listening";
                return RedirectToAction("Index", "Admin");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(listeningSkill.Id);
            
            // Thống kê
            ViewBag.TotalLessons = lessons.Count;
            ViewBag.ActiveLessons = lessons.Count(l => l.IsActive);
            ViewBag.TotalExercises = 0;
            foreach (var lesson in lessons)
            {
                var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
                ViewBag.TotalExercises = (int)ViewBag.TotalExercises + exercises.Count;
            }

            return View("~/Views/Admin/Listening/Index.cshtml", lessons);
        }

        // Chi tiết bài nghe
        [Route("Admin/Listening/LessonDetail/{id}")]
        public async Task<IActionResult> LessonDetail(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài học";
                return RedirectToAction("Index");
            }

            var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
            ViewBag.Exercises = exercises;

            return View("~/Views/Admin/Listening/LessonDetail.cshtml", lesson);
        }

        // Tạo bài nghe mới
        [HttpGet]
        [Route("Admin/Listening/CreateListeningPassage")]
        public async Task<IActionResult> CreateListeningPassage()
        {
            var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
            if (listeningSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Listening";
                return RedirectToAction("Index");
            }

            ViewBag.Skill = listeningSkill;
            var dto = new ListeningPassageDto
            {
                SkillId = listeningSkill.Id,
                Level = 1,
                DefaultSpeed = 1.0,
                PlayLimit = null
            };
            return View("~/Views/Admin/Listening/CreateListeningPassage.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Listening/CreateListeningPassage")]
        public async Task<IActionResult> CreateListeningPassage(ListeningPassageDto dto)
        {
            var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
            if (listeningSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Listening";
                return RedirectToAction("Index");
            }

            dto.SkillId = listeningSkill.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.Skill = listeningSkill;
                return View("~/Views/Admin/Listening/CreateListeningPassage.cshtml", dto);
            }

            try
            {
                // Tạo Lesson với thông tin bài nghe
                var lesson = new Lesson
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    AudioUrl = dto.AudioUrl,
                    Transcript = dto.Transcript,
                    HideTranscript = dto.HideTranscript,
                    PlayLimit = dto.PlayLimit,
                    DefaultSpeed = dto.DefaultSpeed,
                    SkillId = dto.SkillId,
                    Level = dto.Level,
                    Order = dto.Order,
                    IsActive = true
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
                            Timestamp = questionDto.Timestamp,
                            LessonId = createdLesson.Id,
                            Type = ExerciseType.Audio,
                            Order = questionOrder++,
                            IsActive = true
                        };

                        // Tạo các phương án trả lời
                        exercise.Answers = new List<Answer>();
                        int answerOrder = 1;
                        foreach (var answerDto in questionDto.Answers)
                        {
                            exercise.Answers.Add(new Answer
                            {
                                Text = answerDto.Text,
                                IsCorrect = answerDto.IsCorrect,
                                Order = answerOrder++
                            });
                        }

                        await _exerciseRepository.CreateAsync(exercise);
                    }
                }

                TempData["SuccessMessage"] = "Tạo bài nghe với câu hỏi thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài nghe");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo bài nghe. Vui lòng thử lại.");
                ViewBag.Skill = listeningSkill;
                return View("~/Views/Admin/Listening/CreateListeningPassage.cshtml", dto);
            }
        }

        // Sửa bài nghe
        [HttpGet]
        [Route("Admin/Listening/EditLesson/{id}")]
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài học";
                return RedirectToAction("Index");
            }

            var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
            var listeningSkill = await _skillRepository.GetByNameAsync("Listening");

            ViewBag.Skill = listeningSkill;
            ViewBag.Exercises = exercises;

            var dto = new ListeningPassageDto
            {
                Title = lesson.Title,
                Description = lesson.Description,
                AudioUrl = lesson.AudioUrl,
                Transcript = lesson.Transcript,
                HideTranscript = lesson.HideTranscript,
                PlayLimit = lesson.PlayLimit,
                DefaultSpeed = lesson.DefaultSpeed,
                SkillId = lesson.SkillId,
                Level = lesson.Level,
                Order = lesson.Order
            };

            // Map exercises to questions
            foreach (var exercise in exercises.OrderBy(e => e.Order))
            {
                var questionDto = new ListeningQuestionDto
                {
                    Question = exercise.Question,
                    Timestamp = exercise.Timestamp,
                    Order = exercise.Order
                };

                foreach (var answer in exercise.Answers.OrderBy(a => a.Order))
                {
                    questionDto.Answers.Add(new ListeningAnswerDto
                    {
                        Text = answer.Text,
                        IsCorrect = answer.IsCorrect,
                        Order = answer.Order
                    });
                }

                dto.Questions.Add(questionDto);
            }

            return View("~/Views/Admin/Listening/EditLesson.cshtml", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Listening/EditLesson/{id}")]
        public async Task<IActionResult> EditLesson(int id, ListeningPassageDto dto)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài học";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
                ViewBag.Skill = listeningSkill;
                var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
                ViewBag.Exercises = exercises;
                return View("~/Views/Admin/Listening/EditLesson.cshtml", dto);
            }

            try
            {
                // Cập nhật Lesson
                lesson.Title = dto.Title;
                lesson.Description = dto.Description;
                lesson.AudioUrl = dto.AudioUrl;
                lesson.Transcript = dto.Transcript;
                lesson.HideTranscript = dto.HideTranscript;
                lesson.PlayLimit = dto.PlayLimit;
                lesson.DefaultSpeed = dto.DefaultSpeed;
                lesson.Level = dto.Level;
                lesson.Order = dto.Order;
                lesson.UpdatedAt = DateTime.UtcNow;

                await _lessonRepository.UpdateAsync(lesson);

                // Xóa các exercises cũ
                var existingExercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
                foreach (var exercise in existingExercises)
                {
                    await _exerciseRepository.DeleteAsync(exercise.Id);
                }

                // Tạo lại các câu hỏi
                if (dto.Questions != null && dto.Questions.Any())
                {
                    int questionOrder = 1;
                    foreach (var questionDto in dto.Questions)
                    {
                        var exercise = new Exercise
                        {
                            Question = questionDto.Question,
                            Timestamp = questionDto.Timestamp,
                            LessonId = lesson.Id,
                            Type = ExerciseType.Audio,
                            Order = questionOrder++,
                            IsActive = true
                        };

                        exercise.Answers = new List<Answer>();
                        int answerOrder = 1;
                        foreach (var answerDto in questionDto.Answers)
                        {
                            exercise.Answers.Add(new Answer
                            {
                                Text = answerDto.Text,
                                IsCorrect = answerDto.IsCorrect,
                                Order = answerOrder++
                            });
                        }

                        await _exerciseRepository.CreateAsync(exercise);
                    }
                }

                TempData["SuccessMessage"] = "Cập nhật bài nghe thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài nghe");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật bài nghe. Vui lòng thử lại.");
                var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
                ViewBag.Skill = listeningSkill;
                var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
                ViewBag.Exercises = exercises;
                return View("~/Views/Admin/Listening/EditLesson.cshtml", dto);
            }
        }

        // Xóa bài nghe
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Listening/DeleteLesson/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            try
            {
                var result = await _lessonRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa bài nghe thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài nghe để xóa";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài nghe");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa bài nghe";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

