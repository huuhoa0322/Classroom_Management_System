# Infrastructure-First: Lean Prompt Set (IDE-Optimized)

> **Phiên bản:** Lean — Dành cho **IDE environments** (Cursor, Claude Code, Gemini Code Assist)
> nơi có khả năng **attach context files** trực tiếp vào prompt.
>
> **So sánh:** Bản Verbose gốc (~789 dòng) → Bản Lean này (~350 dòng) — **giảm ~55% token**

---

## Nguyên tắc Lean Prompt

> [!IMPORTANT]
> **Chỉ viết vào prompt những thứ thuộc 1 trong 3 loại sau:**
> 1. **Chưa có** trong bất kỳ context file nào
> 2. Là **quyết định thiết kế mới** cho task cụ thể (e.g., "dùng Java record")
> 3. Là **override/ngoại lệ** so với convention chung

### Prerequisite: Context Files phải có sẵn

```
.ai-context/
├── project_context.md           ← L1: Project Identity, Tech Stack, Glossary
├── api_design_rules.md          ← L2: API Conventions, Response Wrapper, Security Rules
└── coding_conventions.md        ← L3: Package Structure, Naming, Layer Rules, Anti-patterns
```

Ngoài ra, nếu có từ giai đoạn Design (Lesson 03):
```
docs/
├── adr/
│   └── ADR-001-modular-monolith.md
└── api-contracts/
    └── *.yaml                    ← OpenAPI/Swagger (nếu đã generate)
```

---

## So sánh: Verbose vs. Lean

| Prompt | Verbose (dòng) | Lean (dòng) | Giảm |
|--------|:-:|:-:|:-:|
| P1: Scaffolding | ~125 | ~40 | **68%** |
| P2: BaseEntity | ~70 | ~30 | **57%** |
| P3: BaseResponse | ~75 | ~30 | **60%** |
| P4: Exceptions | ~120 | ~50 | **58%** |
| P5: Security | ~105 | ~45 | **57%** |
| P6: Flyway | ~75 | ~35 | **53%** |

---

## Prompt 1: Project Scaffolding + Application Configuration

### 📎 Attach files
- `project_context.md` (L1)
- `coding_conventions.md` (L3)
- `ADR-001-modular-monolith.md`

### 🤖 Prompt

```markdown
# Role: Senior Java/Spring Boot Architect

# Task
Generate the complete project scaffolding for the [PROJECT_NAME] backend.

# Context — READ these files FIRST
1. `project_context.md` → Extract: project name, tech stack, module list
2. `coding_conventions.md` → Follow EXACTLY: package structure, naming rules, layer dependencies
3. `ADR-001` → Follow: architecture pattern, database choice

# Deliverables

## 1. `pom.xml`
- Java version + Spring Boot version → from `<tech_stack>` in L1
- Dependencies — include ONLY:
  - spring-boot-starter-web, data-jpa, validation, security
  - flyway-core + flyway-database-postgresql
  - postgresql (runtime), lombok, jjwt (v0.12.x)
  - spring-boot-starter-test + h2 (test scope)
- groupId/artifactId → derive from project name

## 2. Package Structure
- Follow `coding_conventions.md` exactly
- Create all module folders (empty — no entity/service code yet)
- Create `shared/` subpackages: base, exception, security, config

## 3. `application.yml` + `application-dev.yml`
- context-path: from `<api_design_rules>` in L2
- ddl-auto: `validate` (Flyway manages schema)
- Credentials via env vars with defaults: `${DB_USERNAME:postgres}`
- Dev profile: enable SQL logging, allow Flyway clean

## 4. `AppConfig.java`
- `@EnableJpaAuditing` (for BaseEntity audit fields)
- CORS: allow `localhost:3000`
- Jackson: camelCase, ignore unknown properties

# Constraints
- NO feature module code — empty folders only
- NO hardcoded credentials
- Complete, compilable files — zero TODOs
```

### ✅ HITL Checklist
- [ ] `mvn clean compile` passes
- [ ] Package structure matches `coding_conventions.md`
- [ ] `ddl-auto: validate` (NOT `update` or `create`)
- [ ] Context-path matches `api_design_rules.md`
- [ ] No hardcoded passwords

---

