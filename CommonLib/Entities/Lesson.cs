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

