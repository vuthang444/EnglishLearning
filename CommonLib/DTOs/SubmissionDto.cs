using System.ComponentModel.DataAnnotations;

namespace CommonLib.DTOs
{
    public class SubmissionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã người dùng là bắt buộc")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Mã bài tập là bắt buộc")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Danh sách đáp án là bắt buộc")]
        public List<UserAnswerDto> UserAnswers { get; set; } = new List<UserAnswerDto>();

        public int Score { get; set; } = 0;

        public int MaxScore { get; set; } = 0;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public int TimeSpentSeconds { get; set; } = 0;
    }

    public class UserAnswerDto
    {
        [Required(ErrorMessage = "Mã câu hỏi là bắt buộc")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Đáp án đã chọn là bắt buộc")]
        [StringLength(10)]
        public string SelectedAnswer { get; set; } = string.Empty; // A, B, C, hoặc D
    }

    public class ReadingExerciseDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ReadingContent { get; set; }
        public string? ReadingLevel { get; set; }
        public List<ReadingQuestionDisplayDto> Questions { get; set; } = new List<ReadingQuestionDisplayDto>();
        public int MaxScore { get; set; } = 0;
    }

    public class ReadingQuestionDisplayDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int Order { get; set; }
        public int Score { get; set; } = 1; // Điểm cho mỗi câu hỏi
    }

    public class SubmissionResultDto
    {
        public int SubmissionId { get; set; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage { get; set; }
        public int TimeSpentSeconds { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new List<QuestionResultDto>();
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string SelectedAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
    }
}