## Prompt 2: BaseEntity — Shared Entity Foundation

### 📎 Attach files
- `coding_conventions.md` (L3)

### 🤖 Prompt

```markdown
# Role: Senior Java/Spring Boot Developer (JPA/Hibernate specialist)

# Task
Generate `BaseEntity.java` — the abstract superclass ALL domain entities will extend.

# Context
Read `coding_conventions.md` → sections: package structure, entity naming, anti-patterns.

# Requirements (not in context files)
1. **ID:** UUID with `@GeneratedValue(strategy = GenerationType.UUID)` (Hibernate 6+)
2. **Audit fields** (auto via Spring Data JPA Auditing):
   - `createdAt`, `updatedAt` → `LocalDateTime`
   - `createdBy`, `updatedBy` → `String`
   - `createdAt` + `createdBy` → `@Column(updatable = false)`
3. **Soft delete:** `isDeleted` (default false) + `deletedAt`
4. **Annotations:**
   - `@MappedSuperclass`, `@EntityListeners(AuditingEntityListener.class)`
   - Lombok: `@Getter`, `@Setter`, `@EqualsAndHashCode(of = "id")`

# Hard Constraints
- ❌ Do NOT use `@Data` (unsafe for JPA — see anti-patterns in L3)
- ❌ Do NOT use `GenerationType.AUTO`
- ✅ Use `java.time.LocalDateTime`, NOT `java.util.Date`
- ✅ Include Javadoc: purpose + "ALL entities must extend this"

# Output
Single file: `BaseEntity.java` — package per `coding_conventions.md`
```

### ✅ HITL Checklist
- [ ] `abstract` class with `@MappedSuperclass`
- [ ] UUID strategy = `GenerationType.UUID`
- [ ] `@Column(updatable = false)` on createdAt, createdBy
- [ ] `@EntityListeners(AuditingEntityListener.class)` present
- [ ] No `@Data`, no `@GeneratedValue(AUTO)`
- [ ] Soft delete fields present

---

## Prompt 3: BaseResponse\<T\> — API Response Wrapper

### 📎 Attach files
- `api_design_rules.md` (L2)
- `coding_conventions.md` (L3)

### 🤖 Prompt

```markdown
# Role: Senior Java/Spring Boot Developer

# Task
Generate `BaseResponse<T>` — the universal API response wrapper.

# Context
Read `api_design_rules.md` → `<response_wrapper>` section for the EXACT JSON format.
Read `coding_conventions.md` → package location for shared/base.

# Design Decisions (not in context files)
1. Use Java **`record`** (not class) — immutable, concise
2. Static factory methods:
   - `success(T data)` → 200
   - `created(T data)` → 201
   - `noContent()` → 204, data=null
   - `error(int code, String message)` → data=null
   - `error(int code, String message, T errorDetails)` → for validation
3. Also create `PageResponse<T>` record:
   - Fields: `content`, `page`, `size`, `totalElements`, `totalPages`, `last`
   - Static factory: `from(Page<T> springPage)`

# Constraints
- JSON keys MUST be camelCase
- `data` can be null for error responses
- Do NOT add timestamp to wrapper — keep minimal
- Include Javadoc with usage examples

# Output
Two files: `BaseResponse.java`, `PageResponse.java`
```

### ✅ HITL Checklist
- [ ] Uses `record` (not class)
- [ ] JSON matches `api_design_rules.md` wrapper format
- [ ] All factory methods present
- [ ] `PageResponse` wraps Spring's `Page<T>`
- [ ] No extra fields polluting the wrapper

---

## Prompt 4: Exception Hierarchy + GlobalExceptionHandler

### 📎 Attach files
- `api_design_rules.md` (L2) — error response format, HTTP status codes
- `coding_conventions.md` (L3) — package location
- *Output from Prompt 3:* `BaseResponse.java`

### 🤖 Prompt

