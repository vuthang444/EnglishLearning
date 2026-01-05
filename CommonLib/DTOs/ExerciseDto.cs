using System.ComponentModel.DataAnnotations;
using CommonLib.Entities;

namespace CommonLib.DTOs
{
    public class ExerciseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Câu hỏi là bắt buộc")]
        [StringLength(500, ErrorMessage = "Câu hỏi không được vượt quá 500 ký tự")]
        public string Question { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Nội dung không được vượt quá 2000 ký tự")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Bài học là bắt buộc")]
        public int LessonId { get; set; }

        public ExerciseType Type { get; set; } = ExerciseType.MultipleChoice;

        public int Order { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
    }

    public class AnswerDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung câu trả lời là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự")]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;

        public int Order { get; set; } = 0;
    }
}

