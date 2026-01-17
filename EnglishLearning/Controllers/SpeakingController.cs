using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.DTOs;
using CommonLib.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace EnglishLearning.Controllers
{
    [Authorize]
    public class SpeakingController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<SpeakingController> _logger;

        public SpeakingController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            ISubmissionRepository submissionRepository,
            IOpenAIService openAIService,
            ILogger<SpeakingController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _submissionRepository = submissionRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        // Hiển thị danh sách bài tập Speaking
        [HttpGet]
        [Route("Speaking")]
        [Route("Speaking/Index")]
        public async Task<IActionResult> Index()
        {
            var speakingSkill = await _skillRepository.GetByNameAsync("Speaking");
            if (speakingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Speaking";
                return RedirectToAction("Dashboard", "Home");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(speakingSkill.Id);
            var activeLessons = lessons.Where(l => l.IsActive).OrderBy(l => l.Order).ToList();

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

            return View("~/Views/Speaking/Index.cshtml", activeLessons);
        }

        // Hiển thị bài tập Speaking
        [HttpGet]
        [Route("Speaking/Exercise/{lessonId}")]
        public async Task<IActionResult> Exercise(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài nói";
                return RedirectToAction("Index");
            }

            // Kiểm tra xem bài này có phải Speaking không
            var skill = lesson.Skill;
            if (skill == null || skill.Name != "Speaking")
            {
                TempData["ErrorMessage"] = "Bài tập này không phải bài nói";
                return RedirectToAction("Index");
            }

            var exerciseDto = new SpeakingExerciseDto
            {
                LessonId = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Topic = lesson.Topic ?? "",
                ReferenceText = lesson.ReferenceText,
                DifficultyLevel = lesson.SpeakingLevel,
                TimeLimitSeconds = lesson.TimeLimitSeconds
            };

            // Kiểm tra xem đã làm bài chưa
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var existingSubmission = await _submissionRepository.GetByUserAndLessonAsync(userId, lessonId);
                if (existingSubmission != null)
                {
                    ViewBag.HasSubmitted = true;
                    ViewBag.PreviousScore = existingSubmission.Score;
                    ViewBag.PreviousMaxScore = existingSubmission.MaxScore;
                }
            }

            return View("~/Views/Speaking/Exercise.cshtml", exerciseDto);
        }

        // Upload audio và chấm điểm
        [HttpPost]
        [Route("Speaking/Evaluate")]
        public async Task<IActionResult> Evaluate([FromForm] IFormFile audioFile, [FromForm] int lessonId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng" });
            }

            if (audioFile == null || audioFile.Length == 0)
            {
                return Json(new { success = false, message = "File audio không hợp lệ" });
            }

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson == null || lesson.ReferenceText == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài tập hoặc văn bản mẫu" });
                }

                // Chuyển đổi audio thành text bằng Whisper
                string transcription;
                using (var audioStream = audioFile.OpenReadStream())
                {
                    transcription = await _openAIService.TranscribeAudioAsync(audioStream, audioFile.FileName);
                }

                // Chấm điểm bằng GPT-4o
                var evaluation = await _openAIService.EvaluateSpeakingAsync(transcription, lesson.ReferenceText);

                // Tính điểm tổng (trung bình của Accuracy và Fluency)
                var overallScore = (int)Math.Round(evaluation.OverallScore);
                var maxScore = 100;

                // Lưu kết quả vào database
                var submission = new Submission
                {
                    UserId = userId,
                    LessonId = lessonId,
                    AnswersJson = JsonSerializer.Serialize(new
                    {
                        transcription = evaluation.Transcription,
                        accuracy = evaluation.Accuracy,
                        fluency = evaluation.Fluency,
                        mispronouncedWords = evaluation.MispronouncedWords,
                        hesitationCount = evaluation.HesitationCount,
                        feedback = evaluation.Feedback
                    }),
                    Score = overallScore,
                    MaxScore = maxScore,
                    StartedAt = DateTime.UtcNow.AddSeconds(-evaluation.HesitationCount * 2), // Estimate
                    CompletedAt = DateTime.UtcNow,
                    TimeSpentSeconds = lesson.TimeLimitSeconds
                };

                var createdSubmission = await _submissionRepository.CreateAsync(submission);

                return Json(new
                {
                    success = true,
                    submissionId = createdSubmission.Id,
                    evaluation = new
                    {
                        accuracy = evaluation.Accuracy,
                        fluency = evaluation.Fluency,
                        overallScore = evaluation.OverallScore,
                        mispronouncedWords = evaluation.MispronouncedWords,
                        transcription = evaluation.Transcription,
                        hesitationCount = evaluation.HesitationCount,
                        feedback = evaluation.Feedback
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm điểm Speaking");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi chấm điểm. Vui lòng thử lại." });
            }
        }

        // Xem kết quả chi tiết
        [HttpGet]
        [Route("Speaking/Result/{submissionId}")]
        public async Task<IActionResult> Result(int submissionId)
        {
            var submission = await _submissionRepository.GetByIdAsync(submissionId);
            if (submission == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kết quả";
                return RedirectToAction("Index");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["ErrorMessage"] = "Không xác định được người dùng";
                return RedirectToAction("Index");
            }

            if (submission.UserId != userId && !User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem kết quả này";
                return RedirectToAction("Index");
            }

            var lesson = await _lessonRepository.GetByIdAsync(submission.LessonId);
            ViewBag.Lesson = lesson;

            // Parse evaluation data - use JsonDocument to properly handle JsonElement
            var evaluationData = new Dictionary<string, object>();
            try
            {
                using var doc = JsonDocument.Parse(submission.AnswersJson);
                var root = doc.RootElement;
                
                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in root.EnumerateObject())
                    {
                        // Convert JsonElement to appropriate type
                        object value = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString() ?? "",
                            JsonValueKind.Number => prop.Value.TryGetDouble(out var d) ? (object)d : prop.Value.GetInt32(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Array => prop.Value.EnumerateArray().Select(e => e.GetString() ?? "").ToList(),
                            _ => prop.Value.ToString()
                        };
                        evaluationData[prop.Name] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi parse evaluation data từ submission");
            }
            
            ViewBag.Evaluation = evaluationData;

            return View("~/Views/Speaking/Result.cshtml", submission);
        }
    }
}



