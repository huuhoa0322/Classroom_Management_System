# 🔒 Báo Cáo Rà Soát Bảo Mật — Classroom Management System

> **Ngày kiểm tra:** 2026-05-12  
> **Phạm vi:** Full-stack (Backend .NET 10 + Frontend React 19 + Infra Docker/Render)  
> **Tổng kết:** **2 Critical** · **4 High** · **5 Medium** · **3 Low**

---

## Tổng Quan Severity

| Severity | Count | Mô tả |
|----------|-------|-------|
| 🔴 **CRITICAL** | 2 | Có thể bị khai thác ngay, ảnh hưởng trực tiếp đến dữ liệu/hệ thống |
| 🟠 **HIGH** | 4 | Rủi ro cao, cần khắc phục trong sprint hiện tại |
| 🟡 **MEDIUM** | 5 | Rủi ro trung bình, nên khắc phục sớm |
| 🟢 **LOW** | 3 | Cải thiện defense-in-depth, có thể lên kế hoạch dài hạn |

---

## 🔴 CRITICAL — Khắc phục ngay

### C1. Không có Rate Limiting trên Login endpoint

> [!CAUTION]
> Endpoint `POST /api/v1/auth/login` **không có bất kỳ cơ chế rate limiting nào** — kẻ tấn công có thể brute-force mật khẩu bằng cách thử hàng nghìn request/giây mà không bị chặn.

