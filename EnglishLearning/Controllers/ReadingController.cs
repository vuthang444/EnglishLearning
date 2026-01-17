using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;
using System.Text.Json;
using System.Security.Claims;
using System.Linq;

namespace EnglishLearning.Controllers
{
    [Authorize]
    public class ReadingController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILogger<ReadingController> _logger;

        public ReadingController(
            ILessonRepository lessonRepository,
            IExerciseRepository exerciseRepository,
            ISubmissionRepository submissionRepository,
            ISkillRepository skillRepository,
            ILogger<ReadingController> logger)
        {
            _lessonRepository = lessonRepository;
            _exerciseRepository = exerciseRepository;
            _submissionRepository = submissionRepository;
            _skillRepository = skillRepository;
            _logger = logger;
        }

        // Hiển thị danh sách bài tập Reading
        [HttpGet]
        [Route("Reading")]
        [Route("Reading/Index")]
        public async Task<IActionResult> Index()
        {
            var readingSkill = await _skillRepository.GetByNameAsync("Reading");
            if (readingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Reading";
                return RedirectToAction("Dashboard", "Home");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(readingSkill.Id);
            var activeLessons = lessons.Where(l => l.IsActive).OrderBy(l => l.Order).ToList();

            // Lấy số lượng câu hỏi cho mỗi bài học
            var lessonExerciseCounts = new Dictionary<int, int>();
            foreach (var lesson in activeLessons)
            {
                var exercises = await _exerciseRepository.GetByLessonIdAsync(lesson.Id);
                lessonExerciseCounts[lesson.Id] = exercises?.Count ?? 0;
            }
            ViewBag.LessonExerciseCounts = lessonExerciseCounts;

            // Lấy thông tin submission của user
            var submissions = new Dictionary<int, Submission>();
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                foreach (var lesson in activeLessons)
                {
                    var submission = await _submissionRepository.GetByUserAndLessonAsync(userId, lesson.Id);
                    if (submission != null)
                    {
                        submissions[lesson.Id] = submission;
                    }
                }
            }
            ViewBag.Submissions = submissions;

            return View("~/Views/Reading/Index.cshtml", activeLessons);
        }

        // Hiển thị bài đọc và câu hỏi
        [HttpGet]
        [Route("Reading/Exercise/{lessonId}")]
        public async Task<IActionResult> Exercise(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài đọc";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra xem bài này có phải Reading không
            var skill = lesson.Skill;
            if (skill == null || skill.Name != "Reading")
            {
                TempData["ErrorMessage"] = "Bài tập này không phải bài đọc hiểu";
                return RedirectToAction("Index", "Home");
            }

            // Lấy danh sách câu hỏi
            var exercises = await _exerciseRepository.GetByLessonIdAsync(lessonId);
            var questions = new List<ReadingQuestionDisplayDto>();

            foreach (var exercise in exercises.OrderBy(e => e.Order))
            {
                var orderedAnswers = exercise.Answers?.OrderBy(a => a.Order).ToList() ?? new List<Answer>();

                if (orderedAnswers.Count >= 4)
                {
                    questions.Add(new ReadingQuestionDisplayDto
                    {
                        Id = exercise.Id,
                        Question = exercise.Question,
                        OptionA = orderedAnswers[0].Text,
                        OptionB = orderedAnswers[1].Text,
                        OptionC = orderedAnswers[2].Text,
                        OptionD = orderedAnswers[3].Text,
                        Order = exercise.Order,
                        Score = 1 // Mỗi câu hỏi 1 điểm
                    });
                }
            }

            var readingExercise = new ReadingExerciseDto
            {
                LessonId = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                ReadingContent = lesson.ReadingContent,
                ReadingLevel = lesson.ReadingLevel,
                Questions = questions,
                MaxScore = questions.Count
            };

            // Lấy userId từ claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Không xác định được người dùng";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra xem đã làm bài chưa
            var existingSubmission = await _submissionRepository.GetByUserAndLessonAsync(userId, lessonId);
            if (existingSubmission != null)
            {
                ViewBag.HasSubmitted = true;
                ViewBag.PreviousScore = existingSubmission.Score;
                ViewBag.PreviousMaxScore = existingSubmission.MaxScore;
            }

            return View("~/Views/Reading/Exercise.cshtml", readingExercise);
        }

