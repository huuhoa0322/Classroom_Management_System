# CLS Backend Coding Conventions

> **Scope:** ASP.NET Core 10, EF Core 10, PostgreSQL (Supabase).  
> **Source:** [ADR-001](../Documents/05_ADR/ADR-001-adopt-modular-monolith-dotnet-react.md)  
> **Audience:** AI agents, backend developers, code reviewers.  
> **Related:** [coding-conventions-frontend.md](./coding-conventions-frontend.md) | [api-design-rules.md](./api-design-rules.md)

---

## 1. Architecture Overview

```
[Presentation Layer]   ASP.NET Core 10 — Controllers, Middlewares, Filters
         ↓
[Business Logic Layer] Services, DTOs, Interfaces, FluentValidation, AutoMapper
         ↓
[Data Access Layer]    EF Core 10, Repositories, Entities, Migrations
         ↓
[Database]             PostgreSQL 15+ (Supabase)
```

**Module boundary rule:** Các module KHÔNG được truy cập trực tiếp vào `DbContext` của module khác. Mọi cross-module access phải đi qua **Service Interface**.

---

## 2. Project Structure

```
Developments/CLS/
├── CLS.Server/                        ← Presentation Layer (ASP.NET Web API)
│   ├── Controllers/                   ← HTTP endpoints (thin controllers only)
│   ├── Middlewares/                   ← JWT auth, exception handling, CORS
│   ├── Filters/                       ← Action filters, validation filters
│   ├── Program.cs                     ← DI registration, middleware pipeline
│   └── appsettings.json
├── CLS.BLL/                           ← Business Logic Layer
│   ├── Services/                      ← Core business rules
│   ├── DTOs/                          ← Data Transfer Objects
│   ├── Interfaces/                    ← Service contracts (IXxxService)
│   ├── Validators/                    ← FluentValidation rules
│   └── Common/                        ← ApiResponse<T>, PagedResult<T>, exceptions
└── CLS.DAL/                           ← Data Access Layer
    ├── Data/                          ← AppDbContext.cs
    ├── Entities/                      ← Domain Entities (POCO)
    ├── Repositories/                  ← Generic & Specific Repositories
    ├── Migrations/                    ← EF Core auto-generated
    └── Configurations/                ← Fluent API entity configurations
```

---

## 3. Naming Conventions (.NET / C#)

| Element | Convention | Example |
|---------|-----------|---------|
| **Namespace** | PascalCase, match folder | `CLS.Server.Controllers` |
| **Class** | PascalCase | `StudentEnrollmentService` |
| **Interface** | `I` prefix + PascalCase | `IStudentEnrollmentService` |
| **Method** | PascalCase | `GetEnrollmentByIdAsync()` |
| **Async Method** | Suffix `Async` | `CreateEnrollmentAsync()` |
| **Private Field** | `_camelCase` | `_studentRepository` |
| **Property** | PascalCase | `public string FullName { get; set; }` |
| **DTO** | Suffix `Dto` or `Request`/`Response` | `EnrollmentDto`, `CreateStudentRequest` |
| **Entity** | PascalCase, singular noun | `Student`, `ClassSession` |
| **Repository** | Suffix `Repository` | `StudentRepository` |
| **Controller** | Suffix `Controller` | `StudentsController` |
| **Validator** | Suffix `Validator` | `CreateStudentRequestValidator` |
| **Migration** | Descriptive, auto-generated base | `AddStudentPackageTable` |
| **Constants** | PascalCase in static class | `AppConstants.JwtSecretKey` |

---

## 4. Controller Rules

```csharp
// ✅ CORRECT — Thin controller, delegate to service
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _studentService.GetByIdAsync(id);
        return Ok(ApiResponse<StudentDto>.Success(result));
    }
}

// ❌ WRONG — Business logic and direct DbContext access in controller
[HttpPost]
public async Task<IActionResult> Create(CreateStudentRequest request)
{
    var student = new Student { FullName = request.FullName };
    _context.Students.Add(student);
    await _context.SaveChangesAsync();
    return Ok(student); // raw entity, not ApiResponse
}
```

