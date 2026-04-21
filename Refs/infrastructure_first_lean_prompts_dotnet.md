# Infrastructure-First: Lean Prompt Set (.NET Core 10 / ASP.NET Core)

> **Phiên bản:** Lean — Dành cho **IDE environments** (Cursor, Claude Code, Gemini Code Assist)
> nơi có khả năng **attach context files** trực tiếp vào prompt.
>
> **Tech stack:** .NET 10 · ASP.NET Core Web API · EF Core 10 · PostgreSQL 15+ · FluentValidation · JWT Bearer · Serilog
>
> **So sánh với bản Java/Spring Boot:** Cùng 6 prompts, cùng cấu trúc HITL checklist — chỉ thay thế hoàn toàn tech stack.

---

## Nguyên tắc Lean Prompt

> [!IMPORTANT]
> **Chỉ viết vào prompt những thứ thuộc 1 trong 3 loại sau:**
> 1. **Chưa có** trong bất kỳ context file nào
> 2. Là **quyết định thiết kế mới** cho task cụ thể (e.g., "dùng C# record")
> 3. Là **override/ngoại lệ** so với convention chung

### Prerequisite: Context Files phải có sẵn

```
.ai-context/
├── project_context.md           ← L1: Project Identity, Tech Stack, Glossary
├── api-design-rules.md          ← L2: API Conventions, Response Wrapper, Security Rules
└── coding-conventions.md        ← L3: Project Structure, Naming, Layer Rules, Anti-patterns
```

Ngoài ra, nếu có từ giai đoạn Design (Lesson 03):
```
Documents/
├── 05_ADR/
│   └── ADR-001-adopt-modular-monolith-dotnet-react.md
└── api-contracts/
    └── *.yaml                    ← OpenAPI/Swagger (nếu đã generate)
```

---

## So sánh: Verbose vs. Lean

| Prompt | Verbose (dòng) | Lean (dòng) | Giảm |
|--------|:-:|:-:|:-:|
| P1: Scaffolding | ~130 | ~45 | **65%** |
| P2: BaseEntity | ~75 | ~30 | **60%** |
| P3: ApiResponse | ~70 | ~30 | **57%** |
| P4: Exceptions | ~120 | ~50 | **58%** |
| P5: Security | ~110 | ~45 | **59%** |
| P6: EF Core Migrations | ~80 | ~35 | **56%** |

---

## Prompt 1: Project Scaffolding + Application Configuration

### 📎 Attach files
- `project_context.md` (L1)
- `coding-conventions.md` (L3)
- `ADR-001-adopt-modular-monolith-dotnet-react.md`

### 🤖 Prompt

```markdown
# Role: Senior .NET Architect (ASP.NET Core 10 specialist)

# Task
Generate the complete project scaffolding for the CLS backend using .NET 10 / ASP.NET Core Web API.

# Context — READ these files FIRST
1. `project_context.md` → Extract: project name, tech stack, module list
2. `coding-conventions.md` → Follow EXACTLY: project structure, namespace rules, layer dependencies
3. `ADR-001` → Follow: architecture pattern (Modular Monolith, 3-layer), database choice (PostgreSQL via Supabase)

# Deliverables

## 1. Solution & Project Structure
- Solution file: `CLS.sln`
- Projects (as per `coding-conventions.md` section 2):
  - `CLS.Server/`   ← ASP.NET Core Web API (Presentation Layer)
  - `CLS.BLL/`      ← Business Logic Layer (class library)
  - `CLS.DAL/`      ← Data Access Layer (class library)
- Project references: Server → BLL → DAL

## 2. `CLS.Server/CLS.Server.csproj`
- `<TargetFramework>net10.0</TargetFramework>`
- NuGet packages — include ONLY:
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
  - `Swashbuckle.AspNetCore` (Swagger)
  - Project references to CLS.BLL and CLS.DAL

## 3. `CLS.BLL/CLS.BLL.csproj`
- NuGet packages:
  - `FluentValidation.AspNetCore`
  - `AutoMapper.Extensions.Microsoft.DependencyInjection`
  - Project reference to CLS.DAL

## 4. `CLS.DAL/CLS.DAL.csproj`
- NuGet packages:
  - `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `Microsoft.EntityFrameworkCore.Design` (dev dependency)
  - `BCrypt.Net-Next` (password hashing)

