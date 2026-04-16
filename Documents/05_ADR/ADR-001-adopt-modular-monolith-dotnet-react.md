# ADR-001: Adopt Modular Monolith with ASP.NET + React Tech Stack

| Field        | Value                                                   |
| ------------ | ------------------------------------------------------- |
| **Date**     | 2026-04-16                                              |
| **Status**   | Accepted                                                |
| **Author(s)**| Solution Architect (SA Lead)                            |
| **Applies to**| CLS (Classroom Management System) — MVP Project |

---

## 1. Context

### 1.1 Business Context (Bối cảnh Nghiệp vụ)

We are building the **Center Management System (CLS)** to digitize operations for an English and Kids Coding Center. The goal is to replace manual Excel-based management to resolve administrative bottlenecks, improve parent communication, mitigate student churn risk, and optimize academic scheduling.

| Project | Domain | Scale | Key Users | MVP Timeline |
|---------|--------|-------|-----------|--------------|
| **CLS** | EdTech / Center Management | 1 branch, 50-150 active learners | Academic Admin, Teacher, Parent | 3 weeks (Phase 1 MVP) |


### 1.2 Technical Constraints (Ràng buộc Kỹ thuật)

| Constraint | Detail |
|------------|--------|
| **Team Size** | 1 PM, 1 UI/UX, 1 QA, 2 Fullstack Developers |
| **Team Skill** | C#/.NET proficient. React proficient. Limited DevOps experience. |
| **Budget** | Minimal hosting budget (VPS or free tiers) and Email API (SendGrid). |
| **Timeline** | 3 weeks for MVP Phase 1 (Strict deadline). No room for complex infra setup. |
| **Concurrent Users** | ~50-150 active users max. No flash-sale or viral-traffic scenarios. |
| **Data Sensitivity** | CLS requires standard PII protection and financial data security. |

### 1.3 Problem Statement

> **We need a reliable architecture pattern and tech stack that the team can adopt with minimal ramp-up time**, ensuring fast delivery of the 3-week MVP, low hosting cost, and sufficient extensibility for post-MVP growth (like scale-up or advanced features).

---

## 2. Options Considered (Các Phương án Đánh giá)

### Option A: Microservices (.NET Aspire + K8s)

| Criterion | Assessment |
|-----------|-----------|
| Infra Cost | $150-400/mo minimum (AKS/EKS + service mesh + monitoring) |
| Team Fit | Requires K8s, Docker orchestration, distributed tracing — Junior team cannot operate |
| Time-to-Market | 3-4 months for infra alone before any business logic |
| Scalability | Excellent horizontal scaling per service |
| **Verdict** | **REJECTED** — Catastrophically over-engineered for 50-500 CCU MVPs. Kills both budget and timeline. |

### Option B: Serverless (Azure Functions + API Management)

| Criterion | Assessment |
|-----------|-----------|
| Infra Cost | Pay-per-invoke ($5-20/mo at MVP scale) |
| Team Fit | Requires Azure Functions + Bicep/ARM experience. Cold-start debugging is non-trivial. |
| Time-to-Market | 6-8 weeks with learning curve |
| Scalability | Auto-scales to zero |
| Local Dev | Azure Functions Core Tools adds significant development friction |
| **Verdict** | **REJECTED** — Cold-start latency (3-8s) unacceptable for real-time booking UX. Vendor lock-in contradicts academic portability goals. |

### Option C: Modular Monolith (ASP.NET + React) — Recommended

| Criterion | Assessment |
|-----------|-----------|
| Infra Cost | $20-40/mo (single VPS: DigitalOcean/Vultr) |
| Team Fit | C#/.NET is university curriculum. React is mainstream. Zero ramp-up. |
| Time-to-Market | 3-4 weeks to production-ready MVP |
| Scalability | Vertical scaling only, but sufficient for 500 CCU |
| Maintainability | Clear module boundaries. Can extract to microservices post-MVP if needed. |
| Local Dev | `dotnet run` + `npm run dev` — works on any laptop |
| **Verdict** | **APPROVED** — Best ROI. Fastest time-to-market. Team-ready. |

---

## 3. Decision (Quyết định)

**We will adopt the Modular Monolith architecture pattern with the following standardized tech stack for all 5 MVP projects.**

