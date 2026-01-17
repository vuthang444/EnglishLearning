using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;
using System.Text.Json;
using System.Security.Claims;

namespace EnglishLearning.Controllers
{
    [Authorize]
    public class ListeningController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ILogger<ListeningController> _logger;

        public ListeningController(
            ILessonRepository lessonRepository,
            IExerciseRepository exerciseRepository,
            ISubmissionRepository submissionRepository,
            ISkillRepository skillRepository,
            ILogger<ListeningController> logger)
        {
            _lessonRepository = lessonRepository;
            _exerciseRepository = exerciseRepository;
            _submissionRepository = submissionRepository;
            _skillRepository = skillRepository;
            _logger = logger;
        }

        // Hiển thị danh sách bài tập Listening
        [HttpGet]
        [Route("Listening")]
        [Route("Listening/Index")]
        public async Task<IActionResult> Index()
        {
            var listeningSkill = await _skillRepository.GetByNameAsync("Listening");
            if (listeningSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Listening";
                return RedirectToAction("Dashboard", "Home");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(listeningSkill.Id);
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

            return View("~/Views/Listening/Index.cshtml", activeLessons);
        }

        // Hiển thị bài nghe và câu hỏi
        [HttpGet]
        [Route("Listening/Exercise/{lessonId}")]
        public async Task<IActionResult> Exercise(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài nghe";
                return RedirectToAction("Dashboard", "Home");
            }

            // Kiểm tra xem bài này có phải Listening không
            var skill = lesson.Skill;
            if (skill == null || skill.Name != "Listening")
            {
                TempData["ErrorMessage"] = "Bài tập này không phải bài nghe";
                return RedirectToAction("Dashboard", "Home");
            }

            // Lấy danh sách câu hỏi
            var exercises = await _exerciseRepository.GetByLessonIdAsync(lessonId);
            var questions = new List<ListeningQuestionDisplayDto>();

            foreach (var exercise in exercises.OrderBy(e => e.Order))
            {
                var orderedAnswers = exercise.Answers?.OrderBy(a => a.Order).ToList() ?? new List<Answer>();

                questions.Add(new ListeningQuestionDisplayDto
                {
                    Id = exercise.Id,
                    Question = exercise.Question,
                    Timestamp = exercise.Timestamp,
                    Order = exercise.Order,
                    Answers = orderedAnswers.Select((a, index) => new ListeningAnswerDisplayDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        Order = index + 1
                    }).ToList()
                });
            }

            var listeningExercise = new ListeningExerciseDto
            {
                LessonId = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                AudioUrl = lesson.AudioUrl,
                Transcript = lesson.Transcript,
                HideTranscript = lesson.HideTranscript,
                PlayLimit = lesson.PlayLimit,
                DefaultSpeed = lesson.DefaultSpeed,
                Questions = questions,
                PlayCount = 0 // Sẽ được cập nhật từ localStorage
            };

            // Lấy userId từ claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Không xác định được người dùng";
                return RedirectToAction("Dashboard", "Home");
            }

            // Kiểm tra xem đã làm bài chưa
            var existingSubmission = await _submissionRepository.GetByUserAndLessonAsync(userId, lessonId);
            if (existingSubmission != null)
            {
                ViewBag.HasSubmitted = true;
                ViewBag.PreviousScore = existingSubmission.Score;
                ViewBag.PreviousMaxScore = existingSubmission.MaxScore;
            }

            return View("~/Views/Listening/Exercise.cshtml", listeningExercise);
        }

        // Submit bài làm
        [HttpPost]
        [Route("Listening/Submit")]
        public async Task<IActionResult> Submit([FromBody] SubmissionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng" });
            }

            if (dto == null || dto.LessonId <= 0)
            {
                _logger.LogWarning("Dữ liệu submit không hợp lệ");
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            dto.UserId = userId;

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                if (lesson == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài tập" });
                }

                var exercises = await _exerciseRepository.GetByLessonIdAsync(dto.LessonId);
                if (exercises == null || !exercises.Any())
                {
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
                    var answers = exercise.Answers?.OrderBy(a => a.Order).ToList() ?? new List<Answer>();
                    var correctAnswer = answers.FirstOrDefault(a => a.IsCorrect);

                    if (correctAnswer != null)
                    {
                        // Xác định đáp án đúng (A, B, C, hoặc D)
                        int correctIndex = answers.IndexOf(correctAnswer);
                        string correctAnswerLetter = correctIndex >= 0 && correctIndex < 26 
                            ? ((char)('A' + correctIndex)).ToString() 
                            : "";

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

                var createdSubmission = await _submissionRepository.CreateAsync(submission);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi submit bài làm");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu kết quả. Vui lòng thử lại." });
            }
        }

        // Xem kết quả chi tiết
        [HttpGet]
        [Route("Listening/Result/{submissionId}")]
        public async Task<IActionResult> Result(int submissionId)
        {
            var submission = await _submissionRepository.GetByIdAsync(submissionId);
            if (submission == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kết quả";
                return RedirectToAction("Dashboard", "Home");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Không xác định được người dùng";
                return RedirectToAction("Dashboard", "Home");
            }

            if (submission.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem kết quả này";
                return RedirectToAction("Dashboard", "Home");
            }

            var userAnswers = JsonSerializer.Deserialize<List<UserAnswerDto>>(submission.AnswersJson) ?? new List<UserAnswerDto>();
            var exercises = await _exerciseRepository.GetByLessonIdAsync(submission.LessonId);
            var questionResults = new List<QuestionResultDto>();

            foreach (var exercise in exercises.OrderBy(e => e.Order))
            {
                var userAnswer = userAnswers.FirstOrDefault(ua => ua.QuestionId == exercise.Id);
                var answers = exercise.Answers?.OrderBy(a => a.Order).ToList() ?? new List<Answer>();
                var correctAnswer = answers.FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer != null)
                {
                    int correctIndex = answers.IndexOf(correctAnswer);
                    string correctAnswerLetter = correctIndex >= 0 && correctIndex < 26 
                        ? ((char)('A' + correctIndex)).ToString() 
                        : "";

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
            return View("~/Views/Listening/Result.cshtml", result);
        }
    }
}

