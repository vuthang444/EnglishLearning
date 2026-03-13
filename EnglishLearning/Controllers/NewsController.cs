using CommonLib.DTOs;
using CommonLib.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishLearning.Controllers
{
    [Route("News")]
    public class NewsController : Controller
    {
        private readonly INewsRepository _newsRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            INewsRepository newsRepository,
            IOpenAIService openAIService,
            IOrderRepository orderRepository,
            ILogger<NewsController> logger)
        {
            _newsRepository = newsRepository;
            _openAIService = openAIService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isPremium = false;
            
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                isPremium = await _orderRepository.HasActivePremiumAsync(userId);
            }
            
            // Free users chỉ xem Free Preview, Premium xem tất cả
            var news = isPremium 
                ? await _newsRepository.GetPublishedAsync()
                : await _newsRepository.GetPublishedForFreeAsync();

            ViewBag.IsPremium = isPremium;
            return View("~/Views/News/Index.cshtml", news);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var news = await _newsRepository.GetByIdWithVocabulariesAsync(id);
            if (news == null || !news.IsPublished)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tin tức.";
                return RedirectToAction("Index");
            }

            // Kiểm tra quyền truy cập: Free users chỉ xem Free Preview
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isPremium = false;
            
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                isPremium = await _orderRepository.HasActivePremiumAsync(userId);
            }

            // Nếu không phải Premium và bài không phải Free Preview thì không cho xem
            if (!isPremium && !news.IsFreePreview)
            {
                TempData["ErrorMessage"] = "Bài tin tức này chỉ dành cho người dùng Premium. Vui lòng nâng cấp tài khoản.";
                return RedirectToAction("Index");
            }

            ViewBag.IsPremium = isPremium;
            return View("~/Views/News/Detail.cshtml", news);
        }

        [Authorize]
        [HttpPost]
        [Route("GetSummary")]
        public async Task<IActionResult> GetSummary([FromBody] Dictionary<string, int> data)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { error = "Unauthorized" });
            }

            var isPremium = await _orderRepository.HasActivePremiumAsync(userId);
            if (!isPremium)
            {
                return Json(new { error = "Tính năng này chỉ dành cho Premium users." });
            }

            if (!data.ContainsKey("newsId"))
            {
                return Json(new { error = "Thiếu tham số newsId." });
            }

            try
            {
                var news = await _newsRepository.GetByIdAsync(data["newsId"]);
                if (news == null)
                {
                    return Json(new { error = "Không tìm thấy bài tin tức." });
                }

                var summary = await _openAIService.SummarizeNewsAsync(news.Content);
                return Json(new { summary = summary.Summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo tóm tắt");
                return Json(new { error = "Không thể tạo tóm tắt. Vui lòng thử lại." });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("GetGrammarExplanation")]
        public async Task<IActionResult> GetGrammarExplanation([FromBody] Dictionary<string, int> data)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { error = "Unauthorized" });
            }

            var isPremium = await _orderRepository.HasActivePremiumAsync(userId);
            if (!isPremium)
            {
                return Json(new { error = "Tính năng này chỉ dành cho Premium users." });
            }

            if (!data.ContainsKey("newsId"))
            {
                return Json(new { error = "Thiếu tham số newsId." });
            }

            try
            {
                var news = await _newsRepository.GetByIdAsync(data["newsId"]);
                if (news == null)
                {
                    return Json(new { error = "Không tìm thấy bài tin tức." });
                }

                var grammar = await _openAIService.ExplainGrammarAsync(news.Content);
                return Json(new { grammarPoints = grammar.GrammarPoints });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi giải thích ngữ pháp");
                return Json(new { error = "Không thể giải thích ngữ pháp. Vui lòng thử lại." });
            }
        }
    }
}
