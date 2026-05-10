# 📋 PM/PO Backlog Review — Classroom Management System (CLS)

> **Reviewer Role:** Product Manager / Product Owner  
> **Review Date:** 2026-04-20  
> **Backlog file:** `Documents/04_Project_Management/01_Plans/CLS_Product_Backlog.csv`  
> **Source docs:** SRS (Report3), SDS (Report4), API YAML, Mockups (9 screens), Class Diagrams (7 modules)

---

## ✅ Kết luận tổng thể

| Tiêu chí | Đánh giá | Ghi chú |
|----------|----------|---------|
| Coverage UC (SRS) | ✅ Đủ | UC-01 → UC-11 đều có Story |
| Coverage Epic | ✅ Đủ | 9 Epics mapping đúng SRS sections |
| Coverage API (YAML) | ⚠️ Thiếu | Auth flow chưa có item nào |
| Coverage UI Screen | ⚠️ Thiếu | 9 mockups → chỉ có 7 UI tasks |
| Coverage DB | ⚠️ Thiếu | Seed/master data chưa có |
| Coverage SDS Detail Design | ❌ Thiếu | Class diagram review, package diagram chưa có |
| NFR coverage | ⚠️ Thiếu | Thiếu performance test, monitoring |
| DevOps/QA depth | ⚠️ Mỏng | CI gate, unit test chưa có |

---

## 🔍 Gap Analysis — Những gì đang thiếu

### 🔴 GAP 1 — Authentication (Thiếu hoàn toàn)

**API YAML** định nghĩa rõ `POST /api/v1/auth/login` với JWT Bearer auth. Đây là **prerequisite của toàn bộ hệ thống** nhưng backlog hiện tại **không có 1 item nào** về:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| Story: User Login / Logout | Infrastructure & DevOps | High |
| API: POST /api/v1/auth/login — issue JWT | Infrastructure & DevOps | High |
| UI: Login Screen (Admin & Teacher portals) | Infrastructure & DevOps | High |
| API: Refresh token / revoke session | Infrastructure & DevOps | Medium |

> [!CAUTION]
> Không có Login thì không thể test bất kỳ flow nào. Đây là blocker cho Sprint 1.

---

### 🔴 GAP 2 — Master Data Management (Thiếu hoàn toàn)

SDS Section 1.3 định nghĩa **14 tables**, bao gồm các bảng master data cần được seed trước khi dùng:

| Bảng master | Cần CRUD | Status hiện tại |
|-------------|----------|----------------|
| `users` (Admin/Teacher accounts) | Create, Read, Update | ❌ Không có |
| `classes` (Danh mục lớp học) | CRUD | ❌ Không có |
| `rooms` (Danh sách phòng học) | CRUD | ❌ Không có |
| `packages` (Gói học phí) | CRUD | ❌ Không có |

**Nhận xét PM:** UC-04 yêu cầu Admin chọn Room và Teacher khi tạo Session — nhưng hiện backlog không có item nào để *tạo* Room, Teacher accounts hay Package catalogue. Không thể demo hoặc test UC-04 nếu thiếu dữ liệu này.

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| Story: Manage Teacher Accounts | Student Management (hoặc tạo Epic mới) | High |
| Story: Manage Class Catalogue | Schedule Management | High |
| Story: Manage Room Catalogue | Schedule Management | High |
| Story: Manage Package Catalogue | Financial Administration | High |
| TASK: Seed master data scripts (packages, rooms, test users) | Infrastructure & DevOps | High |

---

### 🟡 GAP 3 — Dashboard / Home Screen (Thiếu)

SRS Section 1.4.1 (Screen Flow) và Section 1.4.2 (Screen Authorization) mô tả **Admin Dashboard** là hub trung tâm. Backlog hiện có nhiều task "Từ Dashboard navigate đến..." nhưng không có item xây dựng Dashboard:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| Story: Admin Dashboard — overview widgets | Student Management | High |
| UI: Admin Dashboard Screen (renewal count, recent sessions) | Retention Management | High |
| UI: Teacher Dashboard / Home Screen | Academic Operations | High |

---

### 🟡 GAP 4 — Student List / Search Screen (Thiếu)

UC-02 mô tả Admin "searches and accesses the specific Student Detail screen" — nghĩa là cần một **Student List** với tìm kiếm. Backlog chỉ có Detail Screen task, không có List Screen:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| API: GET /api/v1/student-profiles — list & search students | Student Management | High |
| UI: Student List / Search Screen | Student Management | High |

---

