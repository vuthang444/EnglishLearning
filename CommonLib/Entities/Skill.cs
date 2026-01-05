using System.ComponentModel.DataAnnotations;

namespace CommonLib.Entities
{
    public class Skill
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Listening, Speaking, Reading, Writing

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

