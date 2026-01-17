using System.ComponentModel.DataAnnotations;

namespace CommonLib.Entities
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(10000)]
        public string? ReadingContent { get; set; } // Nội dung bài đọc chi tiết

        [StringLength(10)]
        public string? ReadingLevel { get; set; } // Cấp độ: A1, A2, B1, B2, C1, C2

        // Listening fields
        [StringLength(1000)]
        public string? AudioUrl { get; set; } // URL file audio hoặc link Soundcloud/Drive

        [StringLength(20000)]
        public string? Transcript { get; set; } // Bản gỡ băng (Rich Text)

        public bool HideTranscript { get; set; } = false; // Ẩn transcript khi làm bài

        public int? PlayLimit { get; set; } // Số lần được nghe tối đa (null = không giới hạn)

        public double DefaultSpeed { get; set; } = 1.0; // Tốc độ phát mặc định (0.5, 1.0, 1.5)

        // Speaking fields
        [StringLength(500)]
        public string? Topic { get; set; } // Chủ đề bài nói

        [StringLength(10000)]
        public string? ReferenceText { get; set; } // Đoạn văn mẫu (có thể tạo tự động bằng AI)

        [StringLength(10)]
        public string? SpeakingLevel { get; set; } // Cấp độ: A1, A2, B1, B2, C1, C2

        public int TimeLimitSeconds { get; set; } = 60; // Thời gian giới hạn (giây)

        // Writing fields
        [StringLength(500)]
        public string? WritingTopic { get; set; } // Chủ đề bài viết

        [StringLength(5000)]
        public string? WritingPrompt { get; set; } // Đề bài Writing Task 2

        [StringLength(5000)]
        public string? WritingHints { get; set; } // Các gợi ý từ AI để phát triển ý tưởng

        [StringLength(10)]
        public string? WritingLevel { get; set; } // Cấp độ Writing: A1, A2, B1, B2, C1, C2

        public int? WordLimit { get; set; } // Giới hạn số từ (null = không giới hạn)

        public int? TimeLimitMinutes { get; set; } // Giới hạn thời gian làm bài (phút)

        [Required]
        public int SkillId { get; set; }
        public Skill? Skill { get; set; }

        public int Level { get; set; } = 1; // 1: Beginner, 2: Intermediate, 3: Advanced

        public int Order { get; set; } = 0; // Thứ tự hiển thị

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}