## 5. `CLS.Server/appsettings.json` + `appsettings.Development.json`
- Connection string via env var: `${ConnectionStrings__DefaultConnection}`
- `JwtSettings`: Issuer, Audience, SecretKey (from env var `${JWT_SECRET_KEY}`), AccessTokenExpiryMinutes: 60
- Serilog sink config: Console + rolling file
- Development override: detailed EF Core SQL logging enabled

## 6. `CLS.Server/Program.cs` skeleton
- Register: DbContext, AutoMapper, FluentValidation, JWT Bearer auth, Swagger, CORS
- CORS: allow `localhost:5173` (Vite dev server)
- Middleware pipeline order: UseHttpsRedirection → UseCors → UseAuthentication → UseAuthorization → MapControllers
- JSON: camelCase serialization, ignore null (via `JsonSerializerOptions`)
- Mark with `// TODO: Register module services here` for Vertical Slice phase

# Constraints
- NO feature module code — empty folders + placeholder files only
- NO hardcoded secrets or connection strings
- Complete, compilable `.csproj` and `Program.cs` — zero TODOs except the one marked above
- `dotnet build` MUST pass
```

### ✅ HITL Checklist
- [ ] `dotnet build` passes with 0 errors
- [ ] Solution has 3 projects with correct references (Server→BLL→DAL)
- [ ] JWT secret loaded from env var (NOT hardcoded)
- [ ] CORS allows `localhost:5173`
- [ ] JSON serialization is camelCase
- [ ] Middleware order: Auth before Authorization
- [ ] Swagger registered (dev only)

---

## Prompt 2: BaseEntity — Shared Entity Foundation

### 📎 Attach files
- `coding-conventions.md` (L3)

### 🤖 Prompt

```markdown
# Role: Senior .NET Developer (EF Core 10 specialist)

# Task
Generate `BaseEntity.cs` — the abstract base class ALL domain entities in CLS.DAL will inherit from.

# Context
Read `coding-conventions.md` → sections: project structure, entity naming (section 4.1), EF Core conventions (section 7), anti-patterns.

# Requirements (not in context files)
1. **Primary Key:** `Guid Id` — default value via `Guid.NewGuid()` in constructor or property initializer
2. **Audit fields** (auto-populated via EF Core SaveChanges override or interceptor):
   - `CreatedAt`, `UpdatedAt` → `DateTime` (UTC)
   - `CreatedBy`, `UpdatedBy` → `string?`
3. **Soft delete:** `bool IsDeleted` (default `false`) + `DateTime? DeletedAt`
4. **EF Core config:** All column mappings via Fluent API in a separate `BaseEntityConfiguration<T>` class
   - Column naming: `snake_case` (EF Core `UseSnakeCaseNamingConvention()` or explicit mapping)
   - `created_at` + `created_by` → `IsRequired(false)`, no update after insert (handled via interceptor)

# Hard Constraints
- ❌ Do NOT use Data Annotations on the entity class — Fluent API ONLY (per L3 section 7.2)
- ❌ Do NOT use `int` or `long` as primary key — Guid ONLY
- ✅ Use `DateTime` (UTC), NOT `DateTimeOffset`
- ✅ Class must be `abstract`
- ✅ Include XML doc comments: purpose + "ALL entities must inherit this"

# Output
Two files:
1. `CLS.DAL/Entities/BaseEntity.cs`
2. `CLS.DAL/Configurations/BaseEntityConfiguration.cs` (abstract IEntityTypeConfiguration<T>)
```

### ✅ HITL Checklist
- [ ] `abstract` class with `Guid Id` property
- [ ] `IsDeleted = false` default, `DeletedAt` nullable
- [ ] Audit fields present (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- [ ] `BaseEntityConfiguration<T>` maps all columns to snake_case
- [ ] Zero Data Annotations on entity
- [ ] XML doc comment present

---

## Prompt 3: ApiResponse\<T\> — Unified API Response Wrapper

### 📎 Attach files
- `api-design-rules.md` (L2)
- `coding-conventions.md` (L3)

### 🤖 Prompt

```markdown
# Role: Senior .NET Developer (ASP.NET Core Web API)

