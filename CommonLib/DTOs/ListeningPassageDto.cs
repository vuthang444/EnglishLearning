using System.ComponentModel.DataAnnotations;

namespace CommonLib.DTOs
{
    public class ListeningPassageDto
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

        // Listening specific fields
        [StringLength(1000)]
        public string? AudioUrl { get; set; } // URL file audio hoặc link Soundcloud/Drive

        [StringLength(20000)]
        public string? Transcript { get; set; } // Bản gỡ băng (Rich Text)

        public bool HideTranscript { get; set; } = false; // Ẩn transcript khi làm bài

        public int? PlayLimit { get; set; } // Số lần được nghe tối đa (null = không giới hạn)

        [Range(0.5, 2.0)]
        public double DefaultSpeed { get; set; } = 1.0; // Tốc độ phát mặc định

        // Danh sách câu hỏi
        public List<ListeningQuestionDto> Questions { get; set; } = new List<ListeningQuestionDto>();
    }

    public class ListeningQuestionDto
    {
        [Required]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Timestamp { get; set; } // Thời gian trong audio (format: "MM:SS" hoặc "HH:MM:SS")

        public int Order { get; set; } = 0;

        // Danh sách đáp án
        public List<ListeningAnswerDto> Answers { get; set; } = new List<ListeningAnswerDto>();
    }

    public class ListeningAnswerDto
    {
        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;

        public int Order { get; set; } = 0;
    }
}

