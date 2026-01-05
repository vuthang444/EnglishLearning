using System.ComponentModel.DataAnnotations;
using CommonLib.Entities;

namespace CommonLib.DTOs
{
    public class ReadingPassageDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề bài đọc là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Nội dung bài đọc là bắt buộc")]
        [StringLength(10000, ErrorMessage = "Nội dung bài đọc không được vượt quá 10000 ký tự")]
        public string ReadingContent { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Cấp độ không được vượt quá 10 ký tự")]
        public string? ReadingLevel { get; set; }

        [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
        public int SkillId { get; set; }

        [Range(1, 3, ErrorMessage = "Cấp độ phải từ 1 đến 3")]
        public int Level { get; set; } = 1;

        public int Order { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public List<ReadingQuestionDto> Questions { get; set; } = new List<ReadingQuestionDto>();
    }

    public class ReadingQuestionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Câu hỏi là bắt buộc")]
        [StringLength(500, ErrorMessage = "Câu hỏi không được vượt quá 500 ký tự")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phương án A là bắt buộc")]
        [StringLength(500, ErrorMessage = "Phương án không được vượt quá 500 ký tự")]
        public string OptionA { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phương án B là bắt buộc")]
        [StringLength(500, ErrorMessage = "Phương án không được vượt quá 500 ký tự")]
        public string OptionB { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phương án C là bắt buộc")]
        [StringLength(500, ErrorMessage = "Phương án không được vượt quá 500 ký tự")]
        public string OptionC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phương án D là bắt buộc")]
        [StringLength(500, ErrorMessage = "Phương án không được vượt quá 500 ký tự")]
        public string OptionD { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phải chọn đáp án đúng")]
        public string CorrectAnswer { get; set; } = string.Empty; // A, B, C, hoặc D

        public int Order { get; set; } = 0;
    }
}