### 🟡 GAP 5 — Payment History Screen (Thiếu)

UC-03 có màn hình ghi nhận payment nhưng thiếu màn hình xem **lịch sử giao dịch**:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| API: GET /api/v1/tuition-payments — list payment history | Financial Administration | Medium |
| UI: Payment History Screen / Transaction log | Financial Administration | Medium |

---

### 🟡 GAP 6 — Session List / Timetable cho Admin (Thiếu)

Admin cần xem được toàn bộ master timetable (không phải chỉ Teacher xem schedule của mình):

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| API: GET /api/v1/teaching-schedules — list all sessions (Admin view) | Schedule Management | Medium |
| API: GET/PUT/DELETE /api/v1/teaching-schedules/{id} — view & edit session | Schedule Management | Medium |
| UI: Master Schedule / Session List Screen | Schedule Management | Medium |

---

### 🟡 GAP 7 — Feedback Viewing (Admin side) (Thiếu)

UC-09 ở SDS (2.6) ghi rõ: *"UC-09: Manage Curriculum Feedback — how an Admin views aggregated feedback from teachers."* Backlog chỉ có Teacher submit feedback, không có Admin xem:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| API: GET /api/v1/academic-feedback — list feedback (Admin view) | Academic Quality Assurance | Medium |
| UI: Feedback Management Screen (Admin) | Academic Quality Assurance | Medium |

---

### 🟡 GAP 8 — Attendance History Viewing (Thiếu)

Sau khi Teacher submit attendance, cần có màn hình xem lại lịch sử điểm danh:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| API: GET /api/v1/class-attendances?sessionId={id} — view attendance records | Academic Operations | Medium |
| UI: Attendance History Screen / Session detail view | Academic Operations | Medium |

---

### 🟠 GAP 9 — SDS Detail Design Coverage (Thiếu trong backlog)

SDS đã có **7 Class Diagrams** và **11+ Sequence Diagrams**, nhưng backlog không theo dõi việc review/approve các design artifacts này:

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| TASK: Review & sign-off Class Diagrams (7 modules) | Infrastructure & DevOps | Medium |
| TASK: Review & sign-off Sequence Diagrams (11 UCs) | Infrastructure & DevOps | Medium |
| TASK: API contract review & alignment with YAML spec | Infrastructure & DevOps | Medium |

---

### 🟠 GAP 10 — Monitoring & Observability (NFR thiếu)

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| NFR: Set up application error logging (e.g., Sentry) | Non-Functional Requirements | Medium |
| NFR: Set up uptime monitoring dashboard (e.g., UptimeRobot) | Non-Functional Requirements | Medium |
| NFR: Cron job execution logging & alerting | Non-Functional Requirements | Medium |

---

### 🔵 GAP 11 — Unit Tests & Code Quality (Tech Debt)

| Item cần thêm | Epic | Priority |
|---------------|------|----------|
| TECH DEBT: Unit tests for BLL (Business Logic Layer) services | Infrastructure & DevOps | Medium |
| TECH DEBT: API contract tests (Pact or schema validation) | Infrastructure & DevOps | Low |
| TECH DEBT: Code linting and formatting rules (ESLint/Prettier config) | Infrastructure & DevOps | Low |

---

## 📊 Bảng tổng hợp items bổ sung đề xuất

