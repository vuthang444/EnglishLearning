namespace CommonLib.DTOs
{
    public class NewsGenerationResult
    {
        public string Title { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate";
        public string? Topic { get; set; }
        public List<VocabularyItem> Vocabularies { get; set; } = new List<VocabularyItem>();
    }

    public class VocabularyItem
    {
        public string Word { get; set; } = string.Empty;
        public string VietnameseMeaning { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
    }

    public class NewsSummaryDto
    {
        public string Summary { get; set; } = string.Empty;
    }

    public class GrammarExplanationDto
    {
        public List<GrammarPoint> GrammarPoints { get; set; } = new List<GrammarPoint>();
    }

    public class GrammarPoint
    {
        public string Structure { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
    }
}
