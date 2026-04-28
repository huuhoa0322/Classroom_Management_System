# Hướng dẫn Deploy CLS Backend lên Render.com

## Tổng quan Architecture

```
Push code → GitHub Actions (CI: Build & Test)
                ↓ pass
         Render.com (CD: Docker Build → Deploy → Health Check)
                ↓
         https://cls-api.onrender.com/health ✅
```

> [!IMPORTANT]
> Với Render.com, bạn **KHÔNG cần** Docker Hub, SSH key, hay VPS.
> Render tự build Docker image từ source code và deploy.

---

## Bước 1: Tạo tài khoản Render

1. Truy cập [render.com](https://render.com)
2. Click **"Get Started for Free"**
3. Chọn **"Sign in with GitHub"** → Authorize Render truy cập GitHub

---

## Bước 2: Tạo Web Service

1. Sau khi đăng nhập, click **"New +"** → **"Web Service"**
2. Chọn **"Build and deploy from a Git repository"** → **Next**
3. Tìm repo **`huuhoa0322/Classroom_Management_System`** → Click **"Connect"**

---

## Bước 3: Cấu hình Service

Điền các thông tin sau:

| Field | Giá trị |
|-------|---------|
| **Name** | `cls-api` |
| **Region** | `Singapore (Southeast Asia)` ← gần VN nhất |
| **Branch** | `main` |
| **Runtime** | `Docker` |
| **Dockerfile Path** | `./Developments/CLS/Dockerfile` |
| **Docker Context Directory** | `./Developments/CLS` |
| **Instance Type** | `Free` (hoặc Starter $7/mo nếu cần always-on) |

> [!WARNING]
> **Free tier**: Service sẽ tự "spin down" sau 15 phút không có request. Request đầu tiên sau đó sẽ mất ~30-60s để "cold start". Upgrade lên Starter ($7/mo) nếu cần always-on.

---

## Bước 4: Thêm Environment Variables

Trong trang tạo service, kéo xuống phần **"Environment Variables"** → click **"Add Environment Variable"** cho từng biến:

### 4.1 — Database (Supabase)

| Key | Value |
|-----|-------|
| `ConnectionStrings__DefaultConnection` | `Host=db.xxxxxxxxxxxx.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=YOUR_SUPABASE_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Pooling=false` |

> Lấy thông tin từ: **Supabase Dashboard** → **Project Settings** → **Database**

### 4.2 — JWT Authentication

| Key | Value |
|-----|-------|
| `JwtSettings__SecretKey` | *(chuỗi 64 ký tự hex — tạo bằng `openssl rand -hex 32`)* |
| `JwtSettings__Issuer` | `cls-api` |
| `JwtSettings__Audience` | `cls-client` |
| `JwtSettings__AccessTokenExpiryMinutes` | `60` |
| `JwtSettings__RefreshTokenExpiryDays` | `7` |

### 4.3 — ASP.NET Core

| Key | Value |
|-----|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### 4.4 — CORS

| Key | Value |
|-----|-------|
| `AllowedOrigins` | `http://localhost:5173` |

> Sau khi deploy frontend lên Vercel/Netlify, update lại giá trị này thành URL frontend production.

---

## Bước 5: Deploy

1. Click **"Create Web Service"**
2. Render sẽ:
   - Clone repo từ GitHub
   - Build Docker image từ `Developments/CLS/Dockerfile`
   - Start container
   - Chạy health check tại `/health`
3. Quá trình build lần đầu mất **3-5 phút**
4. Khi thấy status **"Live"** → deploy thành công! ✅

---

## Bước 6: Verify

Sau khi deploy xong, Render cấp URL dạng:

```
https://cls-api.onrender.com
```

Kiểm tra:

```bash
# Health check
curl https://cls-api.onrender.com/health

# API docs (nếu bật ở Production)
# https://cls-api.onrender.com/scalar/v1
```

---

## Cấu hình bổ sung (sau khi deploy thành công)

### Health Check (trong Render Dashboard)

1. Vào service **cls-api** → **Settings**
2. Mục **Health Check Path**: nhập `/health`
3. Render sẽ tự động restart container nếu health check fail

### Auto-Deploy

Render **tự động deploy** mỗi khi có push mới lên branch `main`. Không cần thêm config.

Nếu muốn tắt auto-deploy:
- Service → **Settings** → **Build & Deploy** → **Auto-Deploy** → Off

### Custom Domain (tùy chọn)

1. Service → **Settings** → **Custom Domains**
2. Thêm domain: `api.cls.yourdomain.com`
3. Thêm CNAME record ở DNS provider

---

## CI/CD Flow hoàn chỉnh

```
Developer push code
       ↓
GitHub Actions (CI)
  ├── dotnet restore
  ├── dotnet build (Release)
  └── dotnet test (khi có)
       ↓ pass
Render.com (CD) — tự động trigger
  ├── Clone repo
  ├── Docker build (multi-stage)
  ├── Deploy container
  └── Health check /health
       ↓
https://cls-api.onrender.com ✅
```

> [!TIP]
> Với cách này, bạn **KHÔNG cần** các GitHub Secrets: `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`, `DEPLOY_HOST`, `DEPLOY_USER`, `DEPLOY_SSH_KEY`. Render handles everything.

---

## Troubleshooting

| Vấn đề | Giải pháp |
|--------|----------|
| Build fail: "CLS.slnx not found" | Kiểm tra Docker Context = `./Developments/CLS` |
| App crash: "JwtSettings:SecretKey is missing" | Kiểm tra env var `JwtSettings__SecretKey` đã set chưa |
| Health check fail | Kiểm tra env var `ConnectionStrings__DefaultConnection` đúng chưa |
| CORS error từ frontend | Update `AllowedOrigins` thành URL frontend production |
| Cold start chậm (30-60s) | Upgrade lên Starter plan ($7/mo) |
