namespace CommonLib.DTOs
{
    /// <summary>Kết quả AI thiết kế khóa học (Course Creator)</summary>
    public class CourseDesignResult
    {
        public string CourseTitle { get; set; } = string.Empty;
        /// <summary>JSON chuỗi cấu trúc syllabus (modules, lessons)</summary>
        public string Syllabus { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = string.Empty;
        public string MarketingCopy { get; set; } = string.Empty;
        public decimal PricingSuggestion { get; set; }
    }
}

