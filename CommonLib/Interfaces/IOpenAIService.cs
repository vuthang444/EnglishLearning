using CommonLib.DTOs;

namespace CommonLib.Interfaces
{
    public interface IOpenAIService
    {
        /// <summary>
        /// Tạo nội dung bài tập Speaking tự động từ chủ đề
        /// </summary>
        Task<SpeakingContentGenerationResult> GenerateSpeakingContentAsync(string topic, string level);

        /// <summary>
        /// Chuyển đổi audio thành text bằng Whisper
        /// </summary>
        Task<string> TranscribeAudioAsync(Stream audioStream, string fileName);

        /// <summary>
        /// Chấm điểm bài nói bằng GPT-4o
        /// </summary>
        Task<SpeakingEvaluationDto> EvaluateSpeakingAsync(string studentTranscription, string referenceText);

        /// <summary>
        /// Tạo đề bài Writing Task 2 tự động từ chủ đề
        /// </summary>
        Task<WritingContentGenerationResult> GenerateWritingPromptAsync(string topic, string level);

        /// <summary>
        /// Chấm điểm bài viết bằng GPT-4o với rubric chi tiết
        /// </summary>
        Task<WritingEvaluationDto> EvaluateWritingAsync(string studentEssay, string writingPrompt, string? hints = null);

        /// <summary>AI thiết kế khóa học: syllabus, target audience, marketing copy, pricing.</summary>
        Task<CourseDesignResult> GenerateCourseDesignAsync(string topic, string level);

        /// <summary>AI tư vấn lộ trình: đề xuất khóa học dựa trên điểm Speaking, Writing và mục tiêu.</summary>
        Task<string> GetCourseRecommendationsAsync(int speakingScore, int writingScore, string userGoal, string courseListText);
    }

    public class SpeakingContentGenerationResult
    {
        public string Title { get; set; } = string.Empty;
        public string Passage { get; set; } = string.Empty;
    }

    public class WritingContentGenerationResult
    {
        public string Title { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Hints { get; set; } = string.Empty; // Gợi ý để phát triển ý tưởng
    }
}





