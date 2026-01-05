using System.ComponentModel.DataAnnotations;

namespace CommonLib.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc Email là bắt buộc")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
    }
}

