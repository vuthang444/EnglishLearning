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

        // Speaking fields
        [StringLength(500, ErrorMessage = "Chủ đề không được vượt quá 500 ký tự")]
        public string? Topic { get; set; }

        [StringLength(10000, ErrorMessage = "Văn bản mẫu không được vượt quá 10000 ký tự")]
        public string? ReferenceText { get; set; }

        [StringLength(10, ErrorMessage = "Cấp độ không được vượt quá 10 ký tự")]
        public string? SpeakingLevel { get; set; }

        [Range(10, 300, ErrorMessage = "Thời gian giới hạn phải từ 10 đến 300 giây")]
        public int TimeLimitSeconds { get; set; } = 60;

        // Writing fields
        [StringLength(500, ErrorMessage = "Chủ đề không được vượt quá 500 ký tự")]
        public string? WritingTopic { get; set; }

        [StringLength(5000, ErrorMessage = "Đề bài không được vượt quá 5000 ký tự")]
        public string? WritingPrompt { get; set; }

        [StringLength(5000, ErrorMessage = "Gợi ý không được vượt quá 5000 ký tự")]
        public string? WritingHints { get; set; }

        [StringLength(10, ErrorMessage = "Cấp độ không được vượt quá 10 ký tự")]
        public string? WritingLevel { get; set; }

        [Range(100, 1000, ErrorMessage = "Giới hạn số từ phải từ 100 đến 1000")]
        public int? WordLimit { get; set; }

        [Range(15, 120, ErrorMessage = "Thời gian giới hạn phải từ 15 đến 120 phút")]
        public int? TimeLimitMinutes { get; set; }

        [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
        public int SkillId { get; set; }

        [Range(1, 3, ErrorMessage = "Cấp độ phải từ 1 đến 3")]
        public int Level { get; set; } = 1;

        public int Order { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}

