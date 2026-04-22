# CLS System Test Convention

> **Level:** System Test (ST)  
> **Role:** Senior QA — Classroom Management System (CLS)  
> **Last Updated:** 2026-04-22  
> **Parent:** [testing_convention.md](../testing_convention.md)  
> **Related:** [unit-test-convention.md](./unit-test-convention.md) | [integration-test-convention.md](./integration-test-convention.md)

---

## 1. Tech Stack

<techstack_system_test>
  <level>System Test (ST)</level>
  <purpose>Kiểm tra end-to-end toàn bộ workflow từ UI → API → DB theo Use Cases trong SRS</purpose>
  <frameworks>
    <framework>Manual Testing — dùng template Excel Report5.3, thực hiện thủ công theo step-by-step</framework>
    <framework>Postman Collection — API workflow testing cho các flow phức tạp</framework>
    <framework>Browser — Chrome/Edge cho UI flow, kiểm tra responsive</framework>
  </frameworks>
  <environment>Staging environment với PostgreSQL Supabase thực (không dùng InMemory)</environment>
  <run_approach>3 Testing Rounds (Round 1 bắt buộc, Round 2/3 cho regression)</run_approach>
  <coverage_target>
    <workflows>Toàn bộ UC-01 đến UC-11 theo SRS</workflows>
    <business_rules>Mỗi Business Rule (BR) phải có ít nhất 1 test case trace</business_rules>
    <roles>Admin, Teacher, Parent (email notification flow)</roles>
  </coverage_target>
</techstack_system_test>

---

## 2. Naming Conventions

<naming_system_test>

### 2.1 Document & Sheet Naming

| Element | Pattern | Example |
|---------|---------|---------|
| Output file | `Report5.3_CLS_System Test.xlsx` | — |
| Workflow Sheet | `{UCCode} {UCName}` | `UC-01 Student Enrollment` |
| Scenario Group | `Scenario {Alpha} — {Type}` | `Scenario A — Happy Path` |

### 2.2 Test Case ID

<naming_system_test_id>
  Pattern : STC-{WorkflowCode}-{SequentialNumber}
  Examples :
    STC-UC01-001 → System Test, Use Case 01, case 001
    STC-UC03-002 → System Test, Use Case 03, case 002
    STC-UC10-001 → System Test, UC-10 (Renewal Alert Automation), case 001
  Workflow Codes map to SRS Use Cases:
    UC-01 = Student Enrollment    | UC-02 = Attendance Tracking
    UC-03 = Schedule Management   | UC-04 = Package / Payment
    UC-05 = Teacher Feedback      | UC-06 = Parent Notification
    UC-07 = User Management       | UC-08 = Class Management
    UC-09 = Report & Dashboard    | UC-10 = Renewal Alert (Auto)
    UC-11 = Attendance Email (Auto)
</naming_system_test_id>

</naming_system_test>

---

## 3. Testing Rules

<system_test_rules>
  <rule id="TR-ST01">System Test thực hiện trên Staging environment — KHÔNG chạy trên Production.</rule>
  <rule id="TR-ST02">Mỗi Use Case trong SRS (UC-01 → UC-11) phải có ít nhất 1 Workflow sheet.</rule>
  <rule id="TR-ST03">Thực hiện tối thiểu 3 Testing Rounds; Round 1 bắt buộc pass trước go-live.</rule>
  <rule id="TR-ST04">Defect phát hiện phải ghi vào cột Note: BUG-{NNN}: {Short description} và log Bug Tracker.</rule>
  <rule id="TR-ST05">Test phải cover toàn bộ roles: Admin, Teacher, Parent (email flow).</rule>
  <rule id="TR-ST06">Mỗi Business Rule (BR) trong SRS phải được trace đến ít nhất 1 test case.</rule>
  <rule id="TR-ST07">Automation workflows (UC-10, UC-11) phải kiểm tra trigger condition thực tế trên staging.</rule>
</system_test_rules>

### Quy trình thực hiện ST

