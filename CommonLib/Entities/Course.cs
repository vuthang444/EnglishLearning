namespace CommonLib.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Level { get; set; } = "B2";
        /// <summary>JSON: modules and lessons structure</summary>
        public string? Syllabus { get; set; }
        public string? TargetAudience { get; set; }
        public string? MarketingCopy { get; set; }
        public decimal PriceUSD { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

