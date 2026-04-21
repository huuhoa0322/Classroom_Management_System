# CLS API Design Rules

> **Scope:** Áp dụng cho toàn bộ RESTful API của Classroom Management System (CLS).  
> **Source:** Derived from [ADR-001](../Documents/05_ADR/ADR-001-adopt-modular-monolith-dotnet-react.md) & [`api_design_rule.md`](../api_design_rule.md).  
> **Audience:** AI agents, backend developers, frontend consumers, QA engineers.

---

## 1. Base URL & Versioning

| Rule | Value |
|------|-------|
| **Global prefix** | `/api/v1` |
| **Versioning strategy** | URI-based (`/v1`, `/v2`) |
| **Base URL (local dev)** | `https://localhost:57264/api/v1` |
| **Base URL (staging/prod)** | `https://api.cls.vn/api/v1` |

**All endpoints MUST start with `/api/v1/...`**

```
✅  GET  /api/v1/students
✅  POST /api/v1/class-sessions
❌  GET  /students
❌  GET  /api/student
```

---

## 2. URL Naming Conventions

### 2.1 Path Casing

All URL path segments MUST use **kebab-case** (lowercase, hyphen-separated):

```
✅  /api/v1/student-enrollments
✅  /api/v1/class-sessions
✅  /api/v1/renewal-alerts
❌  /api/v1/studentEnrollments     ← camelCase FORBIDDEN
❌  /api/v1/student_enrollments    ← snake_case FORBIDDEN
❌  /api/v1/StudentEnrollments     ← PascalCase FORBIDDEN
```

### 2.2 Resource Naming

- Use **plural nouns** for collection resources
- Use **singular noun** only for singleton resources (rare)
- Resources represent **entities, NOT actions**

```
✅  /api/v1/students                  ← collection
✅  /api/v1/students/{id}             ← single item
✅  /api/v1/class-sessions/{id}/attendance  ← sub-resource
❌  /api/v1/getStudents               ← verb in URL FORBIDDEN
❌  /api/v1/createStudent             ← verb in URL FORBIDDEN
❌  /api/v1/student/list              ← non-standard pattern
```

### 2.3 HTTP Method → Action Mapping

| HTTP Method | Action | Example |
|-------------|--------|---------|
| `GET` | Retrieve resource(s) | `GET /api/v1/students` |
| `POST` | Create new resource | `POST /api/v1/students` |
| `PUT` | Full update (replace) | `PUT /api/v1/students/{id}` |
| `PATCH` | Partial update | `PATCH /api/v1/students/{id}` |
| `DELETE` | Soft/hard delete | `DELETE /api/v1/students/{id}` |

> **CLS Convention:** Prefer **soft delete** (set `IsDeleted = true`) over hard delete for all student/enrollment/financial records to maintain audit trail.

---

## 3. Standard Response Structure

ALL API responses (success AND error) MUST be wrapped in the following JSON envelope:

```json
{
  "code": 200,
  "message": "Students retrieved successfully",
  "data": { ... } 
}
```

### 3.1 Field Definitions

| Field | Type | Description |
|-------|------|-------------|
| `code` | `integer` | HTTP status code mirrored (200, 201, 400, 401, 403, 404, 409, 500) |
| `message` | `string` | Human-readable result description |
| `data` | `object \| array \| null` | Response payload; `null` for error/empty results |

### 3.2 Success Response Examples

**Single resource:**
```json
// GET /api/v1/students/42
{
  "code": 200,
  "message": "Student retrieved successfully",
  "data": {
    "id": 42,
    "fullName": "Nguyen Van An",
    "email": "an.nguyen@email.com",
    "enrolledAt": "2026-03-01T00:00:00Z",
    "packageRemainingSessions": 8
  }
}
```