**Rules:**
- Controllers MUST be **thin**: validate → call service → return `ApiResponse`
- Controllers MUST NOT access `DbContext` directly
- All routes MUST follow `api/v1/[controller]` pattern
- Use `[Authorize]` on protected endpoints; `[AllowAnonymous]` only on auth endpoints
- ALL actions MUST have `[ProducesResponseType]` for Swagger

---

## 5. Service Layer Rules

```csharp
// ✅ CORRECT
public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    private readonly IMapper _mapper;

    public StudentService(IStudentRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<StudentDto> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Student {id} not found");
        return _mapper.Map<StudentDto>(entity);
    }
}
```

**Rules:**
- Services MUST implement an interface (`IXxxService`)
- Services own **business logic, validation coordination, and exception throwing**
- Use typed custom exceptions: `NotFoundException`, `ValidationException`, `ConflictException`
- Use **AutoMapper** for DTO ↔ Entity — never manual property mapping

---

## 6. Validation Rules (FluentValidation)

```csharp
public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10,11}$").WithMessage("Phone must be 10-11 digits")
            .When(x => x.PhoneNumber != null);
    }
}
```

**Rules:**
- ALWAYS use **FluentValidation** — NEVER Data Annotations for business validation
- Register via `AddFluentValidationAutoValidation()` in `Program.cs`
- Validators live in `CLS.BLL/Validators/`; one validator per request DTO

---

## 7. Repository Rules

```csharp
// Generic interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}

// Domain-specific extension
public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> FindByEmailAsync(string email);
    Task<IEnumerable<Student>> GetNearExpiryAsync(int daysThreshold);
}
```

**Rules:**
- Generic Repository handles standard CRUD; extend with domain-specific queries
- Repositories MUST NOT contain business logic
- `SaveChangesAsync()` called in **Services**, NOT inside repositories (Unit of Work)
- Use `AsNoTracking()` for all read-only queries

---

## 8. Async / Await Rules

- ALL I/O-bound operations MUST use `async`/`await` — no `.Result` or `.Wait()`
- Async method signatures MUST have `Async` suffix
- Pass `CancellationToken` to long-running / background operations

```csharp
// ✅ CORRECT
public async Task<StudentDto> GetByIdAsync(int id, CancellationToken ct = default)

// ❌ WRONG — blocks thread, deadlock risk
public StudentDto GetById(int id) => _repo.GetByIdAsync(id).Result;
```

---

## 9. Exception Handling

- **Global exception middleware** in `CLS.Server/Middlewares/ExceptionHandlingMiddleware.cs`
- Domain exception types in `CLS.BLL/Common/Exceptions/`:

| Exception | HTTP Code | When to use |
|-----------|-----------|-------------|
| `NotFoundException` | 404 | Resource does not exist |
| `ValidationException` | 400 | Invalid input data |
| `ConflictException` | 409 | Business rule conflict (e.g., scheduling overlap) |
| `UnauthorizedException` | 401 | Auth token invalid/expired |
| `ForbiddenException` | 403 | Insufficient role/permissions |

- NEVER leak stack traces to client in production

---

## 10. Database & EF Core Conventions

### 10.1 Entity Structure

```csharp
// ✅ CORRECT
public class ClassSession
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;   // Soft delete flag
}
```

### 10.2 Fluent API Configuration

```csharp
// CLS.DAL/Configurations/ClassSessionConfiguration.cs
public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("class_sessions");           // snake_case (PostgreSQL)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StartTime).IsRequired();
        builder.HasOne(x => x.Teacher)
               .WithMany(t => t.Sessions)
               .HasForeignKey(x => x.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);   // ALWAYS explicit
        builder.HasQueryFilter(x => !x.IsDeleted);  // Global soft-delete filter
    }
}
```

**Rules:**
- Table & column names: `snake_case` (PostgreSQL convention)
- ALL configurations via Fluent API — NOT Data Annotations on entities
- ALWAYS define `OnDelete` explicitly
- Apply `HasQueryFilter` for all soft-delete entities

### 10.3 Migration Rules