# Task
Generate `ApiResponse<T>` — the universal response wrapper for ALL CLS API endpoints.

# Context
Read `api-design-rules.md` → section 3 for the EXACT JSON envelope format and field definitions.
Read `coding-conventions.md` → section 2 for the correct project/folder placement (CLS.BLL/Common/).

# Design Decisions (not in context files)
1. Use C# **`record`** (not class) — immutable, value-based equality
2. Static factory methods:
   - `Success(T data, string message = "Success")` → code 200
   - `Created(T data, string message)` → code 201
   - `NoContent(string message)` → code 200, data = default
   - `Fail(string message, int code = 400)` → data = default(T)
   - `Fail(string message, int code, T errorDetails)` → for validation errors
3. Also create `PagedResult<T>` record:
   - Fields: `IEnumerable<T> Items`, `int TotalCount`, `int Page`, `int PageSize`, `int TotalPages`
   - Static factory: `From(IEnumerable<T> items, int totalCount, int page, int pageSize)`
   - `TotalPages` computed: `(int)Math.Ceiling(totalCount / (double)pageSize)`

# Constraints
- JSON keys MUST be camelCase (enforced by `JsonSerializerOptions` in Program.cs — no extra attributes needed)
- `data` field can be null for error/no-content responses
- Do NOT add `timestamp` field — keep wrapper minimal per L2 spec
- Include XML doc with usage example

# Output
Two files:
1. `CLS.BLL/Common/ApiResponse.cs`
2. `CLS.BLL/Common/PagedResult.cs`
```

### ✅ HITL Checklist
- [ ] Uses `record` (not class)
- [ ] JSON matches `api-design-rules.md` section 3 envelope (`code`, `message`, `data`)
- [ ] All 5 factory methods present
- [ ] `PagedResult<T>` computes `TotalPages` correctly
- [ ] No `[JsonPropertyName]` clutter — relies on global camelCase setting
- [ ] Placed in `CLS.BLL/Common/`

---

## Prompt 4: Exception Hierarchy + Global Exception Middleware

### 📎 Attach files
- `api-design-rules.md` (L2) — error response format, HTTP status codes
- `coding-conventions.md` (L3) — project structure, exception handling section (5.6)
- *Output from Prompt 3:* `ApiResponse.cs`

### 🤖 Prompt

```markdown
# Role: Senior .NET Developer (ASP.NET Core middleware specialist)

# Task
Generate the complete Exception Hierarchy + centralized ExceptionHandlingMiddleware for CLS.
After this, NO Controller should ever use try-catch for business logic errors.

# Context
- `api-design-rules.md` → sections 3.3 (error response examples) + 4 (HTTP status codes)
- All error responses MUST use `ApiResponse<T>` wrapper (already generated)
- `coding-conventions.md` → section 5.6: middleware placement in `CLS.Server/Middlewares/`

# Part 1: Custom Exception Classes
Package: `CLS.BLL/Exceptions/`

| Exception Class | HTTP | Use Case |
|---|---|---|
| `NotFoundException` | 404 | Entity not found by ID |
| `BusinessException` | 422 | Business rule violation (e.g., scheduling conflict logic) |
| `ConflictException` | 409 | Scheduling conflict, duplicate resource |
| `UnauthorizedException` | 401 | Authentication failure |
| `ForbiddenException` | 403 | Role-based authorization failure |

All inherit from a base `AppException(string message, int statusCode)`.

## Part 2: `ExceptionHandlingMiddleware.cs`
Placed in `CLS.Server/Middlewares/` — implements `IMiddleware`.

