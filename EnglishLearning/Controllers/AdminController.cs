using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonLib.Interfaces;

namespace EnglishLearning.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ISkillRepository _skillRepository;

        public AdminController(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Trang quản trị";
            var skills = await _skillRepository.GetAllAsync();
            return View(skills);
        }
    }
}

