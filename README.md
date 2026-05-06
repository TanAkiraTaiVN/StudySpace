# StudySpace

**Hệ thống quản lý học nhóm và tài liệu đa nền tảng** — gồm REST API, website quản trị và ứng dụng di động .NET MAUI dùng chung C#.

## Tính năng

- **Xác thực &amp; phân quyền** — JWT, BCrypt password hashing, vai trò `Admin` / `Leader` / `Member`.
- **Nhóm học** — tạo nhóm, mã mời, thành viên, phân quyền trưởng nhóm.
- **Tài liệu** — upload tệp lên nhóm, tải xuống, sửa metadata, đếm lượt tải.
- **Lịch học** — lịch theo nhóm, link online, nhắc lịch sắp tới.
- **Trò chuyện nhóm** — gửi/nhận tin nhắn, soft-delete.
- **Thông báo** — tự động khi có tài liệu mới / lịch mới, đánh dấu đã đọc.
- **Tiến độ học tập** — mục tiêu cá nhân theo nhóm, % hoàn thành, tổng kết theo nhóm.
- **Dashboard quản trị** — thống kê người dùng, nhóm, tài liệu, hoạt động 7 ngày.

## Kiến trúc

```
StudySpace.sln
├── StudySpace.Core/             # Entities, DTOs, Enums, Interfaces
├── StudySpace.Infrastructure/   # EF Core DbContext, Services
├── StudySpace.API/              # ASP.NET Core Web API + Admin Web UI (wwwroot)
└── StudySpace.Mobile/           # .NET MAUI app (Android/iOS/Windows)
```

- Backend: ASP.NET Core 8, EF Core 8 (SQLite dev / SQL Server prod)
- Auth: JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)
- Password: BCrypt.Net-Next
- API docs: Swagger / Swashbuckle
- Admin UI: HTML + CSS + Vanilla JS (phục vụ tĩnh từ `wwwroot/`)
- Mobile: .NET MAUI 8 (XAML + MVVM)

## Yêu cầu môi trường

- .NET 8 SDK ([install](https://dotnet.microsoft.com/download/dotnet/8.0))
- SQLite (đi kèm runtime) hoặc SQL Server (cho production)
- (Tuỳ chọn) Visual Studio 2022 với MAUI workload nếu build mobile
- (Tuỳ chọn) Android SDK nếu build app Android từ CLI

## Chạy backend + admin UI (local, SQLite)

```bash
git clone https://github.com/TanAkiraTaiVN/StudySpace.git
cd StudySpace
dotnet restore
dotnet build StudySpace.sln
cd StudySpace.API
dotnet run --urls "http://0.0.0.0:5000"
```

Truy cập:
- Admin UI: http://localhost:5000/
- Swagger: http://localhost:5000/swagger/

Tài khoản demo (được seed sẵn):

| Email | Mật khẩu | Vai trò |
| --- | --- | --- |
| `admin@studyspace.com` | `Admin@123` | Admin |
| `leader@studyspace.com` | `Admin@123` | Leader |
| `student1@studyspace.com` | `Admin@123` | Member |
| `student2@studyspace.com` | `Admin@123` | Member |

## Chuyển sang SQL Server cho production

Sửa `StudySpace.API/appsettings.json` (xem mẫu `appsettings.Production.example.json`):

```json
{
  "Database": { "Provider": "sqlserver" },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudySpace;User Id=sa;Password=YourPwd;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_LONG_RANDOM_KEY_AT_LEAST_32_CHARS"
  }
}
```

Schema được auto-tạo khi khởi động (`EnsureCreatedAsync`). Cho production thật bạn có thể đổi sang EF migrations:

```bash
dotnet ef migrations add Init --project StudySpace.Infrastructure --startup-project StudySpace.API
dotnet ef database update --project StudySpace.Infrastructure --startup-project StudySpace.API
```

## Chạy app .NET MAUI (StudySpace.Mobile)

App MAUI **không** được thêm vào `StudySpace.sln` để CI chạy không cần Android SDK. Cách build:

```bash
# Cài workload (chỉ cần làm 1 lần)
dotnet workload install maui-android   # Windows/Linux
# hoặc dotnet workload install maui    # macOS (đầy đủ Android + iOS + MacCatalyst)

# Build (Android)
cd StudySpace.Mobile
dotnet build -f net8.0-android
```

Hoặc mở `StudySpace.Mobile/StudySpace.Mobile.csproj` trong Visual Studio 2022 (đã cài MAUI workload) và bấm Run.

Trong app, đặt **API URL** ở màn hình đăng nhập:
- Android emulator: `http://10.0.2.2:5000` (mặc định)
- iOS simulator: `http://localhost:5000`
- Thiết bị thật: `http://<IP-máy-chạy-API>:5000`

## API tóm tắt

| Module | Endpoint chính |
| --- | --- |
| Auth | `POST /api/auth/login`, `POST /api/auth/register`, `GET /api/auth/me`, `PUT /api/auth/me`, `POST /api/auth/change-password` |
| Users (admin) | `GET /api/users`, `PUT /api/users/{id}/role`, `PUT /api/users/{id}/active` |
| Groups | `GET /api/groups`, `GET /api/groups/mine`, `POST /api/groups`, `PUT /api/groups/{id}`, `DELETE /api/groups/{id}`, `POST /api/groups/join`, `POST /api/groups/{id}/leave`, `GET /api/groups/{id}/members` |
| Documents | `GET /api/documents/by-group/{groupId}`, `POST /api/documents/upload` (multipart), `GET /api/documents/{id}/download`, `PUT /api/documents/{id}`, `DELETE /api/documents/{id}` |
| Schedules | `GET /api/schedules/by-group/{groupId}`, `GET /api/schedules/upcoming`, `POST /api/schedules`, `PUT /api/schedules/{id}`, `DELETE /api/schedules/{id}` |
| Chat | `GET /api/chat/by-group/{groupId}`, `POST /api/chat/send`, `DELETE /api/chat/{id}` |
| Notifications | `GET /api/notifications`, `GET /api/notifications/count-unread`, `PUT /api/notifications/{id}/read`, `PUT /api/notifications/read-all` |
| Progress | `GET /api/progress/mine`, `GET /api/progress/by-group/{groupId}`, `GET /api/progress/group/{groupId}/summary`, `POST /api/progress`, `PUT /api/progress/{id}`, `DELETE /api/progress/{id}` |
| Dashboard (admin) | `GET /api/dashboard/summary` |

Mọi response (trừ download tệp) dùng wrapper:

```json
{ "success": true, "message": "...", "data": <T> }
```

## Cấu trúc dữ liệu

8 bảng chính: `Users`, `StudyGroups`, `GroupMembers`, `Documents`, `Schedules`, `ChatMessages`, `Notifications`, `ProgressEntries`.

## License

MIT
