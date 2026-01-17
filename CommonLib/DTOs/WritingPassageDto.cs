using System.ComponentModel.DataAnnotations;

namespace CommonLib.DTOs
{
    public class WritingPassageDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Chủ đề là bắt buộc")]
        [StringLength(500)]
        public string WritingTopic { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đề bài là bắt buộc")]
        [StringLength(5000)]
        public string WritingPrompt { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? WritingHints { get; set; } // Gợi ý từ AI

        [StringLength(10)]
        public string? WritingLevel { get; set; } = "B2";

        public int? WordLimit { get; set; } = 250; // Mặc định 250 từ cho IELTS Task 2

        public int? TimeLimitMinutes { get; set; } = 40; // Mặc định 40 phút

        [Required]
        public int SkillId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}


