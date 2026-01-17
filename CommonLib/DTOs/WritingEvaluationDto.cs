namespace CommonLib.DTOs
{
    public class WritingEvaluationDto
    {
        public double OverallScore { get; set; } // Điểm tổng (0-100)

        // Task Response (0-25)
        public double TaskResponseScore { get; set; }
        public string TaskResponseFeedback { get; set; } = string.Empty;

        // Coherence and Cohesion (0-25)
        public double CoherenceScore { get; set; }
        public string CoherenceFeedback { get; set; } = string.Empty;

        // Lexical Resource (0-25)
        public double LexicalScore { get; set; }
        public string LexicalFeedback { get; set; } = string.Empty;
        public List<string> SuggestedVocabulary { get; set; } = new List<string>(); // Từ vựng học thuật gợi ý

        // Grammar (0-25)
        public double GrammarScore { get; set; }
        public string GrammarFeedback { get; set; } = string.Empty;
        public List<GrammarError> GrammarErrors { get; set; } = new List<GrammarError>();

        // Tone assessment
        public string ToneFeedback { get; set; } = string.Empty;

        // General feedback
        public string GeneralFeedback { get; set; } = string.Empty;

        public int WordCount { get; set; } // Số từ trong bài viết
    }

    public class GrammarError
    {
        public string Text { get; set; } = string.Empty; // Đoạn text có lỗi
        public string Correction { get; set; } = string.Empty; // Cách sửa
        public string Reason { get; set; } = string.Empty; // Lý do lỗi
        public int? StartIndex { get; set; } // Vị trí bắt đầu trong text (optional)
        public int? EndIndex { get; set; } // Vị trí kết thúc trong text (optional)
    }
}


