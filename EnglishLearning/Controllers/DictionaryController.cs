using CommonLib.DTOs;
using CommonLib.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Controllers
{
    [Authorize]
    public class DictionaryController : Controller
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<DictionaryController> _logger;

        public DictionaryController(
            IOpenAIService openAIService,
            ILogger<DictionaryController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var wordOfTheDay = await _openAIService.GetWordOfTheDayAsync();
                ViewBag.WordOfTheDay = wordOfTheDay;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Word of the day");
                ViewBag.WordOfTheDay = new WordOfTheDayDto
                {
                    Word = "package",
                    Phonetic = "/'pæk-1d3/",
                    Definition = "Một vật hoặc nhóm vật được gói trong giấy, thường để gửi qua bưu điện."
                };
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromForm] string word, [FromForm] string fromLang = "EN", [FromForm] string toLang = "VI")
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var result = await _openAIService.LookupWordAsync(word.Trim(), fromLang, toLang);
                return View("Detail", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tra từ: {Word}", word);
                TempData["Error"] = "Không thể tra từ. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var result = await _openAIService.LookupWordAsync(word.Trim());
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tra từ: {Word}", word);
                TempData["Error"] = "Không thể tra từ. Vui lòng thử lại sau.";
                return RedirectToAction("Index");
            }
        }
    }
}
