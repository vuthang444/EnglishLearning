using System.ComponentModel.DataAnnotations;

namespace CommonLib.Entities
{
    public class Submission
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        // JSON string chứa danh sách đáp án: [{"questionId": 1, "selectedAnswer": "A"}, ...]
        [Required]
        [StringLength(5000)]
        public string AnswersJson { get; set; } = string.Empty;

        public int Score { get; set; } = 0; // Điểm số đạt được

        public int MaxScore { get; set; } = 0; // Điểm tối đa của bài tập

        public DateTime StartedAt { get; set; } = DateTime.UtcNow; // Thời gian bắt đầu làm bài

        public DateTime? CompletedAt { get; set; } // Thời gian hoàn thành

        public int TimeSpentSeconds { get; set; } = 0; // Thời gian làm bài (giây)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

