using System.ComponentModel.DataAnnotations;

namespace CommonLib.Entities
{
    public class Exercise
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Content { get; set; } // Nội dung bài tập (audio URL, text, etc.)

        [Required]
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public ExerciseType Type { get; set; } = ExerciseType.MultipleChoice;

        public int Order { get; set; } = 0;

        [StringLength(20)]
        public string? Timestamp { get; set; } // Thời gian trong audio (format: "MM:SS" hoặc "HH:MM:SS")

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }

    public enum ExerciseType
    {
        MultipleChoice = 1,  // Trắc nghiệm
        FillInBlank = 2,      // Điền vào chỗ trống
        TrueFalse = 3,        // Đúng/Sai
        Essay = 4,            // Tự luận
        Audio = 5,            // Nghe
        Speaking = 6          // Nói
    }
}