**Collection resource (with pagination):**
```json
// GET /api/v1/students?page=1&pageSize=20
{
  "code": 200,
  "message": "Students retrieved successfully",
  "data": {
    "items": [ { ... }, { ... } ],
    "totalCount": 87,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

**Create resource:**
```json
// POST /api/v1/students → 201 Created
{
  "code": 201,
  "message": "Student enrolled successfully",
  "data": {
    "id": 88,
    "fullName": "Tran Thi Bich"
  }
}
```

**No content (delete/action):**
```json
// DELETE /api/v1/students/42 → 200 OK
{
  "code": 200,
  "message": "Student removed successfully",
  "data": null
}
```

### 3.3 Error Response Examples

**Validation error (400):**
```json
{
  "code": 400,
  "message": "Validation failed",
  "data": {
    "errors": {
      "fullName": ["Full name is required"],
      "email": ["Invalid email format"]
    }
  }
}
```

**Not found (404):**
```json
{
  "code": 404,
  "message": "Student with ID 999 not found",
  "data": null
}
```

**Conflict (409) — e.g., scheduling conflict:**
```json
{
  "code": 409,
  "message": "Scheduling conflict detected",
  "data": {
    "conflictType": "TeacherUnavailable",
    "teacherId": 5,
    "conflictingSessionId": 77,
    "requestedTime": "2026-04-21T09:00:00Z"
  }
}
```

**Unauthorized (401):**
```json
{
  "code": 401,
  "message": "Authentication required. Please provide a valid Bearer token.",
  "data": null
}
```

**Server error (500):**
```json
{
  "code": 500,
  "message": "An unexpected error occurred. Please try again later.",
  "data": null
}
```

---

## 4. Authentication & Authorization

### 4.1 JWT Bearer Token

ALL secured endpoints MUST require JWT Bearer token:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Token configuration:**
| Setting | Value |
|---------|-------|
| Algorithm | HS256 |
| Issuer | Configured in `appsettings.json` → `JwtSettings:Issuer` |
| Audience | Configured in `appsettings.json` → `JwtSettings:Audience` |
| Access token expiry | 60 minutes |
| Refresh token expiry | 7 days |

### 4.2 Role-Based Access

CLS defines 3 system roles:

| Role | Access Level |
|------|-------------|
| `Admin` | Full system access |
| `Teacher` | Own schedule, attendance marking, feedback submission |
| `Parent` | Read-only: child's attendance, schedule, feedback |

Endpoint-level authorization:
```csharp
[Authorize(Roles = "Admin")]                    // Admin only
[Authorize(Roles = "Admin,Teacher")]            // Admin or Teacher
[Authorize]                                     // Any authenticated user
[AllowAnonymous]                                // Public (login, register only)
```

### 4.3 Public Endpoints (No Auth Required)

```
POST /api/v1/auth/login
POST /api/v1/auth/refresh-token
```

---

## 5. Query Parameters

### 5.1 Pagination (Required for all list endpoints)

```
GET /api/v1/students?page=1&pageSize=20
```

| Param | Type | Default | Max |
|-------|------|---------|-----|
| `page` | `int` | `1` | — |
| `pageSize` | `int` | `20` | `100` |

### 5.2 Filtering

Use descriptive query param names in camelCase:

```
GET /api/v1/students?classId=5&status=active
GET /api/v1/class-sessions?teacherId=3&date=2026-04-21
GET /api/v1/renewal-alerts?daysThreshold=14
```

### 5.3 Sorting

```
GET /api/v1/students?sortBy=fullName&sortOrder=asc
```

| Param | Values |
|-------|--------|
| `sortBy` | Field name (camelCase) |
| `sortOrder` | `asc` \| `desc` |

---

## 6. Data Format Standards

### 6.1 Date & Time

- ALL dates/times MUST be in **ISO 8601 UTC** format: `"2026-04-21T09:00:00Z"`
- Frontend displays times in local timezone (Vietnam: UTC+7)
- NEVER send timestamp as Unix epoch or localized string

### 6.2 Property Naming (JSON)

- JSON property names MUST be **camelCase**
- Consistent with JavaScript consumer conventions

```json
✅ { "fullName": "...", "createdAt": "...", "remainingSessions": 8 }
❌ { "FullName": "...", "created_at": "...", "remaining_sessions": 8 }
```

### 6.3 Null vs Omit

- ALWAYS include fields in response even if `null` — do NOT omit undefined fields
- This prevents client-side `undefined` errors

```json
✅ { "parentEmail": null, "feedbackNote": null }
❌ { }  ← missing fields
```

---

## 7. CLS Domain API Endpoints Reference

### Auth Module

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/auth/login` | ❌ | Login, returns JWT tokens |
| POST | `/api/v1/auth/refresh-token` | ❌ | Refresh access token |
| POST | `/api/v1/auth/logout` | ✅ | Invalidate refresh token |

