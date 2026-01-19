namespace CommonLib.DTOs
{
    public class DictionaryResultDto
    {
        public string Word { get; set; } = string.Empty;
        public string PartOfSpeech { get; set; } = string.Empty; // noun, verb, adjective, etc.
        public string Phonetic { get; set; } = string.Empty; // /'pæk-1d3/
        public string EnglishDefinition { get; set; } = string.Empty;
        public string VietnameseTranslation { get; set; } = string.Empty;
        public string CommonMeaning { get; set; } = string.Empty; // Nghĩa phổ thông
        public string CefrLevel { get; set; } = string.Empty; // A1, A2, B1, B2, C1, C2
        public List<DictionaryExampleDto> Examples { get; set; } = new List<DictionaryExampleDto>();
        public List<string> Synonyms { get; set; } = new List<string>();
        public List<string> Antonyms { get; set; } = new List<string>();
    }

    public class DictionaryExampleDto
    {
        public string EnglishSentence { get; set; } = string.Empty;
        public string VietnameseTranslation { get; set; } = string.Empty;
    }

    public class WordOfTheDayDto
    {
        public string Word { get; set; } = string.Empty;
        public string Phonetic { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
    }
}
