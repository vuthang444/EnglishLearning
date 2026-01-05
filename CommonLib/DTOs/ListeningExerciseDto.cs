namespace CommonLib.DTOs
{
    public class ListeningExerciseDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }
        public string? Transcript { get; set; }
        public bool HideTranscript { get; set; }
        public int? PlayLimit { get; set; }
        public double DefaultSpeed { get; set; } = 1.0;
        public int PlayCount { get; set; } = 0; // Số lần đã nghe (từ localStorage)
        public List<ListeningQuestionDisplayDto> Questions { get; set; } = new List<ListeningQuestionDisplayDto>();
    }

    public class ListeningQuestionDisplayDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string? Timestamp { get; set; }
        public int Order { get; set; }
        public List<ListeningAnswerDisplayDto> Answers { get; set; } = new List<ListeningAnswerDisplayDto>();
    }

    public class ListeningAnswerDisplayDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}

