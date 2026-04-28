# Hướng dẫn Deploy CLS Frontend lên Vercel

## Bước 1: Tạo tài khoản Vercel

1. Truy cập [vercel.com](https://vercel.com)
2. Click **"Sign Up"** → chọn **"Continue with GitHub"**
3. Authorize Vercel truy cập GitHub

---

## Bước 2: Import Repository

1. Click **"Add New..."** → **"Project"**
2. Tìm repo **`huuhoa0322/Classroom_Management_System`** → click **"Import"**

---

## Bước 3: Cấu hình Project

| Field | Giá trị |
|-------|---------|
| **Project Name** | `cls-client` |
| **Framework Preset** | `Vite` (Vercel tự detect) |
| **Root Directory** | `Developments/CLS/cls.client` ← click "Edit" để đổi |
| **Build Command** | `npm run build` (default) |
| **Output Directory** | `dist` (default) |

> **Quan trọng**: Phải set **Root Directory** = `Developments/CLS/cls.client` vì frontend không nằm ở repo root.

---

## Bước 4: Thêm Environment Variable

Trong phần **"Environment Variables"**:

| Key | Value |
|-----|-------|
| `VITE_API_BASE_URL` | `https://cls-api.onrender.com/api/v1` |

---

## Bước 5: Deploy

Click **"Deploy"** → Vercel sẽ:
1. Clone repo
2. `cd Developments/CLS/cls.client`
3. `npm install`
4. `npm run build` (Vite build)
5. Deploy static files từ `dist/`

Build mất khoảng **1-2 phút**.

---

## Bước 6: Update CORS trên Render

Sau khi deploy xong, Vercel cấp URL:
```
https://cls-client.vercel.app
```

Quay lại **Render Dashboard** → service `cls-api` → **Environment** → sửa:

```
AllowedOrigins = https://cls-client.vercel.app
```

Nếu không update CORS, frontend sẽ bị **CORS error** khi gọi API.

---

## Tóm tắt Architecture

```
Vercel (Frontend)                    Render (Backend)
┌──────────────────┐    HTTPS/REST   ┌──────────────────┐
│  React + Vite    │ ──────────────→ │  ASP.NET Core 10 │
│  cls-client      │    CORS allow   │  cls-api         │
│  Static SPA      │ ←────────────── │  Docker/Alpine   │
└──────────────────┘                 └────────┬─────────┘
                                              │
                                     ┌────────▼─────────┐
                                     │  Supabase        │
                                     │  PostgreSQL      │
                                     └──────────────────┘
```
