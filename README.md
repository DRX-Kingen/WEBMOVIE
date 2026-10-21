# WEBMOVIE

Website xem phim gồm frontend React và backend ASP.NET Core Web API. Dự án hỗ trợ duyệt và tìm kiếm phim, xem video, đăng ký/đăng nhập, bình luận và giao diện quản trị.

## Framework và công nghệ

| Thành phần | Công nghệ sử dụng |
| --- | --- |
| Frontend | React 19, TypeScript 6, Vite 8 |
| Điều hướng | React Router DOM 7 |
| Gọi API | Axios |
| Phát video HLS | hls.js |
| Biểu đồ và biểu tượng | Recharts, Lucide React |
| Backend | ASP.NET Core Web API, .NET 10 |
| Truy cập dữ liệu | Entity Framework Core 10 |
| Xác thực và phân quyền | ASP.NET Core Identity, JWT Bearer |
| Cơ sở dữ liệu | Microsoft SQL Server |
| Nguồn phim bên ngoài | API VSMOV |

## Cấu trúc dự án

```text
WEBMOVIE/
├── readme.md
├── movie-frontend/                 # Giao diện React
│   ├── src/api/                    # Gọi backend
│   ├── src/components/             # Thành phần giao diện
│   ├── src/pages/                  # Các trang của website
│   ├── .env                        # Địa chỉ API
│   └── package.json
└── MovieStreaming/
    ├── MovieStreaming.slnx
    └── MovieStreaming/             # Backend ASP.NET Core
        ├── Controllers/
        ├── Data/
        ├── Migrations/
        ├── Models/
        ├── Services/
        ├── Program.cs
        └── appsettings.Development.json
```

## Yêu cầu trước khi chạy

- Node.js 22.12 trở lên và npm.
- .NET SDK 10.
- SQL Server hoặc SQL Server Express đang hoạt động.
- Quyền truy cập và tạo/cập nhật database trên SQL Server.
- Kết nối Internet để tải thư viện và gọi API phim bên ngoài.

Kiểm tra các công cụ:

```powershell
node --version
npm --version
dotnet --version
```

Hướng dẫn dưới đây sử dụng PowerShell. Mỗi terminal bắt đầu tại thư mục gốc dự án (`D:\WEBMOVIE` nếu giữ vị trí hiện tại).

## 1. Cấu hình backend

Đi đến thư mục backend và khôi phục thư viện:

```powershell
cd .\MovieStreaming\MovieStreaming
dotnet restore
```

### Kết nối SQL Server

Dự án đã khai báo `UserSecretsId`, có thể dùng User Secrets để cấu hình máy phát triển. Thay `localhost\SQLEXPRESS` bằng tên server/instance trên máy của bạn:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=MovieStreamingDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

Chuỗi kết nối trên dùng Windows Authentication. Nếu dùng SQL Authentication, thay bằng chuỗi kết nối có tài khoản SQL Server phù hợp.

File `appsettings.Development.json` hiện chứa tên SQL Server của máy phát triển ban đầu. User Secrets sẽ ghi đè giá trị này trong môi trường Development.

Backend hiện sử dụng `ApplicationDbContext` với `DefaultConnection` cho cả dữ liệu phim và Identity. Không cần tạo database riêng theo `AuthConnection` cho cấu hình đang chạy.

### Cấu hình JWT

Chạy trong cùng thư mục backend:

```powershell
dotnet user-secrets set "Jwt:Key" "local-development-example-key-change-this-32-bytes-minimum"
dotnet user-secrets set "Jwt:Issuer" "MovieStreaming"
dotnet user-secrets set "Jwt:Audience" "MovieStreamingFrontend"
```

`Jwt:Key` phải dài tối thiểu 32 byte UTF-8. Khóa trên chỉ là ví dụ chạy local; thay bằng khóa ngẫu nhiên riêng khi triển khai và không commit khóa thật vào Git.

Địa chỉ API phim đã được khai báo trong `appsettings.Development.json`:

```json
{
  "Vsmov": {
    "BaseUrl": "https://vsmov.com/api/"
  }
}
```

### Tạo cơ sở dữ liệu