```markdown
# Role: Senior Java/Spring Boot Developer (error handling specialist)

# Task
Generate the complete Exception Hierarchy + centralized GlobalExceptionHandler.
After this, NO Controller should EVER use try-catch for business logic.

# Context
- `api_design_rules.md` → `<http_methods_and_status_codes>` + `<error_response>` sections
- All error responses MUST use `BaseResponse<T>` wrapper (already generated)

# Part 1: Custom Exception Classes
Package: per `coding_conventions.md` → shared/exception

| Exception | HTTP | Use Case |
|---|---|---|
| `ResourceNotFoundException` | 404 | Entity not found by ID |
| `BusinessException` | 422 | Business rule violation (+ errorCode field) |
| `DuplicateResourceException` | 409 | Unique constraint violation |
| `UnauthorizedException` | 401 | Authentication failure |
| `ForbiddenException` | 403 | Authorization failure |

## Part 2: `GlobalExceptionHandler.java`
`@RestControllerAdvice` — handle ALL of:
- 5 custom exceptions above → map to BaseResponse.error()
- `MethodArgumentNotValidException` → 400 with structured field errors:
  ```json
  { "code": 400, "message": "Validation failed", 
    "data": { "errors": [{"field":"email","message":"must not be blank"}] } }
  ```
- `ConstraintViolationException` → 400
- `HttpMessageNotReadableException` → 400 "Malformed JSON"
- `DataIntegrityViolationException` → 409
- `Exception` (catch-all) → 500 — LOG real error, return generic message

## Part 3: `ValidationErrorResponse` record
Structure: `List<FieldError>` where `FieldError(String field, String message)`

# Constraints
- ALL handlers return `ResponseEntity<BaseResponse<?>>`
- Catch-all MUST use `@Slf4j` logging — NEVER expose internal errors to client
- Order: most specific → least specific

# Output
7 files: 5 exceptions + GlobalExceptionHandler + ValidationErrorResponse
```

### ✅ HITL Checklist
- [ ] `@RestControllerAdvice` on handler
- [ ] All 5 custom exceptions created
- [ ] Validation errors = structured field-level list (not just string)
- [ ] 500 catch-all: logs real error, returns generic message
- [ ] `DataIntegrityViolationException` handled
- [ ] All responses wrapped in `BaseResponse`

---

## Prompt 5: Security Configuration (JWT Skeleton)

### 📎 Attach files
- `api_design_rules.md` (L2) — `<security_rules>`, `<endpoint_classification>`
- `coding_conventions.md` (L3)

### 🤖 Prompt

```markdown
# Role: Senior Java/Spring Security Architect

# Task
Generate the Security Configuration SKELETON using Spring Security 6.x + JWT.
This is Infrastructure only — feature-specific security (roles, permissions) comes in Vertical Slice.

# Context
Read `api_design_rules.md` → `<security_rules>` + `<endpoint_classification>` for:
- Which endpoints are public vs. protected
- JWT authentication rules

# Files to Generate

## 1. `SecurityConfig.java`
- `@EnableWebSecurity` + `@EnableMethodSecurity`
- Spring Security 6.x **lambda DSL** (NOT deprecated `.and()` chains)
- Session: `STATELESS`, CSRF: disabled
- Endpoint rules from `<endpoint_classification>` in L2
- JWT filter before `UsernamePasswordAuthenticationFilter`
- Custom 401/403 handlers returning `BaseResponse` format
- `PasswordEncoder` bean (BCrypt)
- `AuthenticationManager` bean

## 2. `JwtProvider.java`
Utility class:
- `generateToken(username, claims)`, `extractUsername(token)`, `isTokenValid(token)`
- Config from `application.yml`: `${JWT_SECRET}`, `${JWT_EXPIRATION:86400000}`
- Use JJWT library (io.jsonwebtoken)

## 3. `JwtAuthenticationFilter.java`
Extends `OncePerRequestFilter`:
- Extract Bearer token → validate → set SecurityContext
- Mark with: `// TODO: Replace with UserDetailsService in Auth module slice`

## 4. JWT properties to add to `application.yml`

# Constraints
- ❌ Do NOT create User entity or UserDetailsService — Vertical Slice concern
- ❌ Do NOT hardcode JWT secret — env var only
- ✅ Use lambda DSL (Spring Security 6.x)
- ✅ STATELESS sessions — no cookies
```

### ✅ HITL Checklist
- [ ] Lambda DSL (no `.and()` chains)
- [ ] STATELESS, CSRF disabled
- [ ] Public endpoints from L2 `<endpoint_classification>` → `permitAll()`
- [ ] JWT secret from env var
- [ ] Custom 401/403 return `BaseResponse`
- [ ] `@EnableMethodSecurity` present
- [ ] No User entity created

---

## Prompt 6: Database Migration Baseline (Flyway)

### 📎 Attach files
- `coding_conventions.md` (L3) — database conventions section
- `physical_schema.sql` (DDL from Lesson 03 Design phase)
- `ADR-001` (database choice)

### 🤖 Prompt

```markdown
# Role: Senior Database Engineer (PostgreSQL + Flyway)