```bash
dotnet ef migrations add <MeaningfulName> --project CLS.DAL --startup-project CLS.Server
dotnet ef database update --project CLS.DAL --startup-project CLS.Server
```

**Rules:**
- Migration names must be descriptive: `AddStudentPackageTable`, `SeedRoles`
- NEVER manually edit auto-generated migration files
- Migrations MUST be committed together with the feature branch

---

## 11. Security Standards

| Area | Rule |
|------|------|
| **Authentication** | JWT Bearer — `Microsoft.AspNetCore.Authentication.JwtBearer` |
| **Password hashing** | BCrypt via ASP.NET Identity — NEVER plain text |
| **CORS** | Explicit allowed origins only — no wildcard `*` in production |
| **Sensitive config** | User Secrets (dev) / Environment Variables (prod) |
| **Input validation** | FluentValidation on ALL incoming request DTOs |
| **SQL injection** | EF Core parameterized queries ONLY — no string interpolation in raw SQL |
| **Role-based auth** | `[Authorize(Roles = "Admin,Teacher")]` at controller/action level |
| **Soft delete** | Financial & student records use `IsDeleted` flag — NEVER hard delete |

---

## 12. Logging Standards (Serilog)

```csharp
// ✅ CORRECT — Structured message templates
_logger.LogInformation("Student {StudentId} enrolled in class {ClassId}", studentId, classId);
_logger.LogWarning("Renewal alert: student {StudentId} has {Days} days remaining", id, days);
_logger.LogError(ex, "Failed to send attendance email for session {SessionId}", sessionId);

// ❌ WRONG — String interpolation loses structured properties
_logger.LogInformation($"Student {studentId} enrolled");
```

| Level | When to use |
|-------|-------------|
| `Debug` | Development diagnostics only |
| `Information` | User-initiated actions (enrollment created, login success) |
| `Warning` | Soft failures (email retry, near-expiry triggered) |
| `Error` | Caught exceptions (email failed, DB timeout) |
| `Critical` | System-level failures (DB unreachable, app crash) |

**Rule:** NEVER log passwords, JWT tokens, or unmasked PII.

---

## 13. Code Quality Rules

- Maximum method length: **30 lines** (extract helpers if longer)
- Maximum class length: **300 lines**
- No magic numbers/strings — use named constants in `AppConstants`
- Remove all `Console.WriteLine` before committing

### Git Commit Convention (Conventional Commits)

```
feat(student): add renewal alert background service
fix(auth): correct JWT expiry check on refresh
refactor(scheduler): extract conflict detection to ScheduleValidator
test(enrollment): add unit tests for package depletion calculation
chore(deps): upgrade EF Core to 10.1
```

Format: `<type>(<scope>): <short description>`  
Types: `feat` | `fix` | `refactor` | `docs` | `test` | `chore` | `perf`

### Branch Strategy

```
main        ← Production-ready, protected (PR + review required)
develop     ← Integration branch
feature/    ← feature/cls-123-student-enrollment
fix/        ← fix/cls-456-renewal-alert-duplicate
```

---

## 14. Testing Standards

| Layer | Framework | What to test |
|-------|-----------|-------------|
| **Unit** | xUnit + Moq | Service methods, validators, utility functions |
| **Integration** | WebApplicationFactory | Controller endpoints, EF Core queries |

**Minimum coverage (MVP):**
- Services: ≥ **70%** | Validators: **100%** | Controllers: happy path + major error cases

**Test naming:**
```csharp
// Pattern: MethodName_Scenario_ExpectedResult
public async Task GetByIdAsync_StudentNotFound_ThrowsNotFoundException() { }
public async Task CreateAsync_DuplicateEmail_ThrowsConflictException() { }
```

---

> **Last Updated:** 2026-04-21  
> **Maintained by:** Tech Lead / SA Lead  
> **Related:** [coding-conventions-frontend.md](./coding-conventions-frontend.md) | [api-design-rules.md](./api-design-rules.md) | [ADR-001](../Documents/05_ADR/ADR-001-adopt-modular-monolith-dotnet-react.md)
