using System.ComponentModel.DataAnnotations;

namespace CommonLib.DTOs
{
    public class LessonDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [StringLength(10000, ErrorMessage = "Nội dung bài đọc không được vượt quá 10000 ký tự")]
        public string? ReadingContent { get; set; }

        [StringLength(10, ErrorMessage = "Cấp độ không được vượt quá 10 ký tự")]
        public string? ReadingLevel { get; set; }

        [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
        public int SkillId { get; set; }

        [Range(1, 3, ErrorMessage = "Cấp độ phải từ 1 đến 3")]
        public int Level { get; set; } = 1;

        public int Order { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}

