# Module Writing - Hướng dẫn triển khai

## Tổng quan
Module Writing đã được triển khai đầy đủ với các tính năng:
- **Admin**: Tạo và quản lý đề bài Writing Task 2 với AI tự động
- **User**: Làm bài viết, nộp bài và xem kết quả với AI chấm điểm theo rubric IELTS
- **AI Integration**: GPT-4o để tạo đề bài và chấm điểm chi tiết

## Các thành phần đã triển khai

### 1. Entities & DTOs
- ✅ **Lesson.cs**: Thêm các trường Writing:
  - `WritingTopic` (string, max 500)
  - `WritingPrompt` (string, max 5000) - Đề bài Writing Task 2
  - `WritingHints` (string, max 5000) - Gợi ý từ AI
  - `WritingLevel` (string, max 10) - Cấp độ A1-C2
  - `WordLimit` (int?) - Giới hạn số từ
  - `TimeLimitMinutes` (int?) - Giới hạn thời gian (phút)

- ✅ **WritingPassageDto.cs**: DTO cho Admin tạo/sửa bài viết
- ✅ **WritingEvaluationDto.cs**: DTO cho kết quả chấm điểm AI
  - Task Response (0-25)
  - Coherence & Cohesion (0-25)
  - Lexical Resource (0-25)
  - Grammar (0-25)
  - Grammar Errors với text, correction, reason
  - Suggested Vocabulary
  - Tone Feedback
  - General Feedback

### 2. Services
- ✅ **IOpenAIService.cs**: Thêm 2 methods:
  - `GenerateWritingPromptAsync(topic, level)` - Tạo đề bài tự động
  - `EvaluateWritingAsync(essay, prompt, hints)` - Chấm điểm với rubric IELTS

- ✅ **OpenAIService.cs**: Implementation với prompts chi tiết:
  - Prompt cho Admin: "Đóng vai trò là chuyên gia soạn đề IELTS..."
  - Prompt cho chấm điểm: Rubric IELTS đầy đủ (Task Response, Coherence, Lexical, Grammar, Tone)

### 3. Controllers
- ✅ **Admin/WritingController.cs**: CRUD đầy đủ
  - `Index` - Danh sách bài viết
  - `Create` (GET/POST) - Tạo bài viết mới
  - `Edit` (GET/POST) - Sửa bài viết
  - `Detail` - Xem chi tiết
  - `Delete` - Xóa bài viết
  - `GenerateContent` (POST) - API tạo đề bài bằng AI

- ✅ **WritingController.cs** (User):
  - `Index` - Danh sách bài tập
  - `Exercise` - Làm bài viết
  - `Submit` (POST) - Nộp bài và chấm điểm bằng AI
  - `Result` - Xem kết quả chi tiết với highlight lỗi

### 4. Views - Admin
- ✅ **Index.cshtml**: Danh sách bài viết với thống kê
- ✅ **Create.cshtml**: Form tạo bài viết với nút "Tạo đề bài tự động bằng AI"
- ✅ **Edit.cshtml**: Form sửa bài viết
- ✅ **Detail.cshtml**: Xem chi tiết bài viết

### 5. Views - User
- ✅ **Index.cshtml**: Danh sách bài tập với progress indicator
- ✅ **Exercise.cshtml**: 
  - Cột trái: Đề bài và gợi ý (sticky)
  - Cột phải: TextArea viết bài với word count và timer
  - Nút nộp bài với loading status
- ✅ **Result.cshtml**: 
  - Score summary (Overall, Task Response, Coherence, Lexical & Grammar)
  - Bài viết với **highlight lỗi grammar** (màu đỏ, tooltip)
  - Danh sách lỗi grammar chi tiết
  - Feedback từng tiêu chí
  - Suggested vocabulary
  - Tone feedback
  - General feedback từ AI

### 6. Database
- ✅ **ApplicationDbContext.cs**: Đã cập nhật mapping cho các trường Writing
- ⚠️ **Migration**: Cần chạy migration khi ứng dụng không chạy

## Các bước triển khai

### Bước 1: Tạo Migration
```bash
# Dừng ứng dụng đang chạy trước
dotnet ef migrations add AddWritingFields --project DataServiceLib --startup-project EnglishLearning
```

### Bước 2: Cập nhật Database
```bash
dotnet ef database update --project DataServiceLib --startup-project EnglishLearning
```

### Bước 3: Cấu hình OpenAI API Key
Đảm bảo trong `appsettings.json` có:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key-here"
  }
}
```

### Bước 4: Kiểm tra
1. **Admin**: Vào `/Admin/Writing` → Tạo bài viết mới → Test nút "Tạo đề bài tự động"
2. **User**: Vào `/Writing` → Chọn bài tập → Viết bài → Nộp bài → Xem kết quả với highlight lỗi

## Tính năng nổi bật

### 1. AI Content Generation (Admin)
- Nhập chủ đề và cấp độ → AI tự động tạo:
  - Title
  - Writing Prompt (đề bài chi tiết)
  - Writing Hints (gợi ý phát triển ý tưởng)

### 2. AI Evaluation với Rubric IELTS (User)
- **Task Response** (0-25): Đánh giá việc trả lời đúng đề bài
- **Coherence & Cohesion** (0-25): Cấu trúc và từ nối
- **Lexical Resource** (0-25): Từ vựng học thuật
- **Grammar** (0-25): Ngữ pháp và cấu trúc câu
- **Grammar Errors**: Danh sách lỗi với:
  - Text có lỗi
  - Cách sửa
  - Lý do lỗi
- **Highlight lỗi**: Tự động highlight các từ/cụm từ có lỗi trong bài viết (màu đỏ, tooltip)

### 3. UI/UX
- **Exercise**: Layout 2 cột (đề bài bên trái sticky, viết bài bên phải)
- **Word Count**: Đếm số từ real-time
- **Timer**: Hiển thị thời gian làm bài
- **Result**: Hiển thị đầy đủ feedback với highlight lỗi trực quan

## Lưu ý
- Đảm bảo ứng dụng đã dừng trước khi chạy migration
- OpenAI API Key phải được cấu hình đúng
- Word limit và time limit có thể để null (không giới hạn)
- Grammar errors sẽ được highlight tự động trong bài viết khi xem kết quả

## Routes
- Admin: `/Admin/Writing`, `/Admin/Writing/Create`, `/Admin/Writing/Edit/{id}`, etc.
- User: `/Writing`, `/Writing/Exercise/{lessonId}`, `/Writing/Result/{submissionId}`, `/Writing/Submit`