### 3.1 Architecture Pattern

```text
┌──────────────────────────────────────────────────────┐
│                    CLIENT LAYER                      │
│            React 18 + Vite (SPA) + JS                │
│         Tailwind CSS + Headless UI / Shadcn/ui       │
└──────────────────────┬───────────────────────────────┘
                       │ HTTPS / REST API
┌──────────────────────▼───────────────────────────────┐
│                 PRESENTATION LAYER (API)             │
│          ASP.NET Core 10 (Controllers, Filters)      │
│         JWT Auth Middleware ← ASP.NET Identity       │
├──────────────────────▼───────────────────────────────┤
│                BUSINESS LOGIC LAYER (BLL)            │
│         Services, Domain Logic (Academic, Finance,   │
│         Parent Portal, Notifications), Validators    │
├──────────────────────▼───────────────────────────────┤
│                 DATA ACCESS LAYER (DAL)              │
│      EF Core 10 (DbContext, Repositories, Entities)  │
│                PostgreSQL (Supabase)                 │
└──────────────────────────────────────────────────────┘
```

### 3.2 Full Tech Stack

#### Backend

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Runtime** | .NET | 10 LTS | Long-term support until Nov 2026. Cross-platform. |
| **Framework** | ASP.NET Core Web API | 10.0 | High-performance Kestrel server. Minimal API + Controller support. Industry standard for C#. |
| **Security** | ASP.NET Identity + JWT Bearer | — | Built-in user management. Stateless auth via `Microsoft.AspNetCore.Authentication.JwtBearer`. |
| **ORM** | Entity Framework Core | 10.0 | Code-First approach. LINQ queries. Auto-migration. |
| **Database** | PostgreSQL | 15+ | Hosted on Supabase. Scalable. `Npgsql.EntityFrameworkCore.PostgreSQL` provider. |
| **Migration** | EF Core Migrations | — | `dotnet ef migrations add` / `dotnet ef database update`. Version-controlled schema. |
| **Validation** | FluentValidation | — | Fluent, testable validation rules. Cleaner than Data Annotations for complex logic. |
| **API Docs** | Swashbuckle (Swagger UI) | — | Auto-generated OpenAPI documentation from controller attributes. |
| **Mapping** | AutoMapper | — | DTO ↔ Entity mapping. Reduces boilerplate code. |
| **Logging** | Serilog | — | Structured logging. Sinks for Console, File, Seq. |
| **Build Tool** | dotnet CLI | — | `dotnet build`, `dotnet run`, `dotnet publish`. IDE-agnostic. |
| **Message Queue** | RabbitMQ + MassTransit (Optional) | — | For async notifications (email/SMS). MassTransit provides clean abstraction. |
| **Caching** | Redis (Optional) | — | Session caching, rate limiting. Post-MVP consideration. `StackExchange.Redis` client. |

#### Frontend

| Layer | Technology | Version | Justification |
|-------|-----------|---------|---------------|
| **Framework** | React | 19 | Component-based. Largest community. Team knows it. |
| **Build Tool** | Vite | 8 | Lightning-fast HMR. 10x faster than CRA. |
| **Styling** | Tailwind CSS | 4 | Utility-first. Rapid UI development. No custom CSS files needed. |
| **UI Components** | Shadcn/ui or Headless UI | — | Accessible, unstyled primitives that integrate perfectly with Tailwind. |
| **State Mgmt** | Zustand or React Context | — | Lightweight. No Redux boilerplate for MVP scale. |
| **HTTP Client** | Axios | — | Interceptors for JWT token refresh. |
| **Routing** | React Router | 7 | Standard SPA routing. |
| **Language** | JavaScript | ES6+ | Native JS for faster integration and lower barrier to entry without TS compile overhead. |
| **Form Handling** | React Hook Form + Zod | — | High-performance form validation. Zod for schema-based validation. |
| **Data Fetching** | TanStack Query (React Query) | 5+ | Server state management. Auto-caching, refetching, pagination. |
| **Icons** | Lucide React | — | Clean, consistent icon set. Tree-shakeable. |

#### DevOps & Infrastructure