Handle ALL of:
- 5 custom `AppException` subclasses → map StatusCode + `ApiResponse.Fail()`
- `ValidationException` (FluentValidation) → 400 with structured field errors:
  ```json
  { "code": 400, "message": "Validation failed",
    "data": { "errors": { "fullName": ["Full name is required"] } } }
  ```
- `DbUpdateException` / `PostgresException` → 409 if unique constraint, else 500
- `Exception` catch-all → 500: LOG with Serilog, return generic message

## Part 3: `ValidationErrorDetail` record
Structure: `Dictionary<string, string[]> Errors` — matches FluentValidation's `ToDictionary()` output.

# Constraints
- ALL responses are `application/json` with `ApiResponse` wrapper
- Catch-all MUST use `ILogger<ExceptionHandlingMiddleware>` — NEVER expose stack trace to client
- Register in `Program.cs` BEFORE `UseAuthentication`

# Output
8 files: `AppException` base + 5 custom exceptions + `ExceptionHandlingMiddleware` + `ValidationErrorDetail`
```

### ✅ HITL Checklist
- [ ] `AppException` base class with `StatusCode` property
- [ ] All 5 custom exceptions inherit `AppException`
- [ ] FluentValidation errors = structured `Dictionary<string, string[]>`
- [ ] 500 catch-all: logs full exception, returns generic message only
- [ ] `DbUpdateException` mapped to 409 (unique) or 500
- [ ] All responses wrapped in `ApiResponse`
- [ ] Middleware registered before `UseAuthentication` in `Program.cs`

---

## Prompt 5: Security Configuration (JWT Skeleton)

### 📎 Attach files
- `api-design-rules.md` (L2) — sections 4 (JWT config), 4.2 (roles), 4.3 (public endpoints)
- `coding-conventions.md` (L3) — section 8 (security standards)

### 🤖 Prompt

```markdown
# Role: Senior ASP.NET Core Security Architect

# Task
Generate the Security Configuration SKELETON using ASP.NET Core Authentication + JWT Bearer.
This is Infrastructure only — feature-specific role policies come in Vertical Slice.

# Context
Read `api-design-rules.md` → section 4 for:
- JWT settings (Issuer, Audience, algorithm HS256, expiry)
- Public endpoints that must be anonymous (section 4.3)
- Role definitions: Admin, Teacher, Parent (section 4.2)

# Files to Generate

## 1. `CLS.Server/Middlewares/JwtSettings.cs`
POCO bound to `appsettings.json → JwtSettings` section:
- `string Issuer`, `string Audience`, `string SecretKey`, `int AccessTokenExpiryMinutes`

## 2. `CLS.Server/Services/JwtTokenService.cs`
Utility service (registered as scoped):
- `string GenerateToken(string userId, string email, string role)` — signs HS256 JWT
- `ClaimsPrincipal? ValidateToken(string token)` — returns null if invalid
- Config injected via `IOptions<JwtSettings>`
- Secret key loaded from `JwtSettings.SecretKey` (from env var — NOT hardcoded)

## 3. JWT Bearer registration in `Program.cs` (additions only)
```csharp
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // TokenValidationParameters: ValidIssuer, ValidAudience, IssuerSigningKey, ClockSkew=0
        // OnChallenge: override to return ApiResponse format 401
        // OnForbidden: override to return ApiResponse format 403
    });
builder.Services.AddAuthorization();
// TODO: Add role-based policies per module in Vertical Slice
```

## 4. `appsettings.json` JwtSettings block (additions only)
```json
"JwtSettings": {
  "Issuer": "cls-api",
  "Audience": "cls-client",
  "SecretKey": "",
  "AccessTokenExpiryMinutes": 60
}
```
SecretKey MUST be empty in appsettings — loaded from env var `JWT_SECRET_KEY` at runtime.

