# CLS Testing Convention

> **Role:** Senior QA — Classroom Management System (CLS)  
> **Scope:** Unit Test (UT) · Integration Test (IT) · System Test (ST)  
> **Tech Stack:** ASP.NET Core 10 · EF Core 10 · PostgreSQL (Supabase) · React · xUnit · Moq · WebApplicationFactory  
> **Last Updated:** 2026-04-22  
> **Related:** [coding-conventions-backend.md](./coding-conventions/coding-conventions-backend.md) | [api-design-rules.md](./api-design-rules.md)

---

## Level-Specific Conventions

Chi tiết tech stack, naming, rules và output format cho từng test level — xem tại:

| Level | File | Output Report |
|-------|------|---------------|
| **Unit Test (UT)** | [unit-test-convention.md](./testing-conventions/unit-test-convention.md) | [Report5.1_CLS_Unit Test.xls](../Testing/01_Unit_Test/Report5.1_CLS_Unit%20Test.xls) |
| **Integration Test (IT)** | [integration-test-convention.md](./testing-conventions/integration-test-convention.md) | [Report5.2_CLS_Integration Test.xlsx](../Testing/02_Integration_Test/Report5.2_CLS_Integration%20Test.xlsx) |
| **System Test (ST)** | [system-test-convention.md](./testing-conventions/system-test-convention.md) | [Report5.3_CLS_System Test.xlsx](../Testing/03_System_Test/Report5.3_CLS_System%20Test.xlsx) |

---

## 1. General Rules (All Levels)

<general_rules>
  <rule id="TR-G01">Mỗi test case phải độc lập — không phụ thuộc thứ tự chạy.</rule>
  <rule id="TR-G02">Mỗi test case chỉ kiểm tra MỘT behavior cụ thể (Single Assertion Principle).</rule>
  <rule id="TR-G03">Không sử dụng Thread.Sleep() — dùng async/await hoặc mock thay thế.</rule>
  <rule id="TR-G04">Test data KHÔNG được hard-code giá trị production (email thật, số điện thoại thật).</rule>
  <rule id="TR-G05">Mỗi test class phải có [Trait("Category", "Unit|Integration|System")].</rule>
  <rule id="TR-G06">Sau mỗi test, phải dọn sạch state (Dispose, Respawn, hoặc InMemory reset).</rule>
</general_rules>

---

## 2. Test Case Type Classification

<test_case_types>

| Code | Type | Description |
|------|------|-------------|
| **N** | Normal | Happy path — đầu vào hợp lệ, luồng thành công |
| **A** | Abnormal | Đầu vào không hợp lệ / edge case / lỗi nghiệp vụ |
| **B** | Boundary | Giá trị biên (min, max, empty, null, length limit) |

</test_case_types>

---

## 3. Test Case ID Convention (Summary)

<test_id_convention>

| Level | Pattern | Example |
|-------|---------|---------|
| Unit Test | `UTC-{MODULE}-{NNN}` | `UTC-STU-001` |
| Integration Test | `ITC-{FEATURE}-{NNN}` | `ITC-STU-001` |
| System Test | `STC-{UCCODE}-{NNN}` | `STC-UC01-001` |

**Module / Feature Codes (shared):**

| Code | Domain |
|------|--------|
| STU | Student |
| TCH | Teacher |
| ENR | Enrollment |
| SCH | Schedule |
| ATT | Attendance |
| PKG | Package |
| PAY | Payment |
| FBK | Feedback |
| NTF | Notification |
| AUTH | Authentication |
| USR | User |

</test_id_convention>

---

## 4. Test Coverage Map (UC ↔ Test Level)

| Use Case | UC Name | UT (Service) | IT (API) | ST (Workflow) |
|----------|---------|:---:|:---:|:---:|
| UC-01 | Student Enrollment | ✅ EnrollmentService | ✅ POST /api/v1/enrollments | ✅ Workflow UC-01 |
| UC-02 | Attendance Tracking | ✅ AttendanceService | ✅ POST /api/v1/attendance | ✅ Workflow UC-02 |
| UC-03 | Schedule Management | ✅ ScheduleService | ✅ CRUD /api/v1/schedules | ✅ Workflow UC-03 |
| UC-04 | Payment / Package | ✅ PackageService | ✅ CRUD /api/v1/packages | ✅ Workflow UC-04 |
| UC-05 | Teacher Feedback | ✅ FeedbackService | ✅ POST /api/v1/feedbacks | ✅ Workflow UC-05 |
| UC-06 | Parent Notification | ✅ EmailService (mock) | ✅ Trigger check | ✅ Workflow UC-06 |
| UC-07 | User Management | ✅ UserService | ✅ CRUD /api/v1/users | ✅ Workflow UC-07 |
| UC-08 | Class Management | ✅ ClassService | ✅ CRUD /api/v1/classes | ✅ Workflow UC-08 |
| UC-09 | Report & Dashboard | ✅ ReportService | ✅ GET /api/v1/reports | ✅ Workflow UC-09 |
| UC-10 | Renewal Alert (Auto) | ✅ RenewalWorker | ✅ Worker integration | ✅ Workflow UC-10 |
| UC-11 | Attendance Email (Auto) | ✅ AttendanceEmailWorker | ✅ Worker integration | ✅ Workflow UC-11 |

---

## 5. Defect Severity Classification

<defect_classification>
  <severity level="Critical">
    Hệ thống crash, data bị mất, không thể login, Email không gửi được trong production.
    SLA: Fix trong 4 giờ.
  </severity>
  <severity level="Major">
    Tính năng core không hoạt động: trùng lịch không bị chặn, học phí tính sai, renewal alert không trigger.
    SLA: Fix trong 24 giờ.
  </severity>
  <severity level="Minor">
    UI hiển thị sai, label sai, format ngày không đúng, thông báo lỗi mơ hồ.
    SLA: Fix trong sprint tiếp theo.
  </severity>
  <severity level="Trivial">
    Typo, style, căn chỉnh UI.
    SLA: Backlog.
  </severity>
</defect_classification>

---

## 6. Quick Reference — File Locations

```
.ai-context/
├── testing_convention.md                    ← File này (thông tin chung)
└── testing-conventions/
    ├── unit-test-convention.md              ← UT: xUnit + Moq, UTC-ID, Report5.1
    ├── integration-test-convention.md       ← IT: WebApplicationFactory, ITC-ID, Report5.2
    └── system-test-convention.md            ← ST: Manual + Postman, STC-ID, Report5.3

CLS.Tests/
├── UnitTests/
│   ├── Services/          ← {ServiceName}Tests.cs
│   ├── Validators/        ← {ValidatorName}Tests.cs
│   └── Utilities/         ← {UtilName}Tests.cs
└── IntegrationTests/
    ├── Controllers/       ← {ControllerName}IntegrationTests.cs
    └── Fixtures/          ← TestWebApplicationFactory.cs

Testing/
├── 01_Unit_Test/
│   └── Report5.1_CLS_Unit Test.xls          ← UT output
├── 02_Integration_Test/
│   └── Report5.2_CLS_Integration Test.xlsx  ← IT output
└── 03_System_Test/
    └── Report5.3_CLS_System Test.xlsx       ← ST output
```

---

> **Maintained by:** QA Lead  
> **Related Docs:** [SRS](../Documents/02_Requirements/Report3_CLS_Software%20Requirement%20Specification.docx) | [SDS](../Documents/02_Requirements/Report4_CLS_Software%20Design%20Specification.docx) | [Backend Conventions](./coding-conventions/coding-conventions-backend.md)
