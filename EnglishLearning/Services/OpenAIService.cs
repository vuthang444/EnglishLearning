using CommonLib.DTOs;
using CommonLib.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EnglishLearning.Services
{
    public class OpenAIService : CommonLib.Interfaces.IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openai.com/v1";

        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger, IHttpClientFactory httpClientFactory)
        {
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key không được cấu hình. Vui lòng thêm 'OpenAI:ApiKey' vào appsettings.json");
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _logger = logger;
        }

        public async Task<SpeakingContentGenerationResult> GenerateSpeakingContentAsync(string topic, string level)
        {
            try
            {
                var prompt = $@"Bạn là chuyên gia biên soạn giáo trình tiếng Anh. Dựa trên chủ đề '{topic}', hãy tạo một đoạn văn mẫu dài khoảng 50 từ dành cho trình độ {level}. Yêu cầu từ vựng thông dụng và cấu trúc câu tự nhiên. Trả về định dạng JSON: {{ ""title"": ""..."", ""passage"": ""..."" }}";

                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là chuyên gia biên soạn giáo trình tiếng Anh. Luôn trả về JSON hợp lệ." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                var aiContent = responseJson.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

                // Parse JSON response
                try
                {
                    aiContent = Regex.Replace(aiContent, @"```json\s*", "", RegexOptions.IgnoreCase);
                    aiContent = Regex.Replace(aiContent, @"```\s*", "", RegexOptions.IgnoreCase);
                    aiContent = aiContent.Trim();

                    var result = JsonSerializer.Deserialize<SpeakingContentGenerationResult>(aiContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Title) && !string.IsNullOrEmpty(result.Passage))
                    {
                        return result;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Không thể parse JSON từ OpenAI response: {Content}", aiContent);
                }

                // Fallback: Extract title and passage manually
                var titleMatch = Regex.Match(aiContent, @"""title""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);
                var passageMatch = Regex.Match(aiContent, @"""passage""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase);

                return new SpeakingContentGenerationResult
                {
                    Title = titleMatch.Success ? titleMatch.Groups[1].Value : $"Speaking about {topic}",
                    Passage = passageMatch.Success ? passageMatch.Groups[1].Value : aiContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo nội dung Speaking bằng AI");
                throw;
            }
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string fileName)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
                using var memoryStream = new MemoryStream();
                await audioStream.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();

                formData.Add(new ByteArrayContent(audioBytes), "file", fileName);
                formData.Add(new StringContent("whisper-1"), "model");
                formData.Add(new StringContent("en"), "language");

                var response = await _httpClient.PostAsync($"{_baseUrl}/audio/transcriptions", formData);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                return responseJson.RootElement.GetProperty("text").GetString() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chuyển đổi audio thành text");
                throw;
            }
        }

        public async Task<SpeakingEvaluationDto> EvaluateSpeakingAsync(string studentTranscription, string referenceText)
        {
            try
            {
                var prompt = $@"Hãy phân tích bản ghi âm sau đây. So sánh văn bản học viên nói (Transcription) với văn bản mẫu (Reference).

Văn bản học viên nói: {studentTranscription}

Văn bản mẫu: {referenceText}

Tính điểm phần trăm giống nhau (Accuracy).
Tìm ra những từ học viên nói sai (Mispronounced).
Nếu học viên ngập ngừng nhiều (uhm, ah), hãy trừ điểm Fluency.

Trả về phản hồi bằng tiếng Việt thân thiện, khích lệ.

Trả về định dạng JSON:
{{
  ""accuracy"": 85.5,
  ""fluency"": 80.0,
  ""overallScore"": 82.75,
  ""mispronouncedWords"": [""word1"", ""word2""],
  ""transcription"": ""{studentTranscription}"",
  ""hesitationCount"": 3,
  ""feedback"": ""Phản hồi bằng tiếng Việt...""
}}";

                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là chuyên gia chấm điểm Speaking tiếng Anh. Luôn trả về JSON hợp lệ với các trường: accuracy, fluency, overallScore, mispronouncedWords (mảng), transcription, hesitationCount, feedback." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                var aiContent = responseJson.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

                // Remove markdown code blocks if present
                aiContent = Regex.Replace(aiContent, @"```json\s*", "", RegexOptions.IgnoreCase);
                aiContent = Regex.Replace(aiContent, @"```\s*", "", RegexOptions.IgnoreCase);
                aiContent = aiContent.Trim();

                try
                {
                    var result = JsonSerializer.Deserialize<SpeakingEvaluationDto>(aiContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Không thể parse JSON từ OpenAI response: {Content}", aiContent);
                }

                // Fallback: Return basic evaluation
                return new SpeakingEvaluationDto
                {
                    Accuracy = 70.0,
                    Fluency = 70.0,
                    OverallScore = 70.0,
                    Transcription = studentTranscription,
                    Feedback = "Đã hoàn thành bài nói. Vui lòng thử lại để có kết quả chính xác hơn."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm điểm Speaking bằng AI");
                throw;
            }
        }

        public async Task<WritingContentGenerationResult> GenerateWritingPromptAsync(string topic, string level)
        {
            try
            {
                var prompt = $@"Đóng vai trò là chuyên gia soạn đề IELTS. Hãy tạo một đề bài Writing Task 2 về chủ đề '{topic}'. Yêu cầu: Đề bài rõ ràng, có các câu hỏi gợi ý để học viên phát triển ý tưởng. Trình độ: {level}.

Trả về định dạng JSON:
{{
  ""title"": ""Tiêu đề bài viết"",
  ""prompt"": ""Đề bài Writing Task 2 chi tiết"",
  ""hints"": ""Các gợi ý để phát triển ý tưởng, cấu trúc bài viết""
}}";

                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là chuyên gia soạn đề IELTS Writing Task 2. Luôn trả về JSON hợp lệ với các trường: title, prompt, hints." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                var aiContent = responseJson.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

                // Parse JSON response
                try
                {
                    aiContent = Regex.Replace(aiContent, @"```json\s*", "", RegexOptions.IgnoreCase);
                    aiContent = Regex.Replace(aiContent, @"```\s*", "", RegexOptions.IgnoreCase);
                    aiContent = aiContent.Trim();

                    var result = JsonSerializer.Deserialize<WritingContentGenerationResult>(aiContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Title) && !string.IsNullOrEmpty(result.Prompt))
                    {
                        return result;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Không thể parse JSON từ OpenAI response: {Content}", aiContent);
                }

                // Fallback
                return new WritingContentGenerationResult
                {
                    Title = $"Writing Task 2: {topic}",
                    Prompt = $"Write an essay about {topic}.",
                    Hints = "Consider the main points, provide examples, and structure your essay clearly."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đề bài Writing bằng AI");
                throw;
            }
        }

        public async Task<WritingEvaluationDto> EvaluateWritingAsync(string studentEssay, string writingPrompt, string? hints = null)
        {
            try
            {
                var rubricPrompt = @"Hãy kiểm tra bài viết sau. Đừng chỉ sửa lỗi chính tả, hãy tập trung vào:

1. Task Response (0-25 điểm): Bài viết đã trả lời đúng trọng tâm đề bài chưa? Có đủ ý tưởng và ví dụ không?

2. Coherence and Cohesion (0-25 điểm): Cấu trúc bài viết có logic không? Sử dụng từ nối có phù hợp không?

3. Lexical Resource (0-25 điểm): Từ vựng có đa dạng và học thuật không? Gợi ý những từ vựng học thuật hơn để thay thế các từ đơn giản.

4. Grammar (0-25 điểm): Tìm các lỗi về thì, mạo từ và cấu trúc câu. Đưa ra cách sửa và lý do.

5. Tone: Ngôn ngữ có phù hợp với văn phong bài viết (trang trọng/không trang trọng) không?

Đếm số từ trong bài viết.";

                var prompt = $@"{rubricPrompt}

Đề bài: {writingPrompt}
{(string.IsNullOrEmpty(hints) ? "" : $"\nGợi ý: {hints}")}

Bài viết của học viên:
{studentEssay}

Trả về định dạng JSON:
{{
  ""overallScore"": 75.5,
  ""taskResponseScore"": 20.0,
  ""taskResponseFeedback"": ""Phản hồi về Task Response..."",
  ""coherenceScore"": 18.5,
  ""coherenceFeedback"": ""Phản hồi về Coherence..."",
  ""lexicalScore"": 19.0,
  ""lexicalFeedback"": ""Phản hồi về Lexical Resource..."",
  ""suggestedVocabulary"": [""academic word 1"", ""academic word 2""],
  ""grammarScore"": 18.0,
  ""grammarFeedback"": ""Phản hồi về Grammar..."",
  ""grammarErrors"": [
    {{
      ""text"": ""đoạn text có lỗi"",
      ""correction"": ""cách sửa"",
      ""reason"": ""lý do lỗi""
    }}
  ],
  ""toneFeedback"": ""Đánh giá về tone..."",
  ""generalFeedback"": ""Phản hồi tổng quan bằng tiếng Việt thân thiện, khích lệ..."",
  ""wordCount"": 250
}}";

                var requestBody = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new { role = "system", content = "Bạn là chuyên gia chấm điểm IELTS Writing Task 2. Luôn trả về JSON hợp lệ với đầy đủ các trường theo rubric IELTS." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                var aiContent = responseJson.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

                // Remove markdown code blocks if present
                aiContent = Regex.Replace(aiContent, @"```json\s*", "", RegexOptions.IgnoreCase);
                aiContent = Regex.Replace(aiContent, @"```\s*", "", RegexOptions.IgnoreCase);
                aiContent = aiContent.Trim();

                try
                {
                    var result = JsonSerializer.Deserialize<WritingEvaluationDto>(aiContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Không thể parse JSON từ OpenAI response: {Content}", aiContent);
                }

                // Fallback: Return basic evaluation
                return new WritingEvaluationDto
                {
                    OverallScore = 70.0,
                    TaskResponseScore = 17.5,
                    CoherenceScore = 17.5,
                    LexicalScore = 17.5,
                    GrammarScore = 17.5,
                    GeneralFeedback = "Đã hoàn thành bài viết. Vui lòng thử lại để có kết quả chính xác hơn.",
                    WordCount = studentEssay.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi chấm điểm Writing bằng AI");
                throw;
            }
        }

        public async Task<CourseDesignResult> GenerateCourseDesignAsync(string topic, string level)
        {
            try
            {
                var prompt = $@"Bạn là một chuyên gia thiết kế chương trình đào tạo (Instructional Designer). Tôi muốn tạo một khóa học về chủ đề '{topic}'. Hãy thiết kế cấu trúc khóa học cho trình độ {level} bao gồm:

Syllabus: Danh sách các chương (Modules) và các bài học (Lessons) bên trong.

Target Audience: Khóa học này phù hợp với ai?

Marketing Copy: Một đoạn giới thiệu ngắn (khoảng 200 từ) cực kỳ thu hút để bán khóa học này.

Pricing Suggestion: Đề xuất mức giá (USD) phù hợp với thị trường hiện nay.

Trả về kết quả dạng JSON. Định dạng: {{ ""courseTitle"": ""..."", ""syllabus"": {{ ""modules"": [ {{ ""name"": ""..."", ""lessons"": [""...""] }} ] }}, ""targetAudience"": ""..."", ""marketingCopy"": ""..."", ""pricingSuggestion"": 99.00 }}";

                var requestBody = new { model = "gpt-4o", messages = new[] { new { role = "system", content = "Trả về JSON: courseTitle, syllabus (object), targetAudience, marketingCopy, pricingSuggestion (số)." }, new { role = "user", content = prompt } }, temperature = 0.7 };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                var aiContent = responseJson.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
                aiContent = Regex.Replace(aiContent, @"```json\s*", "", RegexOptions.IgnoreCase);
                aiContent = Regex.Replace(aiContent, @"```\s*", "", RegexOptions.IgnoreCase);
                aiContent = aiContent.Trim();
                using var doc = JsonDocument.Parse(aiContent);
                var root = doc.RootElement;
                var result = new CourseDesignResult();
                if (root.TryGetProperty("courseTitle", out var jTitle)) result.CourseTitle = jTitle.GetString() ?? "";
                if (root.TryGetProperty("targetAudience", out var jTa)) result.TargetAudience = jTa.GetString() ?? "";
                if (root.TryGetProperty("marketingCopy", out var jMc)) result.MarketingCopy = jMc.GetString() ?? "";
                if (root.TryGetProperty("pricingSuggestion", out var jPrice) && jPrice.ValueKind == System.Text.Json.JsonValueKind.Number) result.PricingSuggestion = jPrice.GetDecimal();
                if (root.TryGetProperty("syllabus", out var jSyl)) result.Syllabus = (jSyl.ValueKind == System.Text.Json.JsonValueKind.Object || jSyl.ValueKind == System.Text.Json.JsonValueKind.Array) ? jSyl.GetRawText() : (jSyl.GetString() ?? "");
                if (string.IsNullOrEmpty(result.CourseTitle)) result.CourseTitle = $"Khóa học {topic} ({level})";
                return result;
            }
            catch (Exception ex) { _logger.LogError(ex, "Lỗi khi tạo thiết kế khóa học bằng AI"); throw; }
        }

        public async Task<string> GetCourseRecommendationsAsync(int speakingScore, int writingScore, string userGoal, string courseListText)
        {
            try
            {
                var prompt = $@"Dựa trên hồ sơ năng lực: Điểm Speaking: {speakingScore}/100, Điểm Writing: {writingScore}/100, Mục tiêu: {userGoal}. Hãy đóng vai trò chuyên gia tư vấn, phân tích điểm yếu và đề xuất 3 khóa học phù hợp nhất từ danh sách: {courseListText}. Giải thích rõ tại sao. Trả lời bằng tiếng Việt, thân thiện và khích lệ.";
                var requestBody = new { model = "gpt-4o", messages = new[] { new { role = "system", content = "Bạn là chuyên gia tư vấn lộ trình học tiếng Anh. Trả lời bằng tiếng Việt." }, new { role = "user", content = prompt } }, temperature = 0.7 };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(responseContent);
                return responseJson.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Không thể tạo tư vấn. Vui lòng thử lại.";
            }
            catch (Exception ex) { _logger.LogError(ex, "Lỗi khi tư vấn khóa học bằng AI"); throw; }
        }
    }
}