| Layer | Technology | Justification |
|-------|-----------|---------------|
| **VCS** | Git + GitHub | Student-familiar. Free CI/CD via GitHub Actions. |
| **CI/CD** | GitHub Actions | Auto-build + deploy on push to `main`. |
| **Hosting (BE)** | DigitalOcean Droplet / Railway / Azure App Service (Free tier) | $12-24/mo. Simple deployment. Azure free tier for .NET. |
| **Hosting (FE)** | Vercel / Netlify (Free tier) | Zero-config React deployment. Auto HTTPS. |
| **DB Hosting** | Managed MySQL (PlanetScale / DO) / Railway | Free tier available. Auto backups. |
| **Containerization** | Docker + Docker Compose | Local dev parity. Single `docker-compose up`. |
| **Monitoring** | ASP.NET Health Checks + Uptime Robot | Built-in `/health` endpoint. Free uptime monitoring. |

### 3.3 Project Structure (Chuẩn thư mục)

```text
project-root/
├── backend/
│   ├── src/
│   │   ├── CLS.API/                      ← Presentation Layer (ASP.NET Web API)
│   │   │   ├── Controllers/
│   │   │   ├── Middlewares/              ← JWT, Exception handling, CORS
│   │   │   ├── Filters/                  ← Action filters, validation filters
│   │   │   ├── Program.cs                ← App entry point + DI configuration
│   │   │   └── appsettings.json
│   │   ├── CLS.BLL/                      ← Business Logic Layer (Services, Data Transfer Objects)
│   │   │   ├── Services/                 ← Core business rules (e.g., Scheduling, Notifications)
│   │   │   ├── DTOs/                     ← Data Transfer Objects
│   │   │   ├── Interfaces/               ← Service contracts
│   │   │   └── Validators/               ← FluentValidation rules
│   │   ├── CLS.DAL/                      ← Data Access Layer (EF Core, Database access)
│   │   │   ├── Data/                     ← AppDbContext.cs
│   │   │   ├── Entities/                 ← Domain Entities
│   │   │   ├── Repositories/             ← Generic & Specific Repositories
│   │   │   ├── Migrations/               ← EF Core auto-generated migrations
│   │   │   └── Configurations/           ← Entity type configurations (Fluent API)
│   │   └── CLS.sln
│   └── Dockerfile
├── frontend/
│   ├── src/
│   │   ├── app/                          ← App shell: routing, guards, layouts
│   │   │   ├── guards/                   ← Auth guards (PrivateRoute, RoleGuard)
│   │   │   ├── layouts/                  ← Page layouts (MainLayout, AuthLayout)
│   │   │   ├── provider/                 ← Context providers (AuthProvider, ThemeProvider)
│   │   │   ├── routers/                  ← Route definitions (AppRouter.jsx)
│   │   │   └── App.jsx                   ← Root component
│   │   ├── assets/                       ← Static files: images, icons, fonts
│   │   ├── features/                     ← Feature modules (academic/, student/, parent/)
│   │   ├── shared/                       ← Reusable cross-feature code
│   │   │   ├── components/               ← Common UI components (Button, Modal, Table)
│   │   │   ├── hoc/                      ← Higher-Order Components (withAuth, withLoading)
│   │   │   ├── hooks/                    ← Custom hooks (useAuth, useFetch, useDebounce)
│   │   │   ├── services/                 ← Axios API clients
│   │   │   └── utils/                    ← Helper functions (formatDate, validators)
│   │   └── styles/                       ← Tailwind config + global CSS
│   ├── tailwind.config.js
│   ├── postcss.config.js
│   ├── package.json
│   └── vite.config.js
├── docs/
│   ├── adr/                              ← Architecture Decision Records
│   └── api/                              ← OpenAPI YAML specs
├── docker-compose.yml
└── README.md
```

---

## 4. Consequences (Hệ quả)

### Positive (Tích cực)

