# CLS Integration Test Convention

> **Level:** Integration Test (IT)  
> **Role:** Senior QA — Classroom Management System (CLS)  
> **Last Updated:** 2026-04-22  
> **Parent:** [testing_convention.md](../testing_convention.md)  
> **Related:** [unit-test-convention.md](./unit-test-convention.md) | [system-test-convention.md](./system-test-convention.md)

---

## 1. Tech Stack

<techstack_integration_test>
  <level>Integration Test (IT)</level>
  <purpose>Kiểm tra tích hợp Controller → Service → Repository → DB</purpose>
  <frameworks>
    <framework>xUnit — test runner</framework>
    <framework>WebApplicationFactory — spin up ASP.NET Core test host thực</framework>
    <framework>EF Core InMemory Provider — database giả lập, nhanh, không cần Supabase</framework>
    <framework>Respawn — reset DB state giữa các test (khi dùng real PostgreSQL test DB)</framework>
    <framework>HttpClient — gọi HTTP endpoint qua test host</framework>
    <framework>FluentAssertions — assertions rõ ràng cho response body</framework>
  </frameworks>
  <project_location>CLS.Tests/IntegrationTests/</project_location>
  <run_command>dotnet test --filter Category=Integration</run_command>
  <coverage_target>
    <api_endpoints>Happy path + major error cases cho mỗi controller action</api_endpoints>
    <database_queries>GetNearExpiry, ScheduleConflict, PackageDepletion</database_queries>
  </coverage_target>
</techstack_integration_test>

---

## 2. Naming Conventions

<naming_integration_test>

### 2.1 File & Class Naming

| Element | Pattern | Example |
|---------|---------|---------|
| Test Project | `CLS.Tests/IntegrationTests/` | — |
| Test File | `{ControllerName}IntegrationTests.cs` | `StudentsControllerIntegrationTests.cs` |
| Fixture File | `TestWebApplicationFactory.cs` | — |

### 2.2 Test Method Naming

```
Pattern: Endpoint_Scenario_ExpectedHttpStatusAndResult
```

| ✅ Correct | ❌ Wrong |
|-----------|---------|
| `PostEnrollment_ValidRequest_Returns201Created` | `TestCreateStudent` |
| `GetStudentById_NotFound_Returns404` | `Integration1` |
| `PutSchedule_ConflictingRoom_Returns409Conflict` | `ScheduleTest` |

### 2.3 Test Case ID

<naming_integration_test_id>
  Pattern : ITC-{FeatureCode}-{SequentialNumber}
  Examples :
    ITC-STU-001  → Student feature, case 001
    ITC-AUTH-001 → Auth feature, case 001
    ITC-SCH-002  → Schedule feature, case 002
  Feature Codes:
    STU = Students   | TCH = Teachers  | ENR = Enrollments
    SCH = Schedules  | ATT = Attendance | PKG = Packages
    PAY = Payments   | FBK = Feedbacks  | AUTH = Auth
</naming_integration_test_id>

</naming_integration_test>

---

## 3. Testing Rules

<integration_test_rules>
  <rule id="TR-IT01">Dùng WebApplicationFactory để spin up ASP.NET Core host thực.</rule>
  <rule id="TR-IT02">Mỗi IT test class kế thừa IClassFixture để tái sử dụng TestHost.</rule>
  <rule id="TR-IT03">JWT token phải được tạo động trong TestFixture — không hard-code token string.</rule>
  <rule id="TR-IT04">Kiểm tra cả HTTP Status Code VÀ Response Body (ApiResponse&lt;T&gt;).</rule>
  <rule id="TR-IT05">Mỗi Feature phải có: GET list, GET by id, POST, PUT, DELETE (nếu applicable).</rule>
  <rule id="TR-IT06">Test phải verify DB state sau action — không chỉ kiểm tra HTTP response.</rule>
  <rule id="TR-IT07">Seed data phải reset về trạng thái ban đầu sau mỗi test class.</rule>
</integration_test_rules>

### Code Pattern

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task PostEnrollment_ValidRequest_Returns201Created()
{
    // Arrange
    var request = new CreateEnrollmentRequest { StudentId = 1, ClassId = 2 };

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/enrollments", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var body = await response.Content.ReadFromJsonAsync<ApiResponse<EnrollmentDto>>();
    body!.Success.Should().BeTrue();
}
```

---

## 4. Output Format — Report 5.2

<output_integration_test>
  <template_file>Testing/02_Integration_Test/Report5.2_CLS_Integration Test.xlsx</template_file>

  <sheets>
    <sheet name="Cover">
      Project Name : CLS — Classroom Management System
      Project Code : CLS-2026
      Document Code: CLS-2026_IT_v{version}
      Creator      : {QA Name}
      Issue Date   : {YYYY-MM-DD}
    </sheet>

    <sheet name="Test Cases (Index)">
      Columns: No | Feature Name | Sheet Name | Description | Pre-Condition
      Feature Name = Controller/API group (Student API, Auth API, Schedule API, ...)
    </sheet>

    <sheet name="Test Statistics">
      Columns: No | Feature | Round 1 (P/F/Pending/NA) | Round 2 | Round 3 | Total TCs
      Dùng formula COUNTIF tham chiếu từ các Feature sheets
    </sheet>

    <sheet name="{FeatureName}">
      Header:
        Feature         : {Feature Name}
        Test Requirement: {API endpoints covered}
        Number of TCs   : =COUNTA(A12:A998)
      Round block: Round 1/2/3 → Passed | Failed | Pending | N/A
      Test Case table (Row 10+):
        Columns: Test Case ID | Description | Procedure | Expected Results | Pre-conditions
                 | Round 1 | Test date | Tester
                 | Round 2 | Test date | Tester
                 | Round 3 | Test date | Tester | Note
      Sub-headers nhóm theo endpoint:
        "POST /api/v1/students — Create Student"
        "GET /api/v1/students/{id} — Get By Id"
    </sheet>
  </sheets>

  <output_rules>
    <rule>ITC ID unique toàn document: ITC-{FEATURE}-{NNN}</rule>
    <rule>Description nêu rõ: HTTP Method + Endpoint + Scenario</rule>
    <rule>Pre-conditions nêu rõ: JWT role + seed data cần thiết</rule>
    <rule>Expected Results nêu: HTTP Status Code + Response body key fields</rule>
    <rule>Round 1 bắt buộc complete trước release; Round 2/3 cho regression</rule>
    <rule>Result cell: Passed | Failed | Pending | N/A</rule>
  </output_rules>

</output_integration_test>

---

> **Maintained by:** QA Lead  
> **Output file:** [Report5.2_CLS_Integration Test.xlsx](../../Testing/02_Integration_Test/Report5.2_CLS_Integration%20Test.xlsx)
