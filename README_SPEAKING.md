# Module Speaking - Hướng dẫn cài đặt và sử dụng

## Tổng quan
Module Speaking cho phép:
- **Admin**: Tạo bài tập Speaking với tính năng tạo nội dung tự động bằng AI
- **Học viên**: Thu âm bài nói và được chấm điểm tự động bằng AI (Whisper + GPT-4o)

## Cài đặt

### 1. Cấu hình OpenAI API Key
Thêm API Key vào file `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here"
  }
}
```

**Lưu ý**: 
- Lấy API Key tại: https://platform.openai.com/api-keys
- Đảm bảo API Key có quyền truy cập GPT-4o và Whisper

### 2. Chạy Migration
```bash
dotnet ef database update --project DataServiceLib --startup-project EnglishLearning
```

## Tính năng

### Admin Panel
- **Tạo bài nói mới**: `/Admin/Speaking/Create`
  - Nhập chủ đề và nhấn "Tạo nội dung tự động bằng AI" để AI tạo văn bản mẫu
  - Hoặc nhập thủ công văn bản mẫu
  - Cấu hình thời gian giới hạn, cấp độ, v.v.

- **Quản lý bài nói**: `/Admin/Speaking`
  - Xem danh sách, sửa, xóa bài nói

### User Panel
- **Danh sách bài tập**: `/Speaking`
  - Xem tất cả bài tập Speaking
  - Xem điểm số nếu đã làm

- **Làm bài tập**: `/Speaking/Exercise/{lessonId}`
  - Đọc văn bản mẫu
  - Nhấn "Bắt đầu ghi âm" để thu âm
  - Nghe lại bản ghi trước khi nộp
  - Nhấn "Nộp bài" để AI chấm điểm tự động

- **Xem kết quả**: `/Speaking/Result/{submissionId}`
  - Xem điểm số (Accuracy, Fluency, Overall Score)
  - Xem các từ phát âm sai (highlighted)
  - Xem phản hồi từ AI

## Công nghệ sử dụng
- **Backend**: ASP.NET Core MVC, Entity Framework Core
- **AI**: OpenAI API (GPT-4o cho chấm điểm, Whisper cho transcription)
- **Frontend**: Web Audio API (MediaRecorder) để thu âm từ browser
- **Database**: SQL Server

## Lưu ý
- Cần có microphone để thu âm
- Trình duyệt cần hỗ trợ MediaRecorder API (Chrome, Edge, Firefox mới)
- API Key OpenAI cần có credit để sử dụng