**Affected files:**
- [AuthController.cs:25](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Controllers/AuthController.cs#L25) — Login endpoint
- [Program.cs](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Program.cs) — Không có `AddRateLimiter()`

**Remediation — Thêm Rate Limiting (.NET 10 built-in):**

```csharp
// Program.cs — Thêm vào phần service registration
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = 429;
    
    // Fixed window: max 5 login attempts per IP per minute
    opts.AddFixedWindowLimiter("login", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 0;
    });
    
    // Global rate limit cho các API khác
    opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Middleware pipeline — Thêm TRƯỚC Authentication
app.UseRateLimiter();
```

```csharp
// AuthController.cs — Gắn rate limiter cho login
[HttpPost("login")]
[EnableRateLimiting("login")]   // ← Thêm dòng này
public async Task<IActionResult> Login(...)
```

---

### C2. Login endpoint thiếu `[AllowAnonymous]` — có thể bị chặn bởi global Auth

> [!CAUTION]
> [AuthController.cs:22](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Controllers/AuthController.cs#L22) có `//[AllowAnonymous]` **bị comment out**, nhưng controller cũng không có `[Authorize]`. Hiện tại hoạt động nhờ default policy không yêu cầu auth. Tuy nhiên, nếu sau này thêm **global authorization policy** (ví dụ `options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`), login sẽ **hoàn toàn bị khóa** — tất cả người dùng sẽ không thể đăng nhập.

**Remediation:**
```csharp
// AuthController.cs — Uncomment [AllowAnonymous]
[HttpPost("login")]
[AllowAnonymous]   // ← Bỏ comment để đảm bảo luôn truy cập được
public async Task<IActionResult> Login(...)
```

---

## 🟠 HIGH — Cần khắc phục sớm

### H1. JWT Token lưu trong `localStorage` — Dễ bị XSS đánh cắp

> [!WARNING]
> [authStore.js:54](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/src/app/provider/authStore.js#L54) — Zustand persist lưu `accessToken` và `refreshToken` trực tiếp trong `localStorage`. Nếu có **bất kỳ lỗ hổng XSS nào** (kể cả qua third-party library), kẻ tấn công có thể đọc token bằng `localStorage.getItem('cls-auth-storage')`.

**Mức độ rủi ro:** Hiện tại React mặc định escape HTML và project không dùng `dangerouslySetInnerHTML` (✅ đã verify), nên rủi ro XSS trực tiếp thấp. Tuy nhiên, đây là **defense-in-depth** quan trọng.

**Remediation (2 cấp độ):**

| Cấp | Giải pháp | Effort |
|-----|-----------|--------|
| **Quick fix** | Sử dụng `sessionStorage` thay `localStorage` — token mất khi đóng tab | Thấp |
| **Best practice** | Lưu token trong **HttpOnly Cookie** (set từ backend) — JS hoàn toàn không đọc được | Cao |

---

### H2. Không có Security Headers (XSS Protection, Clickjacking, MIME Sniffing)

> [!WARNING]
> Backend **không trả về bất kỳ security header nào** — dễ bị clickjacking (nhúng vào iframe), MIME sniffing attack, và thiếu CSP.

**Affected file:** [Program.cs](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Program.cs) — Không có middleware security headers

**Remediation — Thêm custom middleware hoặc dùng NuGet `NWebsec`:**

```csharp
// Program.cs — Thêm sau UseCors(), trước UseAuthentication()
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    // CSP cho API-only backend
    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    await next();
});
```

---

### H3. HTTPS Redirect bị tắt hoàn toàn

> [!WARNING]
> [Program.cs:243](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Program.cs#L243) — `UseHttpsRedirection()` bị comment out. Comment giải thích rằng "Render handles TLS termination at edge", **đúng cho production trên Render**, nhưng khi self-host hoặc chạy local, traffic sẽ bị gửi qua HTTP plaintext — token JWT, password có thể bị sniff.

**Remediation:**
```csharp
// Chỉ tắt HTTPS redirect khi có reverse proxy
if (!app.Environment.IsDevelopment() && !IsRunningBehindReverseProxy())
{
    app.UseHttpsRedirection();
}
// Hoặc bật UseHsts() cho production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
```

---

### H4. `GenerateRandomPassword()` sử dụng `System.Random` — không cryptographically secure

> [!WARNING]
> [UserManagementService.cs:131](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/UserManagementService.cs#L131) — `new Random()` tạo random password cho tính năng **Reset Password**. `System.Random` **không phải CSPRNG** (Cryptographically Secure Pseudo-Random Number Generator) — kẻ tấn công có thể predict password nếu biết seed.

**Remediation:**
```csharp
private static string GenerateRandomPassword(int length = 12)
{
    const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#$!%";
    // Sử dụng RandomNumberGenerator — cryptographically secure
    return string.Create(length, chars, static (span, chars) =>
    {
        var data = RandomNumberGenerator.GetBytes(span.Length);
        for (int i = 0; i < span.Length; i++)
            span[i] = chars[data[i] % chars.Length];
    });
}
```

---

## 🟡 MEDIUM — Nên khắc phục

### M1. Thiếu `pageSize` Clamping ở hầu hết Controller/Service

Chỉ có [ActivityLogsController.cs:37](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Controllers/ActivityLogsController.cs#L37) và [RenewalAlertService.cs:41](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/RenewalAlertService.cs#L41) thực hiện `Math.Clamp(pageSize, 1, MaxPageSize)`. **Tất cả controller khác** (Students, Classes, Sessions, Payments, Rooms, Users, Packages) **không có bất kỳ giới hạn pageSize nào** — kẻ tấn công có thể gửi `?pageSize=999999` để gây **DoS** (Out of Memory / DB overload).

**Affected controllers:** `StudentsController`, `ClassesController`, `SessionsController`, `PaymentsController`, `RoomsController`, `UsersController`, `TuitionPackagesController`

**Remediation — Centralized clamping:**
```csharp
// Tạo extension method dùng chung
internal static (int page, int pageSize) ClampPagination(int page, int pageSize)
    => (Math.Max(1, page), Math.Clamp(pageSize, 1, AppConstants.Pagination.MaxPageSize));
```

---

### M2. Refresh Token chưa được implement — Access Token là tuyến phòng thủ duy nhất

[JwtService.cs:59-63](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/JwtService.cs#L59-L63) — `GenerateRefreshToken()` tạo refresh token nhưng **không được lưu vào database**, **không có endpoint refresh**, và token được trả về client nhưng **không bao giờ được validate**. Nếu access token bị leak, kẻ tấn công có 60 phút sử dụng.

| Vấn đề | Chi tiết |
|--------|----------|
| Không có `/auth/refresh-token` endpoint | Chỉ khai báo trong `AppConstants.PublicEndpoints` nhưng chưa implement |
| Refresh token không lưu DB | Không thể revoke khi cần |
| Không có token blacklist | Access token đã phát hành không thể thu hồi |

**Remediation:** Implement full refresh token flow hoặc giảm `AccessTokenExpiryMinutes` xuống 15-30 phút.

---

### M3. Không có Account Lockout sau nhiều lần đăng nhập thất bại

[AuthService.cs:60-71](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/AuthService.cs#L60-L71) — Khi đăng nhập thất bại, chỉ log warning mà **không track số lần thất bại** và **không lock account**. Kết hợp với C2 (thiếu rate limiting), hệ thống hoàn toàn mở cho brute-force.

**Remediation:**
- Thêm field `FailedLoginCount` và `LockoutUntil` vào entity `User`
- Sau 5 lần sai → lock account 15 phút
- Reset counter sau khi đăng nhập thành công

---

### M4. Production CORS chỉ whitelist `localhost:5173`

[render.yaml:48](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/render.yaml#L48) — `AllowedOrigins: http://localhost:5173` được dùng cho cả production trên Render. Frontend production deploy trên Vercel nên cần whitelist đúng domain.

```yaml
# render.yaml — Sửa thành domain thật
- key: AllowedOrigins
  value: https://your-production-domain.vercel.app
```

> [!NOTE]
> Dev CORS policy (`SetIsOriginAllowed(_ => true)`) trong [Program.cs:40](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Program.cs#L40) chỉ active khi `IsDevelopment()` — ✅ đúng thiết kế.

---

### M5. Production CORS thiếu `AllowCredentials()`

[Program.cs:45-50](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.Server/Program.cs#L45-L50) — `ProductionCors` policy không gọi `.AllowCredentials()`, trong khi SignalR **yêu cầu credentials** để gửi cookie/auth header. Nếu frontend production kết nối NotificationHub, sẽ bị CORS block.

---

## 🟢 LOW — Cải thiện Defense-in-Depth

### L1. `ValidateToken()` trong `JwtService` dùng sync `.GetAwaiter().GetResult()`

[JwtService.cs:86-89](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/JwtService.cs#L86-L89) — Blocking call trên async method. Trong một số hosting model (IIS classic mode), điều này có thể gây **deadlock**. Nên đổi method signature thành `async Task<ClaimsPrincipal?>`.

---

### L2. Docker default DB password là `localpass123`

[docker-compose.yml:24](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/docker-compose.yml#L24) — Default password cho local PostgreSQL là `localpass123`. Dù chỉ dùng cho local dev, nếu port 5432 bị expose ra public, DB sẽ bị truy cập.

---

### L3. Vite proxy `secure: false` tắt SSL certificate validation

[vite.config.js:106-111](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/vite.config.js#L106-L111) — `secure: false` trong Vite proxy bỏ qua SSL cert verification. Chỉ ảnh hưởng khi development, nhưng nên ghi rõ comment để tránh hiểu nhầm.

---

## ✅ Điểm Tốt — Đã Làm Đúng

| Hạng mục | Đánh giá | Chi tiết |
|----------|----------|----------|
| **Password hashing** | ✅ BCrypt | `BCrypt.Net.BCrypt.HashPassword()` + `Verify()` — industry standard |
| **SQL Injection** | ✅ Safe | Toàn bộ dùng EF Core LINQ — không có `FromSqlRaw` hay string concatenation |
| **XSS (Frontend)** | ✅ Safe | React mặc định escape, không có `dangerouslySetInnerHTML` |
| **JWT Validation** | ✅ Full | `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` đều `true` |
| **JWT ClockSkew** | ✅ Zero | `ClockSkew = TimeSpan.Zero` — token hết hạn chính xác, không có grace period |
| **Role-based Authorization** | ✅ Consistent | Tất cả controller đều có `[Authorize(Roles = "Admin"/"Teacher")]` |
| **SignalR Auth** | ✅ Protected | `NotificationHub` có `[Authorize(Roles = "Admin")]` |
| **Docker non-root** | ✅ Best practice | Dockerfile tạo `clsuser:clsgroup` và chạy bằng non-root user |
| **`.env` not tracked** | ✅ Gitignore | `.env` đã bị gitignore, không có trong Git history |
| **Error response** | ✅ Safe | Production không leak stack trace (`IsDevelopment()` check trước khi trả `exception.Message`) |
| **Config validation** | ✅ Fail-fast | JWT SecretKey thiếu → `throw InvalidOperationException` ngay khi startup |
| **Teacher data isolation** | ✅ Ownership check | `TeacherController` extract `teacherId` từ JWT và truyền vào service layer |

---

## 📋 Action Plan — Thứ tự Ưu tiên

| # | Severity | Hạng mục | Effort | Sprint |
|---|----------|----------|--------|--------|
| 1 | 🔴 C1 | Thêm Rate Limiting cho Login | 1h | Ngay |
| 2 | 🔴 C2 | Uncomment `[AllowAnonymous]` trên Login | 1 min | Ngay |
| 3 | 🟠 H2 | Thêm Security Headers middleware | 30 min | Sprint hiện tại |
| 4 | 🟠 H4 | Fix `GenerateRandomPassword` → CSPRNG | 15 min | Sprint hiện tại |
| 5 | 🟡 M1 | Centralized pageSize clamping | 1h | Sprint hiện tại |
| 6 | 🟡 M3 | Account lockout mechanism | 2h | Sprint hiện tại |
| 7 | 🟠 H3 | Conditional HTTPS redirect | 30 min | Sprint tiếp |
| 8 | 🟡 M4 | Fix production CORS domain | 15 min | Sprint tiếp |
| 9 | 🟡 M5 | Thêm `AllowCredentials()` cho ProductionCors | 5 min | Sprint tiếp |
| 10 | 🟡 M2 | Implement Refresh Token flow | 4-6h | Backlog |
| 11 | 🟠 H1 | Migrate token → HttpOnly Cookie | 4-8h | Backlog |
