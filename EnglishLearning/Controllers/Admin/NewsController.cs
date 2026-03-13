using CommonLib.DTOs;
using CommonLib.Entities;
using CommonLib.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    [Route("Admin/News")]
    public class NewsController : Controller
    {
        private readonly INewsRepository _newsRepository;
        private readonly IOpenAIService _openAIService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<NewsController> _logger;
        private const string NewsUploadFolder = "uploads/news";

        public NewsController(
            INewsRepository newsRepository,
            IOpenAIService openAIService,
            IWebHostEnvironment env,
            ILogger<NewsController> logger)
        {
            _newsRepository = newsRepository;
            _openAIService = openAIService;
            _env = env;
            _logger = logger;
        }

        private string? SaveFeaturedImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext)) return null;
            var dir = Path.Combine(_env.WebRootPath, NewsUploadFolder);
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(dir, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
                file.CopyTo(stream);
            return "/" + NewsUploadFolder.Replace('\\', '/') + "/" + fileName;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var news = await _newsRepository.GetAllAsync();
            return View("~/Views/Admin/News/Index.cshtml", news);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/News/Create.cshtml");
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] string topicOrUrl, [FromForm] string level, [FromForm] bool publishImmediately = true, [FromForm] IFormFile? featuredImage = null)
        {
            if (string.IsNullOrWhiteSpace(topicOrUrl))
            {
                TempData["Error"] = "Vui lòng nhập chủ đề hoặc link báo.";
                return View();
            }

            try
            {
                // Gọi AI để tạo bài tin tức
                var aiResult = await _openAIService.GenerateNewsAsync(topicOrUrl.Trim(), level);

                // Tạo News entity
                var news = new News
                {
                    Title = aiResult.Title,
                    MetaDescription = aiResult.MetaDescription,
                    Content = aiResult.Content,
                    Level = aiResult.Level,
                    Topic = aiResult.Topic,
                    SourceUrl = topicOrUrl.StartsWith("http") ? topicOrUrl : null,
                    IsPublished = publishImmediately,
                    IsFreePreview = true,
                    CreatedAt = DateTime.UtcNow,
                    PublishedAt = publishImmediately ? DateTime.UtcNow : null
                };

                if (featuredImage != null)
                {
                    news.FeaturedImagePath = SaveFeaturedImage(featuredImage);
                }

                // Thêm vocabularies
                for (int i = 0; i < aiResult.Vocabularies.Count; i++)
                {
                    var vocab = aiResult.Vocabularies[i];
                    news.Vocabularies.Add(new NewsVocabulary
                    {
                        Word = vocab.Word,
                        VietnameseMeaning = vocab.VietnameseMeaning,
                        Example = vocab.Example,
                        Order = i + 1
                    });
                }

                await _newsRepository.CreateAsync(news);
                TempData["Success"] = "Tạo bài tin tức thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bài tin tức");
                TempData["Error"] = "Không thể tạo bài tin tức. Vui lòng thử lại.";
                return View("~/Views/Admin/News/Create.cshtml");
            }
        }

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsRepository.GetByIdWithVocabulariesAsync(id);
            if (news == null)
            {
                TempData["Error"] = "Không tìm thấy bài tin tức.";
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/News/Edit.cshtml", news);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] News news, [FromForm] IFormFile? featuredImage = null)
        {
            if (id != news.Id)
            {
                return NotFound();
            }

            try
            {
                var existingNews = await _newsRepository.GetByIdWithVocabulariesAsync(id);
                if (existingNews == null)
                {
                    TempData["Error"] = "Không tìm thấy bài tin tức.";
                    return RedirectToAction("Index");
                }

                existingNews.Title = news.Title;
                existingNews.MetaDescription = news.MetaDescription;
                existingNews.Content = news.Content;
                existingNews.Level = news.Level;
                existingNews.SourceUrl = news.SourceUrl;
                existingNews.Topic = news.Topic;
                existingNews.IsPublished = news.IsPublished;
                existingNews.IsFreePreview = news.IsFreePreview;
                existingNews.UpdatedAt = DateTime.UtcNow;

                if (featuredImage != null)
                {
                    existingNews.FeaturedImagePath = SaveFeaturedImage(featuredImage);
                }

                if (news.IsPublished && existingNews.PublishedAt == null)
                {
                    existingNews.PublishedAt = DateTime.UtcNow;
                }

                await _newsRepository.UpdateAsync(existingNews);
                TempData["Success"] = "Cập nhật bài tin tức thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bài tin tức");
                TempData["Error"] = "Không thể cập nhật bài tin tức.";
                return View("~/Views/Admin/News/Edit.cshtml", news);
            }
        }

        [HttpPost]
        [Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _newsRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Xóa bài tin tức thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy bài tin tức.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bài tin tức");
                TempData["Error"] = "Không thể xóa bài tin tức.";
            }

            return RedirectToAction("Index");
        }
    }
}
