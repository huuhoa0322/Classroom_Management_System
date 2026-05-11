# Classroom Management System (CLS)

A full-stack web application for managing English & Programming tutoring centers. CLS centralizes student lifecycle management, class scheduling, attendance tracking, financial operations, and parent communication — replacing fragmented spreadsheets and manual processes.

> **Status:** MVP Phase 1 (13/04/2026 – 24/04/2026)

---

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Environment Variables](#-environment-variables)
- [Deployment](#-deployment)
- [API Design Conventions](#-api-design-conventions)
- [Code Conventions](#-code-conventions)
- [Notes](#-notes)

---

## ✨ Features

### Admin Portal

| Module | Description |
|--------|-------------|
| **Dashboard** | Overview of key metrics: total students, active classes, pending payments, renewal alerts |
| **Student Management** | CRUD student profiles (name, DOB, email, phone, address); track enrollment history and parent info |
| **Class Management** | Create/edit classes with tuition config, session count, format; assign teachers and manage student enrollment |
| **Session Management** | Schedule time slots across the week (Mon – Sun); link classes to rooms and teachers per session |
| **Room Management** | Manage room inventory with capacity and equipment configuration |
| **Tuition Packages** | Define packages (name, price, session quota, validity period); assign packages to students upon enrollment |
| **User Management** | Manage staff accounts (Admin / Teacher) with role-based access; reset passwords, toggle active status |
| **Attendance** | Mark attendance per session: Present / Absent / Late |
| **Feedback** | Teachers submit academic feedback per student per session |
| **Financial Management** | View student financial details, record and confirm payments, manage payment lifecycle |
| **Renewal Alerts** | System-generated alerts when student packages reach 30% / 10% remaining sessions; track consultation status |

### Teacher Portal

| Module | Description |
|--------|-------------|
| **Timetable** | Personal weekly teaching schedule with year/week filters and week navigation |
| **Attendance** | Take attendance for assigned sessions |
| **Feedback** | Submit per-student academic feedback within 12-hour SLA |

### Cross-Cutting Concerns

- **JWT Authentication** with access/refresh token flow
- **Role-based routing** — Admin → Dashboard, Teacher → Timetable
- **Real-time notifications** via SignalR
- **Automated email notifications** for attendance and schedule changes (MailKit + SMTP)
- **Background services** — Package depletion scanning and email dispatch queue
- **Global error handling** with connection error banners and toast notifications

---

## 🛠 Tech Stack

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| React | 19 | UI library |
| Vite | 8 | Build tool & dev server |
| TailwindCSS | 4 | Utility-first CSS framework |
| React Router | 7 | Client-side routing with guards |
| TanStack Query | 5 | Server state management, caching, and data fetching |
| Zustand | 5 | Client-side global state (auth, toast, notifications) |
| Axios | 1.x | HTTP client with interceptors (401 handling, retry) |
| React Hook Form + Zod | 7 / 4 | Form handling and schema validation |
| SignalR Client | 10 | Real-time WebSocket communication |

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | .NET 10 | Web API framework |
| Entity Framework Core | 10 | ORM with code-first migrations |
| PostgreSQL (Npgsql) | 16 | Relational database |
| Serilog | 10 | Structured logging (Console + File sinks) |
| AutoMapper | 16 | Object-to-object mapping (Entity ↔ DTO) |
| FluentValidation | 12 | Request validation |
| BCrypt.NET | 4 | Password hashing |
| MailKit | 4 | SMTP email delivery |
| Scalar | 2.4 | Interactive API documentation (OpenAPI) |
| SignalR | Built-in | Real-time server push |

### Infrastructure

| Tool | Purpose |
|------|---------|
| Docker | Containerized backend deployment |
| Docker Compose | Local multi-service orchestration |
| Render.com | Backend hosting (IaC via `render.yaml`) |
| Vercel | Frontend hosting |
| GitHub Actions | CI pipeline (build & test) |
| Supabase | Managed PostgreSQL database |

---

## 🏗 Architecture

The backend follows a **3-layer architecture**:

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

The frontend follows a **feature-based modular architecture**:

```
React App → AppRouter → Layouts → Feature Pages → Feature Components
                ↕                       ↕
           Guards (Auth)         Services (API) + Hooks (TanStack Query)
                ↕                       ↕
           Zustand Stores        Shared Components & Utilities
```

---

## 📁 Project Structure

### Repository Root

```
Classroom_Management_System/
├── .github/workflows/         # CI/CD pipeline configs
├── Developments/CLS/          # Source code (monorepo)
│   ├── CLS.Server/            # ASP.NET Core Web API
│   ├── CLS.BLL/               # Business Logic Layer
│   ├── CLS.DAL/               # Data Access Layer
│   ├── cls.client/            # React SPA
│   ├── Dockerfile             # Production Docker image
│   ├── docker-compose.yml     # Local orchestration
│   └── CLS.slnx               # .NET solution file
├── Documents/                 # Project documentation
│   ├── 00_Workflows/          # Process workflows
│   ├── 01_Business/           # Business requirements
│   ├── 02_Requirements/       # Functional & non-functional specs
│   ├── 03_Design/             # System design & UI mockups
│   ├── 04_Project_Management/ # Sprint planning & tracking
│   └── 05_ADR/                # Architecture Decision Records
├── Templates/                 # Document templates
├── Testing/                   # Test documentation
│   ├── 01_Unit_Test/
│   ├── 02_Integration_Test/
│   └── 03_System_Test/
├── render.yaml                # Render.com IaC blueprint
└── README.md                  # ← You are here
```

### Backend (`Developments/CLS/`)

```
CLS.Server/                    # API Host
├── Controllers/               # REST API endpoints
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
└── Program.cs                 # App configuration & DI registration

CLS.BLL/                       # Business Logic
├── Services/                  # Business service implementations
├── Interfaces/                # Service contracts (ISomethingService)
├── DTOs/                      # Data Transfer Objects
├── Validators/                # FluentValidation rules
├── Mappings/                  # AutoMapper profiles
└── Common/                    # Shared exceptions & helpers

CLS.DAL/                       # Data Access
├── Entities/                  # EF Core entity models (14 entities)
├── Repositories/              # Repository pattern implementations
├── Configurations/            # EF Core entity type configurations
├── Migrations/                # Database migration history
├── Data/                      # DbContext
└── Common/                    # Base entities & shared DB utilities
```

### Frontend (`Developments/CLS/cls.client/`)

```
src/
├── app/                       # Application shell
│   ├── layouts/               # AuthLayout, MainLayout (sidebar + header)
│   ├── guards/                # PrivateRoute (auth gate)
│   ├── provider/              # authStore (Zustand)
│   └── routers/               # AppRouter (all route definitions)
│
├── features/                  # Feature modules (domain-driven)
│   ├── auth/                  # Login page, useAuth hook
│   ├── academic/              # Dashboard page
│   ├── student/               # Student CRUD
│   ├── class/                 # Class management
│   ├── session/               # Session/scheduling management
│   ├── room/                  # Room management
│   ├── package/               # Tuition package management
│   ├── user/                  # User/staff management
│   ├── attendance/            # Attendance marking + Timetable
│   ├── feedback/              # Student feedback (list + form)
│   ├── financial/             # Student financials + Payment management
│   ├── retention/             # Renewal alerts
│   ├── schedule/              # Schedule-related features
│   └── parent/                # Parent-related features
│
├── shared/                    # Cross-cutting shared code
│   ├── components/            # ErrorBoundary, Toast, ConnectionErrorBanner
│   ├── hooks/                 # useNotificationHub (SignalR)
│   ├── services/              # apiClient (Axios instance + interceptors)
│   ├── stores/                # toastStore, notificationStore (Zustand)
│   └── utils/                 # constants, formatters
│
├── styles/                    # Global CSS
├── assets/                    # Static assets
├── App.jsx                    # Root component
└── main.jsx                   # Entry point
```

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version |
|------|---------|
| Node.js | ≥ 18 |
| .NET SDK | 10.0 |
| PostgreSQL | 16+ (or use Supabase) |
| Docker *(optional)* | Latest |

### 1. Clone the Repository

```bash
git clone https://github.com/TuanKiet0704/Classroom_Management_System.git
cd Classroom_Management_System
```

### 2. Backend Setup

```bash
cd Developments/CLS

# Restore NuGet packages
dotnet restore CLS.Server/CLS.Server.csproj

# Configure secrets (local development)
cd CLS.Server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<host>;Database=<db>;Username=<user>;Password=<pass>;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "JwtSettings:SecretKey" "<your-256-bit-secret>"

# Apply EF Core migrations
dotnet ef database update --project ../CLS.DAL --startup-project .

# Run the backend
dotnet run
```

The API will be available at `https://localhost:7065` (development) with Scalar API docs at `/scalar/v1`.

### 3. Frontend Setup

```bash
cd Developments/CLS/cls.client

# Install dependencies
npm install

# Start dev server
npm run dev
```

The frontend dev server runs at `https://localhost:57264` and proxies `/api/*` requests to the backend automatically.

### 4. Docker Setup (Alternative)

```bash
cd Developments/CLS

# Copy and configure environment file
cp .env.docker.example .env

# Mode 1: Backend + Supabase (remote DB)
docker compose up -d cls-api

# Mode 2: Backend + Local PostgreSQL
docker compose --profile local-db up -d
```

---

## 🔐 Environment Variables

### Backend (`CLS.Server`)

| Variable | Description | Required |
|----------|-------------|----------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string (Npgsql format) | ✅ |
| `JwtSettings__SecretKey` | JWT signing key (min 32 chars, high entropy) | ✅ |
| `JwtSettings__Issuer` | JWT issuer claim | Default: `cls-api` |
| `JwtSettings__Audience` | JWT audience claim | Default: `cls-client` |
| `JwtSettings__AccessTokenExpiryMinutes` | Access token TTL | Default: `60` |
| `JwtSettings__RefreshTokenExpiryDays` | Refresh token TTL | Default: `7` |
| `AllowedOrigins` | CORS allowed origins | Default: `http://localhost:5173` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Default: `Production` |

> **Local development:** Use `dotnet user-secrets` instead of `.env` files. See `.env.example` for the full template.

### Frontend (`cls.client`)

| Variable | Description | File |
|----------|-------------|------|
| `VITE_API_BASE_URL` | API base URL (proxied via Vite in dev) | `.env.local` |

---

## 🌐 Deployment

### Backend — Render.com

The backend is deployed as a Docker web service on [Render.com](https://render.com):

- **IaC Blueprint:** `render.yaml` at repo root auto-configures the service
- **Region:** Singapore (lowest latency for Vietnam)
- **Health Check:** `GET /health`
- **Auto-deploy:** Triggered on push to `master` branch
- **Secrets:** `ConnectionStrings__DefaultConnection` and `JwtSettings__SecretKey` must be set manually in the Render dashboard

### Frontend — Vercel

The React SPA is deployed on [Vercel](https://vercel.com):

- **Framework:** Vite (auto-detected)
- **Root Directory:** `Developments/CLS/cls.client`
- **SPA Rewrites:** Configured in `vercel.json`

### CI/CD Pipeline

- **GitHub Actions** (`.github/workflows/cls-backend.yml`): Runs on push/PR to `master` — restores, builds, and tests the backend
- **Render.com**: Handles CD (Docker build → deploy → health check → SSL)
- **Vercel**: Handles frontend CD with preview deployments on PRs

---

## 📐 API Design Conventions

All APIs follow these standardized rules:

- **Base URL:** `/api/v1`
- **Path naming:** `kebab-case` (e.g., `/api/v1/renewal-alerts`)
- **Resource-oriented:** Plural nouns for collections (e.g., `/students`, `/classes`)
- **Authentication:** JWT Bearer token in `Authorization` header
- **Standard response envelope:**

```json
{
  "code": 200,
  "message": "Operation successful",
  "data": { }
}
```

Full API documentation is available via **Scalar UI** at `/scalar/v1` when running the backend.

---

## 📝 Code Conventions

### Feature-Based Module Structure

- Each business domain → its own folder under `src/features/`
- Each feature contains: `pages/`, `components/`, `hooks/`, `services/`, `schemas/`
- Minimize exports; keep modules self-contained

### Routing

- All routes defined in `AppRouter.jsx`
- Protected routes wrapped with `<PrivateRoute>` guard
- Role-based index: Admin → Dashboard (`/`), Teacher → Timetable (`/timetable`)

### Data Fetching & State

- **Server state:** TanStack Query for all API calls (caching, retry, invalidation)
- **Client state:** Zustand for global UI state only (`authStore`, `toastStore`, `notificationStore`)
- **API layer:** Feature-specific services under `src/features/{module}/services/`
- **Custom hooks:** Feature-specific hooks under `src/features/{module}/hooks/`

### Backend Layering

- **Controllers** → thin, delegate to services
- **Services (BLL)** → business logic, validation, mapping
- **Repositories (DAL)** → data access via EF Core, no business logic
- All services are registered via DI and consumed through interfaces

### Error Handling

- **Backend:** `ExceptionHandlingMiddleware` + `ApiExceptionFilter` for global exception handling
- **Frontend:** `ErrorBoundary` component + `ConnectionErrorBanner` for transient errors + `Toast` for user notifications

---

## 📄 Notes

This project is developed as part of the **AISDLC - AI Agent for Software Development Lifecycle** workshop at FPT University Hanoi.

- **Instructor:** Nguyễn Thị Điệu
- **Author:** Đỗ Hữu Hòa - HE186716