# Constraints
- ❌ Do NOT create User entity, UserService, or login endpoint — Vertical Slice concern
- ❌ Do NOT hardcode JWT secret
- ✅ Custom 401/403 challenge handlers MUST return `ApiResponse` JSON (not default HTML)
- ✅ Mark with: `// TODO: Replace userId claim source with AuthModule UserService in Vertical Slice`
```

### ✅ HITL Checklist
- [ ] `JwtSettings` POCO bound from configuration
- [ ] `JwtTokenService` uses `IOptions<JwtSettings>` — no direct `IConfiguration` access
- [ ] HS256 algorithm specified in `TokenValidationParameters`
- [ ] `ClockSkew = TimeSpan.Zero` set
- [ ] Custom 401 challenge returns `ApiResponse.Fail(...)` JSON
- [ ] Custom 403 forbidden returns `ApiResponse.Fail(...)` JSON
- [ ] Secret key NOT in appsettings committed value
- [ ] No User entity created

---

## Prompt 6: Database Migration Baseline (EF Core + PostgreSQL)

### 📎 Attach files
- `coding-conventions.md` (L3) — sections 7 (EF Core conventions), 7.3 (migration rules)
- `physical_schema.sql` (DDL from Lesson 03 Design phase, if available)
- `ADR-001-adopt-modular-monolith-dotnet-react.md` (database choice)
- *Output from Prompt 2:* `BaseEntity.cs` + `BaseEntityConfiguration.cs`

### 🤖 Prompt

```markdown
# Role: Senior .NET Developer (EF Core 10 + PostgreSQL specialist)

# Task
Generate the AppDbContext + baseline EF Core migration — the shared foundation ALL modules depend on.

# Context
- `coding-conventions.md` → sections 7.1–7.3: entity naming, Fluent API, migration rules
- `BaseEntity.cs` → audit columns that ALL entity tables must include
- Database: PostgreSQL 15+ (Supabase), snake_case naming convention

# Files to Generate

## 1. `CLS.DAL/Data/AppDbContext.cs`
- Inherits `DbContext`
- Constructor: `AppDbContext(DbContextOptions<AppDbContext> options)`
- Override `OnModelCreating`: apply all configurations via `modelBuilder.ApplyConfigurationsFromAssembly()`
- Override `SaveChangesAsync`: auto-populate `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` from current user context (inject `ICurrentUserService` — interface only, implementation in Auth Vertical Slice)
- Enable `UseSnakeCaseNamingConvention()` (Npgsql convention) — or apply globally in `OnModelCreating`
- DbSets: `DbSet<AppUser> Users` (placeholder for Auth module)

## 2. `CLS.DAL/Data/ICurrentUserService.cs`
Interface only:
```csharp
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
}
```
Mark with: `// TODO: Implement in CLS.Server/Services/CurrentUserService.cs in Auth Vertical Slice`

## 3. `CLS.DAL/Entities/AppUser.cs` (Auth placeholder)
Minimal entity (NO navigation properties yet):
- Inherits `BaseEntity`
- Properties: `string Email`, `string PasswordHash`, `string FullName`, `string Role`, `bool IsActive`
- Configured in `CLS.DAL/Configurations/AppUserConfiguration.cs`

## 4. `CLS.DAL/Configurations/AppUserConfiguration.cs`
Fluent API config:
- Table: `"app_users"` (snake_case, plural)
- `Email`: required, max 256, unique index `idx_app_users_email`
- `Role`: required, max 50, index `idx_app_users_role`
- `IsActive`: default `true`
- `PasswordHash`: column name `"password_hash"` (NOT `"password"`)
- Inherit `BaseEntityConfiguration<AppUser>` (from Prompt 2)

## 5. EF Core Migration: `InitialBaseline`
Run command (shown as comment — AI generates the migration file content):
```bash
dotnet ef migrations add InitialBaseline --project CLS.DAL --startup-project CLS.Server
```
Generated migration MUST create:
- `app_users` table with ALL columns from `BaseEntity` (id, created_at, updated_at, created_by, updated_by, is_deleted, deleted_at) + user-specific columns
- Unique index on `email`
- Standard indexes on `role`, `is_active`, `is_deleted`

