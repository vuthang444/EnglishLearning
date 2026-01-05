# Hướng dẫn Migration Database

## Bước 1: Tạo Migration

Mở Terminal/PowerShell tại thư mục `EnglishLearning` và chạy lệnh:

```bash
dotnet ef migrations add InitialCreate --project ../DataServiceLib --startup-project .
```

## Bước 2: Cập nhật Database

```bash
dotnet ef database update --project ../DataServiceLib --startup-project .
```

## Bước 3: Tạo tài khoản Admin đầu tiên

Sau khi migration thành công, bạn có thể tạo tài khoản Admin thông qua giao diện đăng ký, sau đó cập nhật trực tiếp trong database:

```sql
UPDATE Users SET RoleId = 1 WHERE Username = 'your_admin_username'
```

Hoặc sử dụng SQL Server Management Studio để thay đổi RoleId từ 2 (User) sang 1 (Admin).

## Lưu ý

- Đảm bảo SQL Server LocalDB đã được cài đặt
- Connection string trong `appsettings.json` có thể cần được điều chỉnh tùy theo môi trường của bạn
- JWT SecretKey trong `appsettings.json` nên được thay đổi trong môi trường production

