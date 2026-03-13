namespace CommonLib.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced
        public string? SourceUrl { get; set; }
        public string? Topic { get; set; }
        /// <summary>Đường dẫn ảnh đại diện (relative, ví dụ: /uploads/news/xxx.jpg)</summary>
        public string? FeaturedImagePath { get; set; }
        public bool IsPublished { get; set; } = false;
        public bool IsFreePreview { get; set; } = true; // Free users có thể đọc
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public List<NewsVocabulary> Vocabularies { get; set; } = new List<NewsVocabulary>();
    }

    public class NewsVocabulary
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public News? News { get; set; }
        public string Word { get; set; } = string.Empty;
        public string VietnameseMeaning { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