### Student Module

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/students` | ✅ Admin | List all students (paginated) |
| GET | `/api/v1/students/{id}` | ✅ Admin | Get student detail |
| POST | `/api/v1/students` | ✅ Admin | Enroll new student |
| PUT | `/api/v1/students/{id}` | ✅ Admin | Update student info |
| DELETE | `/api/v1/students/{id}` | ✅ Admin | Soft delete student |
| GET | `/api/v1/students/{id}/attendance` | ✅ Admin,Parent | Get attendance records |
| GET | `/api/v1/students/renewal-alerts` | ✅ Admin | List students near package expiry |

### Class Sessions Module

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/class-sessions` | ✅ Admin,Teacher | List sessions (filterable) |
| GET | `/api/v1/class-sessions/{id}` | ✅ All | Get session detail |
| POST | `/api/v1/class-sessions` | ✅ Admin | Create new session (with conflict check) |
| PUT | `/api/v1/class-sessions/{id}` | ✅ Admin | Update session (with conflict check) |
| DELETE | `/api/v1/class-sessions/{id}` | ✅ Admin | Cancel session |
| POST | `/api/v1/class-sessions/{id}/attendance` | ✅ Teacher | Submit attendance |

### Feedback Module

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/class-sessions/{id}/feedback` | ✅ Teacher | Submit session feedback (SLA: 12h) |
| GET | `/api/v1/students/{id}/feedback` | ✅ Admin,Parent | View student feedback history |

### Notification Module

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/v1/notifications` | ✅ Admin | List notification logs |
| POST | `/api/v1/notifications/send-test` | ✅ Admin | Trigger test email (dev only) |

---

## 8. OpenAPI / Swagger Rules

- ALL controllers MUST have `[ProducesResponseType]` annotations
- ALL endpoints MUST have XML doc comments for Swagger summary
- Swagger UI available at `/swagger` in development only (disabled in production)

```csharp
/// <summary>
/// Retrieve a paginated list of all enrolled students.
/// </summary>
/// <param name="page">Page number (default: 1)</param>
/// <param name="pageSize">Items per page (default: 20, max: 100)</param>
[HttpGet]
[ProducesResponseType(typeof(ApiResponse<PagedResult<StudentDto>>), 200)]
[ProducesResponseType(typeof(ApiResponse<object>), 401)]
public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
```

---

## 9. API Response Wrapper Implementation

```csharp
// CLS.BLL/Common/ApiResponse.cs
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Success", int code = 200)
        => new() { Code = code, Message = message, Data = data };

    public static ApiResponse<object> Fail(string message, int code = 400)
        => new() { Code = code, Message = message, Data = null };
}
```

Usage in controllers:
```csharp
// 200 OK
return Ok(ApiResponse<StudentDto>.Success(dto, "Student retrieved successfully"));

// 201 Created
return StatusCode(201, ApiResponse<StudentDto>.Success(dto, "Student enrolled successfully", 201));

// 404 Not Found
return NotFound(ApiResponse<object>.Fail("Student not found", 404));
```

---

## 10. Business-Specific API Rules (CLS Domain)

| Rule | Detail |
|------|--------|
| **Scheduling conflict** | `POST/PUT /class-sessions` MUST validate teacher & room availability; return `409` with conflict details on failure |
| **Soft delete only** | Students, enrollments, financial records use `IsDeleted` flag — `DELETE` NEVER removes DB row |
| **Renewal alert threshold** | `GET /students/renewal-alerts` default `daysThreshold=14` (2 weeks per business rule) |
| **Feedback SLA** | Feedback API MUST log `submittedAt` timestamp; dashboard shows sessions missing feedback after 12h |
| **Attendance automation** | After attendance POST, system MUST trigger async email notification to parent (via background service) |
| **Package depletion** | System MUST decrement `RemainingSessions` after each attended session; trigger renewal alert when ≤ 3 sessions remain OR package expiry ≤ 14 days |

---

> **Last Updated:** 2026-04-20  
> **Maintained by:** Tech Lead / SA Lead  
> **Related:** [coding-conventions.md](./coding-conventions.md) | [ADR-001](../Documents/05_ADR/ADR-001-adopt-modular-monolith-dotnet-react.md) | [api_design_rule.md](../api_design_rule.md)
