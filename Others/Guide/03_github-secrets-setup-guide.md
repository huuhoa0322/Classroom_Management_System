# Hướng dẫn Setup GitHub Secrets cho CLS CI/CD

## Tổng quan

Pipeline CI/CD cần **5 secrets bắt buộc** (bây giờ) và **3 secrets khi có VPS** (sau):

| # | Secret | Cần ngay? |
|---|--------|-----------|
| 1 | `DOCKERHUB_USERNAME` | ✅ Có |
| 2 | `DOCKERHUB_TOKEN` | ✅ Có |
| 3 | `DATABASE_URL` | ✅ Có |
| 4 | `JWT_SECRET_KEY` | ✅ Có |
| 5 | `ALLOWED_ORIGINS` | ✅ Có |
| 6 | `DEPLOY_HOST` | ⬜ Khi có VPS |
| 7 | `DEPLOY_USER` | ⬜ Khi có VPS |
| 8 | `DEPLOY_SSH_KEY` | ⬜ Khi có VPS |

---

## Bước 1: Tạo tài khoản Docker Hub (nếu chưa có)

1. Truy cập [hub.docker.com](https://hub.docker.com)
2. Click **Sign Up** → đăng ký miễn phí
3. Xác nhận email

## Bước 2: Tạo Docker Hub Access Token

1. Đăng nhập [hub.docker.com](https://hub.docker.com)
2. Click avatar góc phải → **Account Settings**
3. Menu trái → **Security** → **Personal access tokens**
4. Click **Generate new token**
5. Đặt tên: `cls-github-actions`
6. Permission: **Read & Write**
7. Click **Generate** → **Copy token ngay** (chỉ hiển thị 1 lần!)

> [!CAUTION]
> Token chỉ hiển thị **1 lần duy nhất**. Nếu mất, phải tạo token mới.

## Bước 3: Tạo JWT Secret Key

Mở terminal và chạy:

```bash
# Cách 1: OpenSSL (nếu có)
openssl rand -hex 32

# Cách 2: PowerShell (Windows)
-join ((1..64) | ForEach-Object { '{0:x}' -f (Get-Random -Max 16) })

# Cách 3: Online (không khuyến nghị cho production)
# https://generate-random.org/api-key-generator
```

Copy kết quả (chuỗi 64 ký tự hex).

## Bước 4: Lấy Supabase Connection String

1. Đăng nhập [supabase.com](https://supabase.com)
2. Chọn project CLS
3. **Project Settings** (gear icon) → **Database**
4. Mục **Connection string** → chọn tab **URI** hoặc **Parameters**
5. Copy thông tin và ghép thành format:

```
Host=db.xxxxxxxxxxxx.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Pooling=false
```

## Bước 5: Thêm Secrets vào GitHub

1. Truy cập: [https://github.com/huuhoa0322/Classroom_Management_System/settings/secrets/actions](https://github.com/huuhoa0322/Classroom_Management_System/settings/secrets/actions)

2. Thêm **từng secret** bằng cách click **"New repository secret"**:

### Secret 1: DOCKERHUB_USERNAME
- **Name**: `DOCKERHUB_USERNAME`
- **Secret**: Username Docker Hub của bạn (ví dụ: `huuhoa0322`)

### Secret 2: DOCKERHUB_TOKEN
- **Name**: `DOCKERHUB_TOKEN`
- **Secret**: Access token từ Bước 2

### Secret 3: DATABASE_URL
- **Name**: `DATABASE_URL`
- **Secret**: Connection string Supabase từ Bước 4

### Secret 4: JWT_SECRET_KEY
- **Name**: `JWT_SECRET_KEY`
- **Secret**: Chuỗi hex 64 ký tự từ Bước 3

### Secret 5: ALLOWED_ORIGINS
- **Name**: `ALLOWED_ORIGINS`
- **Secret**: `http://localhost:5173` (tạm thời, cập nhật khi có domain frontend)

## Bước 6: Tạo GitHub Environments (khuyến nghị)

1. Truy cập: [https://github.com/huuhoa0322/Classroom_Management_System/settings/environments](https://github.com/huuhoa0322/Classroom_Management_System/settings/environments)

2. Click **"New environment"** → tạo **`staging`**
   - Không cần protection rules

3. Click **"New environment"** → tạo **`production`**
   - Bật **"Required reviewers"** → thêm bạn vào danh sách
   - Điều này bắt buộc approve thủ công trước khi deploy production

---

## Kiểm tra sau khi setup

Sau khi thêm xong tất cả secrets, trang Settings → Secrets → Actions sẽ hiển thị:

```
Repository secrets
├── DOCKERHUB_USERNAME    Updated just now
├── DOCKERHUB_TOKEN       Updated just now
├── DATABASE_URL          Updated just now
├── JWT_SECRET_KEY        Updated just now
└── ALLOWED_ORIGINS       Updated just now
```

Pipeline sẽ tự động chạy khi bạn push code backend lên branch `main` hoặc `develop`.