# Constraints
- ❌ Do NOT create module-specific tables — Vertical Slice concern
- ❌ NEVER use `EnsureCreated()` — EF Core migrations manage schema
- ✅ snake_case column/table names throughout
- ✅ `password_hash` (NOT `password`)
- ✅ Soft delete columns present (from BaseEntity)
- ✅ Migration name: descriptive PascalCase (`InitialBaseline`, `AddRenewalAlertIndex`)

# Output
5 files: AppDbContext + ICurrentUserService + AppUser entity + AppUserConfiguration + Migration class
```

### ✅ HITL Checklist
- [ ] `AppDbContext` overrides `SaveChangesAsync` for audit fields
- [ ] `ICurrentUserService` interface only — no implementation
- [ ] `AppUser` inherits `BaseEntity`
- [ ] Column `password_hash` (not `password`)
- [ ] Unique index on `email`
- [ ] All `BaseEntity` audit columns in migration output
- [ ] snake_case naming applied globally
- [ ] `dotnet ef database update` → migration applies without error

---

## Quy trình thực hiện

```
P1 Scaffolding → P2 BaseEntity → P3 ApiResponse → P4 Exceptions → P5 Security → P6 EF Core
     │                                                                                  │
     └─────────────── Mỗi prompt attach context files thay vì lặp lại ─────────────────┘
                                                                                         │
                                                                                         ▼
                                                                              ✅ Ready for
                                                                              Vertical Slice
```

### Final Validation (sau khi hoàn thành cả 6 prompts)

```bash
# 1. Build thành công
dotnet build CLS.sln

# 2. Apply migrations
dotnet ef database update --project CLS.DAL --startup-project CLS.Server

# 3. Run application
dotnet run --project CLS.Server

# 4. Health/Swagger check (dev)
curl https://localhost:57264/swagger        # → Swagger UI loads

# 5. Error handler test (unknown endpoint)
curl https://localhost:57264/api/v1/unknown # → ApiResponse JSON 404

# 6. Auth required test
curl https://localhost:57264/api/v1/students # → ApiResponse JSON 401
```

> [!CAUTION]
> **Chỉ khi tất cả 6 bước trên PASS, mới chuyển sang Vertical Slice.**

---

## Tổng kết: Infrastructure Checklist

| ✅ | Component | Prompt | Attach Files |
|----|-----------|:------:|-------------|
| ☐ | Scaffolding + Program.cs | **P1** | L1 + L3 + ADR |
| ☐ | BaseEntity + Configuration | **P2** | L3 |
| ☐ | ApiResponse\<T\> + PagedResult | **P3** | L2 + L3 |
| ☐ | Exceptions + Middleware | **P4** | L2 + L3 + ApiResponse |
| ☐ | JWT Security Skeleton | **P5** | L2 + L3 |
| ☐ | AppDbContext + EF Migration | **P6** | L3 + DDL + ADR + BaseEntity |

> [!TIP]
> **Lean Prompt hoạt động tốt khi và chỉ khi** context files đã đầy đủ.
> Nếu `coding-conventions.md` thiếu project structure → AI sẽ tự bịa namespace → output sai.
> Luôn kiểm tra context files TRƯỚC khi bắt đầu prompting.

---

## Appendix: .NET-specific Gotchas

| Vấn đề | Giải pháp |
|--------|-----------|
| EF Core migration không tìm thấy DbContext | Thêm `IDesignTimeDbContextFactory<AppDbContext>` trong `CLS.DAL` |
| JWT 401 trả về HTML thay vì JSON | Override `OnChallenge` trong `AddJwtBearer` options |
| FluentValidation không trigger tự động | Đăng ký `AddFluentValidationAutoValidation()` trong `Program.cs` |
| snake_case column không apply | Dùng `UseSnakeCaseNamingConvention()` từ `Npgsql.EntityFrameworkCore.PostgreSQL` |
| Audit fields không tự điền | Override `SaveChangesAsync` trong `AppDbContext`, inject `ICurrentUserService` |
| CORS block từ Vite dev server | Allow `https://localhost:5173` trong `UseCors` config |