Nếu chưa cài công cụ EF Core:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Nếu đã cài, kiểm tra bằng `dotnet ef --version` và dùng phiên bản 10.x phù hợp. Áp dụng migrations có sẵn:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --context ApplicationDbContext
```

Cần hoàn thành bước này trước khi chạy backend vì ứng dụng truy cập database để khởi tạo vai trò lúc khởi động. Migrations tạo cấu trúc database, không đảm bảo có dữ liệu phim mẫu.

## 2. Chạy backend

Trong thư mục `MovieStreaming/MovieStreaming`:

```powershell
dotnet dev-certs https --trust
dotnet run --launch-profile https
```

Chấp nhận hộp thoại tin cậy chứng chỉ phát triển nếu hệ điều hành hiển thị. Giữ terminal này mở.

| Dịch vụ | Địa chỉ |
| --- | --- |
| API HTTPS | https://localhost:7032 |
| HTTP trong launch profile | http://localhost:5036 |

Backend bật chuyển hướng HTTPS. Dùng profile `https` để khớp cấu hình frontend. Đường dẫn `/` của backend có thể trả về 404 vì backend chỉ cung cấp các endpoint controller, không phục vụ trang chủ.

## 3. Chạy frontend

Mở terminal thứ hai từ thư mục gốc dự án:

```powershell
cd .\movie-frontend
npm ci
```

Tạo hoặc kiểm tra file `movie-frontend/.env`:

```dotenv
VITE_API_URL=https://localhost:7032
```

Khởi động giao diện:

```powershell
npm run dev -- --port 5173 --strictPort
```

Truy cập **http://localhost:5173** để sử dụng website. Cần giữ cả backend và frontend đang chạy. Khởi động lại Vite sau khi thay đổi `.env`.

Backend hiện chỉ cho phép CORS từ `http://localhost:5173`. Nếu thay đổi cổng hoặc hostname frontend, cập nhật `WithOrigins(...)` trong `MovieStreaming/MovieStreaming/Program.cs` rồi khởi động lại backend.

## 4. Đăng ký và cấp quyền Admin

1. Mở `/register` trên frontend để tạo tài khoản. Mật khẩu cần ít nhất 8 ký tự, có chữ hoa, chữ thường và chữ số.
2. Đăng nhập tại `/login`.
3. Để cấp quyền Admin, dừng backend và cấu hình email của tài khoản đã đăng ký trong thư mục backend:

```powershell
dotnet user-secrets set "BootstrapAdmin:Email" "email-da-dang-ky@example.com"
dotnet run --launch-profile https
```

Chức năng cấp quyền này chỉ chạy trong Development. Nếu email chưa tồn tại, backend sẽ báo lỗi khi khởi động. Sau khi cấp quyền thành công, có thể xóa cấu hình:

```powershell
dotnet user-secrets remove "BootstrapAdmin:Email"
```

Đăng xuất và đăng nhập lại để nhận token có quyền mới. Trang quản trị: `/admin/dashboard`.

## Các lệnh thường dùng

Chạy tại `movie-frontend`:

| Lệnh | Chức năng |
| --- | --- |
| `npm run dev` | Chạy giao diện ở chế độ phát triển |
| `npm run build` | Kiểm tra TypeScript và build vào `dist/` |
| `npm run preview` | Xem thử bản frontend đã build |
| `npm run lint` | Kiểm tra mã nguồn bằng Oxlint |

Chạy tại `MovieStreaming/MovieStreaming`:

| Lệnh | Chức năng |
| --- | --- |
| `dotnet restore` | Khôi phục thư viện NuGet |
| `dotnet build` | Biên dịch backend |
| `dotnet run --launch-profile https` | Chạy backend HTTPS |
| `dotnet ef database update --context ApplicationDbContext` | Cập nhật cấu trúc database |

`npm run preview` vẫn cần backend riêng. Nếu preview sử dụng origin khác cổng 5173, cần cho phép origin đó trong CORS. Khi triển khai thực tế, cấu hình lại địa chỉ API, CORS, HTTPS, JWT và database theo môi trường triển khai.

## Lỗi thường gặp

| Lỗi | Cách kiểm tra/xử lý |
| --- | --- |
| Không kết nối được SQL Server | Kiểm tra dịch vụ SQL Server, tên instance, quyền và `DefaultConnection` |
| Thiếu bảng hoặc lỗi khởi tạo role | Chạy migrations với `ApplicationDbContext` trong Development |
| Thiếu cấu hình JWT | Thiết lập đủ `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` trong thư mục backend |
| Khóa JWT quá ngắn | Dùng khóa ít nhất 32 byte UTF-8 |
| Lỗi chứng chỉ HTTPS | Chạy `dotnet dev-certs https --trust` và tin cậy chứng chỉ |
| Frontend báo Network Error | Kiểm tra backend đang chạy và `VITE_API_URL=https://localhost:7032` |
| Lỗi CORS | Dùng đúng `http://localhost:5173` hoặc cập nhật `WithOrigins(...)` |
| Cổng 5173 bị chiếm | Dừng tiến trình chiếm cổng hoặc đổi cổng đồng thời cập nhật CORS |
| Không tải được phim bên ngoài | Kiểm tra Internet, `Vsmov:BaseUrl` và phản hồi của API VSMOV |
