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
        
        // Premium-only fields
        public string? ImprovedVersion { get; set; } // Bản mẫu hoàn hảo (Premium only)
        public List<string> SuggestedVocabulary { get; set; } = new List<string>(); // 5 từ vựng nâng cao (Premium only)
        public List<SpeakingDetailedError> DetailedErrors { get; set; } = new List<SpeakingDetailedError>(); // Lỗi chi tiết (Premium only)
    }

    public class SpeakingDetailedError
    {
        public string Text { get; set; } = string.Empty; // Đoạn text có lỗi
        public string Correction { get; set; } = string.Empty; // Cách sửa
        public string Reason { get; set; } = string.Empty; // Lý do lỗi
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