| # | Summary | Issue Type | Epic | Priority | SP est. | Lý do |
|---|---------|-----------|------|----------|---------|-------|
| 1 | Story: User Login & Logout | Story | Infra & DevOps | High | 5 | Auth prerequisite |
| 2 | API: POST /api/v1/auth/login — issue JWT | Task | Infra & DevOps | High | 3 | Auth flow |
| 3 | UI: Login Screen | Task | Infra & DevOps | High | 3 | Auth UI |
| 4 | API: Token refresh / logout | Task | Infra & DevOps | Medium | 2 | Session mgmt |
| 5 | Story: Manage Teacher Accounts (CRUD) | Story | Student Mgmt | High | 5 | Master data |
| 6 | API: CRUD /api/v1/teachers | Task | Student Mgmt | High | 3 | Master data |
| 7 | UI: Teacher Account Management Screen | Task | Student Mgmt | High | 3 | Master data |
| 8 | Story: Manage Class Catalogue (CRUD) | Story | Schedule Mgmt | High | 5 | Master data |
| 9 | Story: Manage Room Catalogue (CRUD) | Story | Schedule Mgmt | High | 3 | Master data |
| 10 | Story: Manage Package Catalogue (CRUD) | Story | Financial Admin | High | 5 | Master data |
| 11 | TASK: Seed master data scripts | Task | Infra & DevOps | High | 2 | Test prerequisite |
| 12 | UI: Admin Dashboard Screen | Task | Retention Mgmt | High | 5 | Navigation hub |
| 13 | UI: Teacher Dashboard / Home Screen | Task | Academic Ops | High | 3 | Navigation hub |
| 14 | API: GET /api/v1/student-profiles (list+search) | Task | Student Mgmt | High | 2 | Missing list API |
| 15 | UI: Student List / Search Screen | Task | Student Mgmt | High | 3 | Missing screen |
| 16 | API: GET /api/v1/tuition-payments (history) | Task | Financial Admin | Medium | 2 | Audit trail |
| 17 | UI: Payment History Screen | Task | Financial Admin | Medium | 3 | Audit UI |
| 18 | API: GET /api/v1/teaching-schedules (admin list) | Task | Schedule Mgmt | Medium | 2 | Admin schedule view |
| 19 | UI: Master Schedule / Session List Screen | Task | Schedule Mgmt | Medium | 3 | Admin schedule UI |
| 20 | API: GET /api/v1/academic-feedback (admin view) | Task | Academic QA | Medium | 2 | Admin feedback view |
| 21 | UI: Feedback Management Screen (Admin) | Task | Academic QA | Medium | 3 | Admin feedback UI |
| 22 | API: GET attendance history per session | Task | Academic Ops | Medium | 2 | Audit trail |
| 23 | UI: Attendance History / Session Detail Screen | Task | Academic Ops | Medium | 3 | Missing screen |
| 24 | NFR: Application error logging (Sentry) | Task | NFR | Medium | 2 | Observability |
| 25 | NFR: Uptime monitoring dashboard | Task | NFR | Medium | 1 | Reliability NFR |
| 26 | NFR: Cron job execution logging & alerting | Task | NFR | Medium | 2 | Automation NFR |
| 27 | TECH DEBT: Unit tests for BLL services | Task | Infra & DevOps | Medium | 5 | Test coverage |

**Tổng thêm: 27 items mới / ~83 Story Points bổ sung**

---

## 📈 Backlog Size sau bổ sung

| | Trước | Sau |
|--|-------|-----|
| Epics | 9 | 9 (+0) |
| Stories | 12 | 20 (+8) |
| Tasks | 39 | 57 (+18) |
| Tech Debt / NFR tasks | 9 | 12 (+3) |
| **Tổng items** | **61** | **~88** |
| **Story Points** | ~162 | **~245** |

---

## 🗓️ Gợi ý Sprint Re-planning (sau bổ sung)

| Sprint | Ưu tiên | Key deliverables | SP |
|--------|---------|------------------|----|
| **Sprint 0** | Highest | Infra setup, DB migrations, Login, Seed data | ~35 |
| **Sprint 1** | High | Master data CRUD (Teacher, Class, Room, Package) + Student Mgmt | ~40 |
| **Sprint 2** | High | Schedule Mgmt (UC-04, UC-05) + Financial (UC-03) | ~35 |
| **Sprint 3** | High | Academic Ops (UC-07, UC-08) + Dashboard UIs | ~40 |
| **Sprint 4** | High | Academic QA (UC-09) + Retention (UC-06) + Automation (UC-10/11) | ~45 |
| **Sprint 5** | Medium | NFR hardening, Monitoring, E2E tests, Tech Debt | ~50 |

> [!TIP]
> Nên tách **Sprint 0 (Infrastructure Sprint)** riêng để unblock toàn bộ team develop từ Sprint 1. Sprint 0 cần hoàn thành: Supabase, DB schema, Auth, CI/CD, Seed data.

---

## ⚠️ Rủi ro Backlog hiện tại

> [!WARNING]
> **Rủi ro cao #1:** Login screen và Auth không có trong backlog → Team không có definition of done cho authentication flow → Toàn bộ Sprint 1 bị block.

> [!WARNING]
> **Rủi ro cao #2:** Master data (Rooms, Classes, Packages, Teacher accounts) thiếu → UC-04 "Setup Core Teaching Schedules" không thể demo vì không có data dropdown để select.

> [!NOTE]
> **Rủi ro trung bình:** SDS UC naming có sự khác biệt nhỏ với SRS. SDS ghi UC-07 = "Mark Student Attendance" còn SRS ghi UC-07 = "View Personalized Timetables". Cần align lại trước khi assign Stories vào Sprint.