| # | Impact | Detail |
|---|--------|--------|
| 1 | **Fastest time-to-market** | Single deployable unit. No inter-service communication overhead. MVP in 3-4 weeks. |
| 2 | **Lowest infrastructure cost** | 1 VPS ($20-40/mo) runs everything. No K8s, no service mesh, no API gateway infra. |
| 3 | **Zero team ramp-up** | C#/.NET + React are university curriculum. Team is productive from Day 1. |
| 4 | **Simple debugging** | Single process. Stack traces are linear. No distributed tracing needed. |
| 5 | **Reusable across 5 projects** | Same pattern, same stack. Teams can share knowledge, code templates, and troubleshooting guides. |
| 6 | **Clear upgrade path** | Module boundaries are enforced via .NET projects (Class Libraries). If any module needs to scale independently post-MVP, extract it as a separate service with minimal refactoring. |
| 7 | **Tailwind CSS productivity** | Utility-first approach eliminates CSS context-switching. Consistent design system across all 5 projects without writing custom stylesheets. |
| 8 | **Clean Architecture ready** | 4-project structure (API → Application → Domain → Infrastructure) follows Clean Architecture by default. Easy to maintain and test. |

### Negative (Tiêu cực / Nợ kỹ thuật)

| # | Risk | Mitigation |
|---|------|------------|
| 1 | **Single point of failure** | If the monolith crashes, all modules go down. **Mitigation:** Health checks via ASP.NET Health Checks + auto-restart via systemd/Docker restart policy. |
| 2 | **Vertical scaling limit** | Cannot scale individual modules independently. **Mitigation:** At MVP scale (50-500 CCU), a $40/mo VPS handles this easily. Revisit at 5,000+ CCU. |
| 3 | **Deployment coupling** | Any module change requires full redeployment. **Mitigation:** CI/CD pipeline deploys in < 2 minutes. Blue-green deployment via Docker if zero-downtime is critical. |
| 4 | **Module boundary discipline** | Developers may accidentally bypass module boundaries (direct DB queries across modules). **Mitigation:** Code review checklist + `internal` access modifiers enforced via .NET project references. |
| 5 | **MySQL limitations vs PostgreSQL** | MySQL lacks advanced features like JSON indexing, CTEs performance, and row-level security compared to PostgreSQL. **Mitigation:** For MVP scale, MySQL is more than sufficient. Migrate to PostgreSQL post-MVP only if advanced features are required. |

### Neutral (Trung lập)

- Database schema migrations are managed via EF Core Migrations — same operational overhead regardless of architecture pattern.
- JWT stateless authentication works identically across monolith and microservices — no migration cost if pattern changes.
- Tailwind CSS is framework-agnostic — if team migrates to Next.js or Vue.js in future, the Tailwind knowledge transfers 100%.

---

## 5. Compliance & Verification (Kiểm chứng)

| # | Verification Action | Owner | Deadline |
|---|-------------------|-------|----------|
| 1 | Scaffold base project template (.sln with 4 projects) + Docker Compose with MySQL | Tech Lead | Sprint 0 (Week 1) |
| 2 | Load test: Verify 500 CCU sustained via k6/Locust on staging VPS | Backend Team | Sprint 1 |
| 3 | Security audit: Confirm JWT implementation + password hashing (BCrypt via ASP.NET Identity) + CORS policy | Tech Lead | Sprint 1 |
| 4 | HCMS-specific: Verify MySQL encryption at rest + AES-256 for EMR fields via EF ValueConverters | Backend Team | Sprint 1 |
| 5 | CI/CD pipeline: GitHub Actions → `dotnet publish` → Docker build → Deploy to staging on push to `develop` | DevOps | Sprint 0 |

---

## 6. Related Documents (Tài liệu Liên quan)

| Document | Path | Relationship |
|----------|------|-------------|
| ADR-002 | `docs/adr/ADR-002-use-postgresql.md` | Database selection decision |
| ADR-003 | `docs/adr/ADR-003-jwt-authentication.md` | Authentication strategy |
| CLS Business Goals | `02_Projects/CLS/CLS_Business_Goals_v0.1.html` | NFR source: 3-week MVP, parent portal |


---

## 7. Decision Log (Nhật ký Quyết định)

| Date | Action | By |
|------|--------|----|
| 2026-04-16 | ADR-001 created. Status: **Accepted**. | SA Lead |
| — | *Future: If any project exceeds 5,000 CCU, create ADR-00X to evaluate Microservices extraction.* | — |

---

> **Architect's Note:** This ADR intentionally constrains all 5 projects to one pattern and one stack. The goal is **consistency over optimization**. A team that masters one stack delivers 3x faster than a team that experiments with 5 different stacks. For MVP-phase products serving < 500 users, a well-structured monolith is not a compromise — it is the optimal engineering decision.
