using System.ComponentModel.DataAnnotations;

namespace EnglishLearning.Models
{
    public class AdminCreateUserViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