        // Submit bài làm
        [HttpPost]
        [Route("Reading/Submit")]
        public async Task<IActionResult> Submit([FromBody] SubmissionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng" });
            }

            // Validate dữ liệu đầu vào
            if (dto == null || dto.LessonId <= 0)
            {
                _logger.LogWarning("Dữ liệu submit không hợp lệ. DTO: {Dto}", dto != null ? "not null" : "null");
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            dto.UserId = userId;

            try
            {
                // Lấy bài tập và câu hỏi
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                if (lesson == null)
                {
                    _logger.LogWarning("Không tìm thấy lesson với Id: {LessonId}", dto.LessonId);
                    return Json(new { success = false, message = "Không tìm thấy bài tập" });
                }

                var exercises = await _exerciseRepository.GetByLessonIdAsync(dto.LessonId);
                if (exercises == null || !exercises.Any())
                {
                    _logger.LogWarning("Lesson {LessonId} không có câu hỏi", dto.LessonId);
                    return Json(new { success = false, message = "Bài tập chưa có câu hỏi" });
                }

                var maxScore = exercises.Count;
                dto.MaxScore = maxScore;

                // Tính điểm
                int score = 0;
                var questionResults = new List<QuestionResultDto>();

                foreach (var exercise in exercises)
                {
                    var userAnswer = dto.UserAnswers?.FirstOrDefault(ua => ua.QuestionId == exercise.Id);
                    var answers = exercise.Answers?.ToList() ?? new List<Answer>();
                    var correctAnswer = answers.FirstOrDefault(a => a.IsCorrect);

                    if (correctAnswer != null)
                    {
                        // Xác định đáp án đúng (A, B, C, hoặc D)
                        var orderedAnswers = answers.OrderBy(a => a.Order).ToList();
                        string correctAnswerLetter = "";
                        if (orderedAnswers.Count > 0 && correctAnswer.Id == orderedAnswers[0].Id) correctAnswerLetter = "A";
                        else if (orderedAnswers.Count > 1 && correctAnswer.Id == orderedAnswers[1].Id) correctAnswerLetter = "B";
                        else if (orderedAnswers.Count > 2 && correctAnswer.Id == orderedAnswers[2].Id) correctAnswerLetter = "C";
                        else if (orderedAnswers.Count > 3 && correctAnswer.Id == orderedAnswers[3].Id) correctAnswerLetter = "D";

                        string selectedAnswer = userAnswer?.SelectedAnswer ?? "";
                        bool isCorrect = selectedAnswer.Equals(correctAnswerLetter, StringComparison.OrdinalIgnoreCase);

                        if (isCorrect)
                        {
                            score += 1;
                        }

                        questionResults.Add(new QuestionResultDto
                        {
                            QuestionId = exercise.Id,
                            Question = exercise.Question,
                            SelectedAnswer = selectedAnswer,
                            CorrectAnswer = correctAnswerLetter,
                            IsCorrect = isCorrect,
                            Score = isCorrect ? 1 : 0
                        });
                    }
                }

                dto.Score = score;
                dto.CompletedAt = DateTime.UtcNow;

                // Lưu vào database
                var submission = new Submission
                {
                    UserId = dto.UserId,
                    LessonId = dto.LessonId,
                    AnswersJson = JsonSerializer.Serialize(dto.UserAnswers ?? new List<UserAnswerDto>()),
                    Score = dto.Score,
                    MaxScore = dto.MaxScore,
                    StartedAt = dto.StartedAt,
                    CompletedAt = dto.CompletedAt,
                    TimeSpentSeconds = dto.TimeSpentSeconds
                };

                try
                {
                    var createdSubmission = await _submissionRepository.CreateAsync(submission);
                    _logger.LogInformation("Đã lưu submission thành công. SubmissionId: {Id}, UserId: {UserId}, LessonId: {LessonId}", 
                        createdSubmission.Id, dto.UserId, dto.LessonId);

                    // Trả về kết quả
                    var result = new SubmissionResultDto
                    {
                        SubmissionId = createdSubmission.Id,
                        Score = dto.Score,
                        MaxScore = dto.MaxScore,
                        Percentage = dto.MaxScore > 0 ? Math.Round((double)dto.Score / dto.MaxScore * 100, 2) : 0,
                        TimeSpentSeconds = dto.TimeSpentSeconds,
                        QuestionResults = questionResults
                    };

                    return Json(new { success = true, result = result });
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Lỗi khi lưu vào database. LessonId: {LessonId}, UserId: {UserId}", dto.LessonId, userId);
                    return Json(new { success = false, message = "Lỗi khi lưu kết quả vào database: " + dbEx.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi submit bài làm. LessonId: {LessonId}, UserId: {UserId}", dto?.LessonId ?? 0, userId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu kết quả. Vui lòng thử lại." });
            }
        }

        // Xem kết quả chi tiết
        [HttpGet]
        [Route("Reading/Result/{submissionId}")]
        public async Task<IActionResult> Result(int submissionId)
        {
            var submission = await _submissionRepository.GetByIdAsync(submissionId);
            if (submission == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kết quả";
                return RedirectToAction("Index", "Home");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Không xác định được người dùng";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra quyền truy cập
            if (submission.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem kết quả này";
                return RedirectToAction("Index", "Home");
            }

            // Parse answers
            var userAnswers = JsonSerializer.Deserialize<List<UserAnswerDto>>(submission.AnswersJson) ?? new List<UserAnswerDto>();

            // Lấy câu hỏi và tính toán kết quả
            var exercises = await _exerciseRepository.GetByLessonIdAsync(submission.LessonId);
            var questionResults = new List<QuestionResultDto>();

            foreach (var exercise in exercises.OrderBy(e => e.Order))
            {
                var userAnswer = userAnswers.FirstOrDefault(ua => ua.QuestionId == exercise.Id);
                var answers = exercise.Answers?.ToList() ?? new List<Answer>();
                var correctAnswer = answers.FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer != null)
                {
                    var orderedAnswers = answers.OrderBy(a => a.Order).ToList();
                    string correctAnswerLetter = "";
                    if (orderedAnswers.Count > 0 && correctAnswer.Id == orderedAnswers[0].Id) correctAnswerLetter = "A";
                    else if (orderedAnswers.Count > 1 && correctAnswer.Id == orderedAnswers[1].Id) correctAnswerLetter = "B";
                    else if (orderedAnswers.Count > 2 && correctAnswer.Id == orderedAnswers[2].Id) correctAnswerLetter = "C";
                    else if (orderedAnswers.Count > 3 && correctAnswer.Id == orderedAnswers[3].Id) correctAnswerLetter = "D";

                    string selectedAnswer = userAnswer?.SelectedAnswer ?? "";
                    bool isCorrect = selectedAnswer.Equals(correctAnswerLetter, StringComparison.OrdinalIgnoreCase);

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = exercise.Id,
                        Question = exercise.Question,
                        SelectedAnswer = selectedAnswer,
                        CorrectAnswer = correctAnswerLetter,
                        IsCorrect = isCorrect,
                        Score = isCorrect ? 1 : 0
                    });
                }
            }

            var result = new SubmissionResultDto
            {
                SubmissionId = submission.Id,
                Score = submission.Score,
                MaxScore = submission.MaxScore,
                Percentage = submission.MaxScore > 0 ? Math.Round((double)submission.Score / submission.MaxScore * 100, 2) : 0,
                TimeSpentSeconds = submission.TimeSpentSeconds,
                QuestionResults = questionResults
            };

            ViewBag.Lesson = await _lessonRepository.GetByIdAsync(submission.LessonId);
            return View("~/Views/Reading/Result.cshtml", result);
        }
    }
}

