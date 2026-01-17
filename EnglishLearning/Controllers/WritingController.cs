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
    public class WritingController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<WritingController> _logger;

        public WritingController(
            ILessonRepository lessonRepository,
            ISkillRepository skillRepository,
            ISubmissionRepository submissionRepository,
            IOpenAIService openAIService,
            ILogger<WritingController> logger)
        {
            _lessonRepository = lessonRepository;
            _skillRepository = skillRepository;
            _submissionRepository = submissionRepository;
            _openAIService = openAIService;
            _logger = logger;
        }

        // Hiển thị danh sách bài tập Writing
        [HttpGet]
        [Route("Writing")]
        [Route("Writing/Index")]
        public async Task<IActionResult> Index()
        {
            var writingSkill = await _skillRepository.GetByNameAsync("Writing");
            if (writingSkill == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy kỹ năng Writing";
                return RedirectToAction("Dashboard", "Home");
            }

            var lessons = await _lessonRepository.GetBySkillIdAsync(writingSkill.Id);
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

            return View("~/Views/Writing/Index.cshtml", activeLessons);
        }

        // Hiển thị bài tập Writing
        [HttpGet]
        [Route("Writing/Exercise/{lessonId}")]
        public async Task<IActionResult> Exercise(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết";
                return RedirectToAction("Index");
            }

            if (!lesson.IsActive)
            {
                TempData["ErrorMessage"] = "Bài viết này không khả dụng";
                return RedirectToAction("Index");
            }

            ViewBag.Lesson = lesson;
            return View("~/Views/Writing/Exercise.cshtml");
        }

        // Nộp bài viết và chấm điểm
        [HttpPost]
        [Route("Writing/Submit")]
        public async Task<IActionResult> Submit([FromBody] WritingSubmissionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Không xác định được người dùng" });
            }

            if (string.IsNullOrWhiteSpace(dto.Essay))
            {
                return Json(new { success = false, message = "Vui lòng viết bài trước khi nộp" });
            }

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(dto.LessonId);
                if (lesson == null || string.IsNullOrEmpty(lesson.WritingPrompt))
                {
                    return Json(new { success = false, message = "Không tìm thấy bài tập hoặc đề bài" });
                }

                // Chấm điểm bằng GPT-4o
                var evaluation = await _openAIService.EvaluateWritingAsync(
                    dto.Essay, 
                    lesson.WritingPrompt, 
                    lesson.WritingHints
                );

                // Tính điểm tổng
                var overallScore = (int)Math.Round(evaluation.OverallScore);
                var maxScore = 100;

                // Lưu kết quả vào database
                var submission = new Submission
                {
                    UserId = userId,
                    LessonId = dto.LessonId,
                    AnswersJson = JsonSerializer.Serialize(new
                    {
                        essay = dto.Essay,
                        wordCount = evaluation.WordCount,
                        taskResponseScore = evaluation.TaskResponseScore,
                        taskResponseFeedback = evaluation.TaskResponseFeedback,
                        coherenceScore = evaluation.CoherenceScore,
                        coherenceFeedback = evaluation.CoherenceFeedback,
                        lexicalScore = evaluation.LexicalScore,
                        lexicalFeedback = evaluation.LexicalFeedback,
                        suggestedVocabulary = evaluation.SuggestedVocabulary,
                        grammarScore = evaluation.GrammarScore,
                        grammarFeedback = evaluation.GrammarFeedback,
                        grammarErrors = evaluation.GrammarErrors,
                        toneFeedback = evaluation.ToneFeedback,
                        generalFeedback = evaluation.GeneralFeedback,
                        overallScore = evaluation.OverallScore
                    }),
                    Score = overallScore,
                    MaxScore = maxScore,
                    StartedAt = DateTime.UtcNow.AddMinutes(-(dto.TimeSpentMinutes ?? 0)),
                    CompletedAt = DateTime.UtcNow,
                    TimeSpentSeconds = (dto.TimeSpentMinutes ?? 0) * 60
                };

                var createdSubmission = await _submissionRepository.CreateAsync(submission);

                return Json(new
                {
                    success = true,
                    submissionId = createdSubmission.Id,
                    message = "Nộp bài thành công!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm điểm Writing");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi chấm điểm. Vui lòng thử lại." });
            }
        }

        // Xem kết quả chi tiết
        [HttpGet]
        [Route("Writing/Result/{submissionId}")]
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
                            JsonValueKind.Array => prop.Value.EnumerateArray().Select(e => 
                                e.ValueKind == JsonValueKind.String ? (object)(e.GetString() ?? "") :
                                e.ValueKind == JsonValueKind.Object ? ParseGrammarError(e) : (object)e.ToString()
                            ).ToList(),
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
            return View("~/Views/Writing/Result.cshtml", submission);
        }

        // Helper method to parse grammar error from JsonElement
        private object ParseGrammarError(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object) return element.ToString();
            
            var error = new Dictionary<string, object>();
            foreach (var prop in element.EnumerateObject())
            {
                error[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString() ?? "",
                    JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? (object)i : prop.Value.GetDouble(),
                    _ => prop.Value.ToString()
                };
            }
            return error;
        }
    }

    public class WritingSubmissionDto
    {
        public int LessonId { get; set; }
        public string Essay { get; set; } = string.Empty;
        public int? TimeSpentMinutes { get; set; }
    }
}


