using System.ComponentModel.DataAnnotations;

namespace CommonLib.DTOs
{
    public class SpeakingPassageDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public int SkillId { get; set; }

        public int Level { get; set; } = 1;

        public int Order { get; set; } = 0;

        // Speaking specific fields
        [Required]
        [StringLength(500)]
        public string Topic { get; set; } = string.Empty; // Chủ đề bài nói

        [StringLength(10000)]
        public string? ReferenceText { get; set; } // Đoạn văn mẫu (có thể tạo tự động bằng AI)

        [StringLength(10)]
        public string? DifficultyLevel { get; set; } // A1, A2, B1, B2, C1, C2

        public int TimeLimitSeconds { get; set; } = 60; // Thời gian giới hạn (giây)

        public bool IsActive { get; set; } = true;
    }

    public class SpeakingEvaluationDto
    {
        public double Accuracy { get; set; } // Độ chính xác (%)
        public double Fluency { get; set; } // Độ trôi chảy (%)
        public double OverallScore { get; set; } // Điểm tổng (%)
        public List<string> MispronouncedWords { get; set; } = new List<string>();
        public string Transcription { get; set; } = string.Empty; // Văn bản học viên nói (từ Whisper)
        public string Feedback { get; set; } = string.Empty; // Phản hồi từ AI
        public int HesitationCount { get; set; } // Số lần ngập ngừng (uhm, ah)
    }

    public class SpeakingExerciseDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string? ReferenceText { get; set; }
        public string? DifficultyLevel { get; set; }
        public int TimeLimitSeconds { get; set; }
    }
}