```
Bước 1: Chuẩn bị môi trường Staging (seed data, tài khoản test)
Bước 2: Thực hiện Round 1 — ghi kết quả vào cột "Round 1" + "Test date" + "Tester"
Bước 3: Log tất cả Defect (Failed) vào Bug Tracker với severity
Bước 4: Dev fix defect → Tester verify lại (Round 2)
Bước 5: Final regression — Round 3 trước go-live
Bước 6: Update Test Statistics sheet, chốt sign-off
```

---

## 4. Output Format — Report 5.3

<output_system_test>
  <template_file>Testing/03_System_Test/Report5.3_CLS_System Test.xlsx</template_file>

  <sheets>
    <sheet name="Cover">
      Project Name : CLS — Classroom Management System
      Project Code : CLS-2026
      Document Code: CLS-2026_ST_v{version}
      Creator      : {QA Lead}
      Issue Date   : {YYYY-MM-DD}
    </sheet>

    <sheet name="Test Cases (Index)">
      Mục đích: Liệt kê tất cả Workflow sheets
      Columns : No | Workflow/Use Case | Sheet Name | Description | Pre-Condition
      Map 1-1 với Use Cases trong SRS (UC-01 → UC-11)
    </sheet>

    <sheet name="Test Statistics">
      Mục đích: Tổng hợp kết quả theo Workflow và Round
      Columns : No | Module Code | Passed | Failed | Pending | N/A | Number of TCs
      Dùng formula tham chiếu từ Workflow sheets
    </sheet>

    <sheet name="{WorkflowName}">
      Header block:
        Workflow        : {UC Name} — vd: "UC-01 Student Enrollment"
        Test Requirement: {Business Rule / SRS section reference}
        Number of TCs   : =COUNTA(A12:A1000)

      Round summary block:
        Testing Round | Passed | Failed | Pending | N/A
        Round 1       | formula| formula| formula | formula
        Round 2       | formula| formula| formula | formula
        Round 3       | formula| formula| formula | formula

      Test Case table (from Row 10):
        Columns:
          Test Case ID | Test Case Description | Test Case Procedure
          | Expected Results | Pre-conditions
          | Round 1 | Test date | Tester
          | Round 2 | Test date | Tester
          | Round 3 | Test date | Tester
          | Note

        Group rows by Scenario:
          Scenario A — Happy Path
          STC-UC01-001 | Enroll student with valid data | Step 1... | 201 + email sent | Admin logged in
          Scenario B — Abnormal
          STC-UC01-002 | Enroll with duplicate student  | Step 1... | 409 Conflict    | Admin logged in
    </sheet>
  </sheets>

  <output_rules>
    <rule>STC ID unique toàn document: STC-{UCCODE}-{NNN}</rule>
    <rule>Mỗi Workflow sheet = 1 Use Case từ SRS, đặt tên sheet theo UC code</rule>
    <rule>Test Case Procedure liệt kê từng bước thao tác cụ thể (Step 1, Step 2, ...)</rule>
    <rule>Expected Results nêu: UI message + Email trigger (nếu có) + DB state thay đổi</rule>
    <rule>Pre-conditions nêu: role đăng nhập + dữ liệu seed cần thiết</rule>
    <rule>UC-10 và UC-11 (Automation): phải test trigger condition thực tế (student near-expiry, attendance submitted)</rule>
    <rule>Bug phát hiện ghi vào cột Note: "BUG-{NNN}: {Short description}"</rule>
  </output_rules>

</output_system_test>

---

## 5. Automation Workflow Test Guide (UC-10, UC-11)

| UC | Trigger Condition | Test Action | Expected |
|----|------------------|-------------|----------|
| UC-10 Renewal Alert | Student có ≤ 14 ngày đến hết gói | Seed student với expiry = today+13 → chạy worker | Email gửi đến Admin + Giáo vụ |
| UC-11 Attendance Email | Teacher submit attendance | Submit attendance qua API | Email gửi đến Parent trong 12h |

---

> **Maintained by:** QA Lead  
> **Output file:** [Report5.3_CLS_System Test.xlsx](../../Testing/03_System_Test/Report5.3_CLS_System%20Test.xlsx)
