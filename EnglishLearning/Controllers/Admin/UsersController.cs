using CommonLib.Entities;
using CommonLib.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Controllers.Admin
{
    [Authorize(Policy = "AdminOnly")]
    [Route("Admin/Users")]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IRoleRepository roleRepository,
            ISubmissionRepository submissionRepository,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _roleRepository = roleRepository;
            _submissionRepository = submissionRepository;
            _logger = logger;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            var orders = new Dictionary<int, int>();

            foreach (var u in users)
            {
                var list = await _orderRepository.GetByUserIdAsync(u.Id);
                orders[u.Id] = list.Count(o => o.Status == "Paid");
            }

            ViewBag.OrderCounts = orders;
            return View("~/Views/Admin/Users/Index.cshtml", users);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Index");
            }
            var roles = await _roleRepository.GetAllAsync();
            ViewBag.Roles = roles;
            return View("~/Views/Admin/Users/Edit.cshtml", user);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] User form)
        {
            if (id != form.Id)
            {
                TempData["ErrorMessage"] = "ID người dùng không khớp.";
                return RedirectToAction("Index");
            }

            try
            {
                var existing = await _userRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index");
                }

                existing.RoleId = form.RoleId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existing);
                TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng {UserId}", id);
                TempData["ErrorMessage"] = "Không thể cập nhật người dùng. Vui lòng thử lại.";
                var user = await _userRepository.GetByIdAsync(id);
                var roles = await _roleRepository.GetAllAsync();
                ViewBag.Roles = roles;
                return View("~/Views/Admin/Users/Edit.cshtml", user);
            }
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleRepository.GetAllAsync();
            ViewBag.Roles = roles;
            return View("~/Views/Admin/Users/Create.cshtml", new EnglishLearning.Models.AdminCreateUserViewModel());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnglishLearning.Models.AdminCreateUserViewModel model)
        {
            var roles = await _roleRepository.GetAllAsync();
            ViewBag.Roles = roles;

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            if (await _userRepository.UserExistsAsync(model.Username, model.Email))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại.");
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            var role = await _roleRepository.GetByIdAsync(model.RoleId);
            if (role == null)
            {
                ModelState.AddModelError("RoleId", "Vai trò không hợp lệ.");
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }

            try
            {
                var user = new User
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RoleId = role.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user);
                TempData["SuccessMessage"] = "Tạo người dùng mới thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng mới");
                ModelState.AddModelError("", "Không thể tạo người dùng. Vui lòng thử lại.");
                return View("~/Views/Admin/Users/Create.cshtml", model);
            }
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Index");
                }

                var orders = await _orderRepository.GetByUserIdAsync(id);
                var submissions = await _submissionRepository.GetByUserIdAsync(id);

                if (orders.Any() || submissions.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa người dùng vì đã có đơn hàng hoặc dữ liệu học tập.";
                    return RedirectToAction("Index");
                }

                var deleted = await _userRepository.DeleteAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = "Đã xóa người dùng khỏi hệ thống.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa người dùng.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa người dùng {UserId}", id);
                TempData["ErrorMessage"] = "Không thể xóa người dùng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }
    }
}

