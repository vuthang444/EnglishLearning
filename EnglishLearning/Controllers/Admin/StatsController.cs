using CommonLib.Interfaces;
using EnglishLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    [Route("Admin/Stats")]
    public class StatsController : Controller
    {
        private readonly ISubmissionRepository _submissionRepository;

        public StatsController(ISubmissionRepository submissionRepository)
        {
            _submissionRepository = submissionRepository;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var submissions = await _submissionRepository.GetAllAsync();

            var bySkill = submissions
                .Where(s => s.Lesson?.Skill != null)
                .GroupBy(s => s.Lesson!.Skill!.Name)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var fromDate = DateTime.UtcNow.Date.AddDays(-6);
            var dateGroups = submissions
                .Select(s => (s.CompletedAt ?? s.CreatedAt).Date)
                .Where(d => d >= fromDate)
                .GroupBy(d => d)
                .ToDictionary(g => g.Key, g => g.Count());

            var labels = new List<string>();
            var counts = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                var day = fromDate.AddDays(i);
                labels.Add(day.ToString("dd/MM"));
                counts.Add(dateGroups.TryGetValue(day, out var c) ? c : 0);
            }

            var vm = new AdminStatsViewModel
            {
                SubmissionsBySkill = bySkill,
                Last7DaysLabels = labels,
                Last7DaysCounts = counts
            };

            return View("~/Views/Admin/Stats/Index.cshtml", vm);
        }
    }
}

