using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using EnglishLearning.Models;

namespace EnglishLearning.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly INewsRepository _newsRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IOrderRepository _orderRepository;

        public AdminController(
            ISkillRepository skillRepository,
            ICourseRepository courseRepository,
            INewsRepository newsRepository,
            ILessonRepository lessonRepository,
            IUserRepository userRepository,
            ISubmissionRepository submissionRepository,
            IOrderRepository orderRepository)
        {
            _skillRepository = skillRepository;
            _courseRepository = courseRepository;
            _newsRepository = newsRepository;
            _lessonRepository = lessonRepository;
            _userRepository = userRepository;
            _submissionRepository = submissionRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";
            var courses = await _courseRepository.GetActiveAsync();
            var newsList = await _newsRepository.GetPublishedAsync();
            var skills = await _skillRepository.GetAllAsync();
            var usersCount = await _userRepository.CountAsync();

            var submissions = await _submissionRepository.GetAllAsync();
            var submissionCount = submissions.Count;
            var avgScore = submissionCount > 0 ? submissions.Average(s => s.Score) : 0;

            var allOrders = await _orderRepository.GetAllAsync();
            var premiumUserCount = allOrders
                .Where(o => o.Status == "Paid")
                .Select(o => o.UserId)
                .Distinct()
                .Count();

            var skillStats = new List<SkillStatItem>();
            var skillRoutes = new Dictionary<string, (string Url, string Icon)>(StringComparer.OrdinalIgnoreCase)
            {
                ["Listening"] = ("/Admin/Listening", "bi-earbuds"),
                ["Speaking"] = ("/Admin/Speaking", "bi-mic"),
                ["Reading"] = ("/Admin/Reading", "bi-book"),
                ["Writing"] = ("/Admin/Writing", "bi-pencil")
            };

            foreach (var skill in skills.OrderBy(s => s.Id))
            {
                var lessons = await _lessonRepository.GetBySkillIdAsync(skill.Id);
                var route = skillRoutes.TryGetValue(skill.Name, out var r) ? r : ("/Admin/Lessons?skillId=" + skill.Id, "bi-journal");
                skillStats.Add(new SkillStatItem
                {
                    SkillId = skill.Id,
                    Name = skill.Name,
                    Description = skill.Description ?? "",
                    LessonCount = lessons.Count,
                    AdminUrl = route.Item1,
                    IconClass = route.Item2
                });
            }

            var vm = new AdminDashboardVm
            {
                CourseCount = courses.Count,
                NewsCount = newsList.Count,
                UserCount = usersCount,
                PremiumUserCount = premiumUserCount,
                SubmissionCount = submissionCount,
                AverageScore = Math.Round(avgScore, 1),
                SkillStats = skillStats
            };

            return View(vm);
        }
    }
}

