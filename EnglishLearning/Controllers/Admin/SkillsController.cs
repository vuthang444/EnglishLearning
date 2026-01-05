using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;
using CommonLib.Entities;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class SkillsController : Controller
    {
        private readonly ISkillRepository _skillRepository;
        private readonly ILogger<SkillsController> _logger;

        public SkillsController(ISkillRepository skillRepository, ILogger<SkillsController> logger)
        {
            _skillRepository = skillRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var skills = await _skillRepository.GetAllAsync();
            return View(skills);
        }
    }
}