# Task
Generate baseline Flyway migration scripts — the shared foundation ALL modules depend on.

# Context
- `coding_conventions.md` → database conventions: table naming, column naming, migration naming
- `physical_schema.sql` → the source DDL from Design phase
- Database: PostgreSQL (version per ADR-001)

# Scripts to Generate

## `V1__baseline_extensions.sql`
- Enable `uuid-ossp` or use `gen_random_uuid()` (PG 13+)

## `V2__create_shared_enums.sql`
- Extract shared enum types from `physical_schema.sql` that span multiple modules
- Example: user_role, status_type

## `V3__create_users_table.sql`
- Core `users` table (needed by Security/Auth)
- MUST include ALL audit columns matching `BaseEntity.java`:
  `id (UUID PK)`, `created_at`, `updated_at`, `created_by`, `updated_by`, `is_deleted`, `deleted_at`
- Performance indexes on: email, role, status
- Use `password_hash` (NOT `password`)
- Partial index: `WHERE is_deleted = FALSE`

# Constraints
- Flyway naming: `V{N}__{description}.sql` (double underscore)
- ❌ Do NOT create module-specific tables — Vertical Slice concern
- ❌ Do NOT use `CREATE TABLE IF NOT EXISTS` — Flyway version control handles this
- ✅ snake_case, plural table names
- ✅ Index naming: `idx_{table}_{column}`
- ✅ Include SQL comments explaining WHY each table/index exists

# Output
3 SQL files, ready for `src/main/resources/db/migration/`
```

### ✅ HITL Checklist
- [ ] Flyway naming convention correct
- [ ] Audit columns match `BaseEntity.java`
- [ ] UUID generation matches BaseEntity strategy
- [ ] Only shared/foundation tables — no module tables
- [ ] `password_hash` (not `password`)
- [ ] Soft delete columns present
- [ ] `mvn spring-boot:run` → Flyway migrations succeed

---

## Quy trình thực hiện

```
P1 Scaffolding → P2 BaseEntity → P3 BaseResponse → P4 Exceptions → P5 Security → P6 Flyway
     │                                                                                │
     └─────────────── Mỗi prompt attach context files thay vì lặp lại ───────────────┘
                                                                                       │
                                                                                       ▼
                                                                            ✅ Ready for  
                                                                            Vertical Slice
```

### Final Validation (sau khi hoàn thành cả 6 prompts)

```bash
# 1. Build thành công
mvn clean compile

# 2. Application starts + Flyway runs
mvn spring-boot:run

# 3. Health check
curl http://localhost:8080/api/v1/actuator/health  # → 200

# 4. Error handler test
curl http://localhost:8080/api/v1/nonexistent      # → BaseResponse format 404
```

> [!CAUTION]
> **Chỉ khi 4 bước trên PASS, mới chuyển sang Vertical Slice.**

---

## Tổng kết: Infrastructure Checklist

| ✅ | Component | Prompt | Attach Files |
|----|-----------|:------:|-------------|
| ☐ | Scaffolding + Config | **P1** | L1 + L3 + ADR |
| ☐ | BaseEntity | **P2** | L3 |
| ☐ | BaseResponse\<T\> | **P3** | L2 + L3 |
| ☐ | Exceptions + Handler | **P4** | L2 + L3 + BaseResponse |
| ☐ | Security (JWT) | **P5** | L2 + L3 |
| ☐ | Flyway Baseline | **P6** | L3 + DDL + ADR |

> [!TIP]
> **Lean Prompt hoạt động tốt khi và chỉ khi** context files đã đầy đủ.
> Nếu `coding_conventions.md` thiếu package structure → AI sẽ tự bịa → output sai.
> Luôn kiểm tra context files TRƯỚC khi bắt đầu prompting.
