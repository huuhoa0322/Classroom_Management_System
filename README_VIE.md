# Hệ Thống Quản Lý Lớp Học (CLS)

Ứng dụng web full-stack phục vụ quản lý trung tâm dạy Tiếng Anh & Lập trình. CLS tập trung hóa toàn bộ vòng đời học viên, xếp lịch lớp học, điểm danh, tài chính và giao tiếp phụ huynh — thay thế các bảng tính rời rạc và quy trình thủ công.

> **Trạng thái:** MVP Phase 1 (13/04/2026 – 24/04/2026)

---

## 📋 Mục Lục

- [Tính năng](#-tính-năng)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Bắt đầu](#-bắt-đầu)
- [Biến môi trường](#-biến-môi-trường)
- [Triển khai](#-triển-khai)
- [Quy ước thiết kế API](#-quy-ước-thiết-kế-api)
- [Quy tắc code](#-quy-tắc-code)
- [Ghi chú](#-ghi-chú)

---

## ✨ Tính Năng

### Trang Quản Lý (Admin Portal)

| Module | Mô tả |
|--------|-------|
| **Tổng quan** | Tổng quan các chỉ số chính: tổng học viên, lớp đang hoạt động, thanh toán chờ xử lý, cảnh báo gia hạn |
| **Quản lý Học viên** | CRUD hồ sơ học viên (tên, ngày sinh, email, SĐT, địa chỉ); theo dõi lịch sử nhập học và thông tin phụ huynh |
| **Quản lý Lớp học** | Tạo/sửa lớp với cấu hình học phí, số buổi, hình thức; phân công giáo viên và quản lý danh sách học viên |
| **Quản lý Ca học** | Lên lịch các ca học trong tuần (Thứ 2 – Chủ Nhật); gắn lớp với phòng và giáo viên cho từng ca |
| **Quản lý Phòng** | Quản lý danh sách phòng với sức chứa và cấu hình thiết bị |
| **Quản lý Gói học phí** | Định nghĩa gói (tên, giá, số buổi, thời hạn); gán gói cho học viên khi đăng ký |
| **Quản lý Người dùng** | Quản lý tài khoản nhân viên (Admin / Teacher) với phân quyền; đặt lại mật khẩu, bật/tắt trạng thái |
| **Điểm danh** | Điểm danh theo buổi: Có mặt / Vắng / Đi trễ |
| **Nhận xét** | Giáo viên gửi nhận xét học thuật cho từng học viên theo buổi |
| **Quản lý Tài chính** | Xem thông tin tài chính học viên, ghi nhận và xác nhận thanh toán, quản lý vòng đời payment |
| **Thông báo Gia hạn** | Hệ thống tự động tạo thông báo khi gói học viên còn 30% / 10% số buổi; theo dõi trạng thái tư vấn |

### Trang Giáo Viên (Teacher Portal)

| Module | Mô tả |
|--------|-------|
| **Thời khóa biểu** | Lịch dạy cá nhân theo tuần với bộ lọc năm/tuần và điều hướng tuần trước/sau |
| **Điểm danh** | Điểm danh cho các buổi được phân công |
| **Nhận xét** | Gửi nhận xét học thuật cho từng học viên trong vòng 12 giờ (SLA) |

### Tính năng xuyên suốt

- **Xác thực JWT** với luồng access/refresh token
- **Routing phân quyền** — Admin → Dashboard, Teacher → Thời khóa biểu
- **Thông báo thời gian thực** qua SignalR
- **Email tự động** cho điểm danh và thay đổi lịch học (MailKit + SMTP)
- **Background services** — Quét cạn kiệt gói học và hàng đợi gửi email
- **Xử lý lỗi toàn cục** với banner lỗi kết nối và thông báo toast

---

## 🛠 Công nghệ sử dụng

### Frontend

| Công nghệ | Phiên bản | Mục đích |
|------------|-----------|----------|
| React | 19 | Thư viện UI |
| Vite | 8 | Build tool & dev server |
| TailwindCSS | 4 | CSS framework theo hướng utility-first |
| React Router | 7 | Routing phía client với guards |
| TanStack Query | 5 | Quản lý server state, caching và data fetching |
| Zustand | 5 | Global state phía client (auth, toast, notifications) |
| Axios | 1.x | HTTP client với interceptors (xử lý 401, retry) |
| React Hook Form + Zod | 7 / 4 | Quản lý form và validation schema |
| SignalR Client | 10 | Giao tiếp WebSocket thời gian thực |

### Backend

| Công nghệ | Phiên bản | Mục đích |
|------------|-----------|----------|
| ASP.NET Core | .NET 10 | Web API framework |
| Entity Framework Core | 10 | ORM với code-first migrations |
| PostgreSQL (Npgsql) | 16 | Cơ sở dữ liệu quan hệ |
| Serilog | 10 | Structured logging (Console + File sinks) |
| AutoMapper | 16 | Ánh xạ đối tượng (Entity ↔ DTO) |
| FluentValidation | 12 | Validation request |
| BCrypt.NET | 4 | Băm mật khẩu |
| MailKit | 4 | Gửi email qua SMTP |
| Scalar | 2.4 | Tài liệu API tương tác (OpenAPI) |
| SignalR | Built-in | Server push thời gian thực |

### Hạ tầng

| Công cụ | Mục đích |
|---------|----------|
| Docker | Container hóa backend |
| Docker Compose | Orchestration nhiều service trên local |
| Render.com | Hosting backend (IaC qua `render.yaml`) |
| Vercel | Hosting frontend |
| GitHub Actions | CI pipeline (build & test) |
| Supabase | PostgreSQL được quản lý (managed) |

---

## 🏗 Kiến trúc hệ thống

Backend theo **kiến trúc 3 lớp (3-layer architecture)**:

```
┌──────────────────────────────────────────────────────────┐
│                    CLS.Server (API)                      │
│  Controllers → Filters → Middlewares → Hubs              │
│  Background Services                                     │
├──────────────────────────────────────────────────────────┤
│                    CLS.BLL (Business)                    │
│  Services → Interfaces → DTOs → Validators → Mappings   │
├──────────────────────────────────────────────────────────┤
│                    CLS.DAL (Data)                        │
│  Entities → Repositories → Configurations → Migrations  │
│  DbContext                                               │
└──────────────────────────────────────────────────────────┘
         │
         ▼
   PostgreSQL (Supabase)
```

Frontend theo **kiến trúc module theo tính năng (feature-based)**:

```
React App → AppRouter → Layouts → Feature Pages → Feature Components
                ↕                       ↕
           Guards (Auth)         Services (API) + Hooks (TanStack Query)
                ↕                       ↕
           Zustand Stores        Shared Components & Utilities
```

---

## 📁 Cấu trúc dự án

### Thư mục gốc (Repository Root)

```
Classroom_Management_System/
├── .github/workflows/         # Cấu hình CI/CD pipeline
├── Developments/CLS/          # Mã nguồn (monorepo)
│   ├── CLS.Server/            # ASP.NET Core Web API
│   ├── CLS.BLL/               # Tầng nghiệp vụ (Business Logic Layer)
│   ├── CLS.DAL/               # Tầng truy cập dữ liệu (Data Access Layer)
│   ├── cls.client/            # React SPA
│   ├── Dockerfile             # Docker image cho production
│   ├── docker-compose.yml     # Orchestration trên local
│   └── CLS.slnx              # File solution .NET
├── Documents/                 # Tài liệu dự án
│   ├── 00_Workflows/          # Quy trình làm việc
│   ├── 01_Business/           # Yêu cầu nghiệp vụ
│   ├── 02_Requirements/       # Đặc tả chức năng & phi chức năng
│   ├── 03_Design/             # Thiết kế hệ thống & UI mockups
│   ├── 04_Project_Management/ # Lập kế hoạch & theo dõi sprint
│   └── 05_ADR/                # Hồ sơ quyết định kiến trúc (ADR)
├── Templates/                 # Mẫu tài liệu
├── Testing/                   # Tài liệu kiểm thử
│   ├── 01_Unit_Test/
│   ├── 02_Integration_Test/
│   └── 03_System_Test/
├── render.yaml                # IaC blueprint cho Render.com
└── README.md                  # README tiếng Anh
```

### Backend (`Developments/CLS/`)

```
CLS.Server/                    # API Host
├── Controllers/               # Các endpoint REST API
│   ├── AuthController.cs
│   ├── ClassesController.cs
│   ├── DashboardController.cs
│   ├── StudentsController.cs
│   ├── SessionsController.cs
│   ├── RoomsController.cs
│   ├── TuitionPackagesController.cs
│   ├── UsersController.cs
│   ├── TeacherController.cs
│   ├── PaymentsController.cs
│   └── RenewalAlertsController.cs
├── Filters/                   # ApiExceptionFilter
├── Middlewares/                # ExceptionHandlingMiddleware
├── Hubs/                      # NotificationHub (SignalR)
├── BackgroundServices/        # DepletionScanService, EmailDispatchService
└── Program.cs                 # Cấu hình ứng dụng & đăng ký DI

CLS.BLL/                       # Tầng Nghiệp Vụ
├── Services/                  # Triển khai service nghiệp vụ
├── Interfaces/                # Hợp đồng service (ISomethingService)
├── DTOs/                      # Data Transfer Objects
├── Validators/                # Quy tắc FluentValidation
├── Mappings/                  # AutoMapper profiles
└── Common/                    # Exception & helper dùng chung

CLS.DAL/                       # Tầng Truy Cập Dữ Liệu
├── Entities/                  # EF Core entity models (14 entities)
├── Repositories/              # Triển khai Repository Pattern
├── Configurations/            # EF Core entity type configurations
├── Migrations/                # Lịch sử migration cơ sở dữ liệu
├── Data/                      # DbContext
└── Common/                    # Base entities & tiện ích DB dùng chung
```

### Frontend (`Developments/CLS/cls.client/`)

```
src/
├── app/                       # Application shell
│   ├── layouts/               # AuthLayout, MainLayout (sidebar + header)
│   ├── guards/                # PrivateRoute (cổng xác thực)
│   ├── provider/              # authStore (Zustand)
│   └── routers/               # AppRouter (toàn bộ định nghĩa route)
│
├── features/                  # Modules theo tính năng (domain-driven)
│   ├── auth/                  # Trang đăng nhập, hook useAuth
│   ├── academic/              # Trang Dashboard
│   ├── student/               # CRUD học viên
│   ├── class/                 # Quản lý lớp học
│   ├── session/               # Quản lý ca học / lịch học
│   ├── room/                  # Quản lý phòng
│   ├── package/               # Quản lý gói học phí
│   ├── user/                  # Quản lý người dùng / nhân viên
│   ├── attendance/            # Điểm danh + Thời khóa biểu
│   ├── feedback/              # Nhận xét học viên (danh sách + form)
│   ├── financial/             # Tài chính học viên + Quản lý thanh toán
│   ├── retention/             # Cảnh báo gia hạn
│   ├── schedule/              # Tính năng liên quan lịch học
│   └── parent/                # Tính năng liên quan phụ huynh
│
├── shared/                    # Code dùng chung xuyên suốt
│   ├── components/            # ErrorBoundary, Toast, ConnectionErrorBanner
│   ├── hooks/                 # useNotificationHub (SignalR)
│   ├── services/              # apiClient (Axios instance + interceptors)
│   ├── stores/                # toastStore, notificationStore (Zustand)
│   └── utils/                 # constants, formatters
│
├── styles/                    # CSS toàn cục
├── assets/                    # Tài nguyên tĩnh
├── App.jsx                    # Component gốc
└── main.jsx                   # Điểm khởi chạy
```

---

## 🚀 Bắt đầu

### Yêu cầu hệ thống

| Công cụ | Phiên bản |
|---------|-----------|
| Node.js | ≥ 18 |
| .NET SDK | 10.0 |
| PostgreSQL | 16+ (hoặc dùng Supabase) |
| Docker *(tùy chọn)* | Mới nhất |

### 1. Clone Repository

```bash
git clone https://github.com/huuhoa0322/Classroom_Management_System.git
cd Classroom_Management_System
```

### 2. Cài đặt Backend

```bash
cd Developments/CLS

# Khôi phục NuGet packages
dotnet restore CLS.Server/CLS.Server.csproj

# Cấu hình secrets (môi trường phát triển local)
cd CLS.Server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<host>;Database=<db>;Username=<user>;Password=<pass>;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "JwtSettings:SecretKey" "<khóa-bí-mật-256-bit>"

# Áp dụng EF Core migrations
dotnet ef database update --project ../CLS.DAL --startup-project .

# Chạy backend
dotnet run
```

API sẽ khả dụng tại `https://localhost:7065` (development) với tài liệu API Scalar tại `/scalar/v1`.

### 3. Cài đặt Frontend

```bash
cd Developments/CLS/cls.client

# Cài đặt dependencies
npm install

# Khởi động dev server
npm run dev
```

Dev server frontend chạy tại `https://localhost:57264` và tự động proxy các request `/api/*` sang backend.

### 4. Cài đặt Docker (Phương án thay thế)

```bash
cd Developments/CLS

# Sao chép và cấu hình file biến môi trường
cp .env.docker.example .env

# Chế độ 1: Backend + Supabase (DB từ xa)
docker compose up -d cls-api

# Chế độ 2: Backend + PostgreSQL trên local
docker compose --profile local-db up -d
```

---

## 🔐 Biến môi trường

### Backend (`CLS.Server`)

| Biến | Mô tả | Bắt buộc |
|------|-------|----------|
| `ConnectionStrings__DefaultConnection` | Chuỗi kết nối PostgreSQL (format Npgsql) | ✅ |
| `JwtSettings__SecretKey` | Khóa ký JWT (tối thiểu 32 ký tự, entropy cao) | ✅ |
| `JwtSettings__Issuer` | JWT issuer claim | Mặc định: `cls-api` |
| `JwtSettings__Audience` | JWT audience claim | Mặc định: `cls-client` |
| `JwtSettings__AccessTokenExpiryMinutes` | Thời gian sống access token | Mặc định: `60` |
| `JwtSettings__RefreshTokenExpiryDays` | Thời gian sống refresh token | Mặc định: `7` |
| `AllowedOrigins` | Nguồn gốc CORS được phép | Mặc định: `http://localhost:5173` |
| `ASPNETCORE_ENVIRONMENT` | Môi trường runtime | Mặc định: `Production` |

> **Phát triển local:** Sử dụng `dotnet user-secrets` thay vì file `.env`. Xem `.env.example` để biết mẫu đầy đủ.

### Frontend (`cls.client`)

| Biến | Mô tả | File |
|------|-------|------|
| `VITE_API_BASE_URL` | URL cơ sở API (được proxy qua Vite khi dev) | `.env.local` |

---

## 🌐 Triển Khai

### Backend — Render.com

Backend được triển khai dưới dạng Docker web service trên [Render.com](https://render.com):

- **IaC Blueprint:** `render.yaml` tại gốc repo tự động cấu hình service
- **Khu vực:** Singapore (độ trễ thấp nhất cho Việt Nam)
- **Health Check:** `GET /health`
- **Auto-deploy:** Kích hoạt khi push lên nhánh `master`
- **Secrets:** `ConnectionStrings__DefaultConnection` và `JwtSettings__SecretKey` phải được cài đặt thủ công trong Render dashboard

### Frontend — Vercel

React SPA được triển khai trên [Vercel](https://vercel.com):

- **Framework:** Vite (tự động nhận diện)
- **Root Directory:** `Developments/CLS/cls.client`
- **SPA Rewrites:** Được cấu hình trong `vercel.json`

### CI/CD Pipeline

- **GitHub Actions** (`.github/workflows/cls-backend.yml`): Chạy khi push/PR lên `master` — restore, build và test backend
- **Render.com**: Xử lý CD (Docker build → deploy → health check → SSL)
- **Vercel**: Xử lý CD frontend với preview deployments cho các PR

---

## 📐 Quy ước thiết kế API

Tất cả API tuân theo các quy tắc chuẩn hóa sau:

- **Base URL:** `/api/v1`
- **Đặt tên path:** `kebab-case` (ví dụ: `/api/v1/renewal-alerts`)
- **Hướng tài nguyên:** Danh từ số nhiều cho collection (ví dụ: `/students`, `/classes`)
- **Xác thực:** JWT Bearer token trong header `Authorization`
- **Cấu trúc response chuẩn:**

```json
{
  "code": 200,
  "message": "Thao tác thành công",
  "data": { }
}
```

Tài liệu API đầy đủ có sẵn qua **Scalar UI** tại `/scalar/v1` khi chạy backend.

---

## 📝 Quy tắc code

### Cấu trúc module theo tính năng

- Mỗi nghiệp vụ → folder riêng trong `src/features/`
- Mỗi feature chứa: `pages/`, `components/`, `hooks/`, `services/`, `schemas/`
- Hạn chế export; giữ module tự chứa (self-contained)

### Routing

- Toàn bộ route được định nghĩa trong `AppRouter.jsx`
- Route bảo vệ bọc bởi guard `<PrivateRoute>`
- Trang mặc định theo vai trò: Admin → Dashboard (`/`), Teacher → Thời khóa biểu (`/timetable`)

### Data Fetching & State

- **Server state:** TanStack Query cho mọi lời gọi API (caching, retry, invalidation)
- **Client state:** Zustand chỉ dùng cho global UI state (`authStore`, `toastStore`, `notificationStore`)
- **Tầng API:** Service riêng cho từng feature tại `src/features/{module}/services/`
- **Custom hooks:** Hook riêng cho từng feature tại `src/features/{module}/hooks/`

### Phân tầng Backend

- **Controllers** → mỏng, ủy quyền sang services
- **Services (BLL)** → logic nghiệp vụ, validation, mapping
- **Repositories (DAL)** → truy cập dữ liệu qua EF Core, không chứa logic nghiệp vụ
- Tất cả services được đăng ký qua DI và sử dụng thông qua interfaces

### Xử lý lỗi

- **Backend:** `ExceptionHandlingMiddleware` + `ApiExceptionFilter` xử lý exception toàn cục
- **Frontend:** Component `ErrorBoundary` + `ConnectionErrorBanner` cho lỗi kết nối tạm thời + `Toast` cho thông báo người dùng

---

## 📄 Ghi chú

Dự án này được phát triển trong khuôn khổ môn học workshop **AISDLC - AI Agent for Software Development Lifecycle** trường đại học FPT Hà Nội.

- **Giảng viên hướng dẫn:** Nguyễn Thị Điệu
- **Người thực hiện:** Đỗ Hữu Hòa - HE186716
