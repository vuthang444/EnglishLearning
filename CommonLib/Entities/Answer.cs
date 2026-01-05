using System.ComponentModel.DataAnnotations;

namespace CommonLib.Entities
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }

        public bool IsCorrect { get; set; } = false;

        public int Order { get; set; } = 0;
    }
}

