# CLS Unit Test Convention

> **Level:** Unit Test (UT)  
> **Role:** Senior QA — Classroom Management System (CLS)  
> **Last Updated:** 2026-04-22  
> **Parent:** [testing_convention.md](../testing_convention.md)  
> **Related:** [integration-test-convention.md](./integration-test-convention.md) | [system-test-convention.md](./system-test-convention.md)

---

## 1. Tech Stack

<techstack_unit_test>
  <level>Unit Test (UT)</level>
  <purpose>Kiểm tra từng method/service trong isolation, không phụ thuộc DB hay HTTP</purpose>
  <frameworks>
    <framework>xUnit — test runner chính thức cho .NET</framework>
    <framework>Moq — mock dependencies (IRepository, IService, IEmailSender)</framework>
    <framework>FluentAssertions — assertions rõ ràng, dễ đọc</framework>
    <framework>AutoFixture — sinh test data tự động cho DTO/Entity</framework>
  </frameworks>
  <project_location>CLS.Tests/UnitTests/</project_location>
  <run_command>dotnet test --filter Category=Unit</run_command>
  <coverage_target>
    <services>70% minimum</services>
    <validators>100%</validators>
    <utilities>100%</utilities>
  </coverage_target>
</techstack_unit_test>

---

## 2. Naming Conventions

<naming_unit_test>

### 2.1 File & Class Naming

| Element | Pattern | Example |
|---------|---------|---------|
| Test Project | `CLS.Tests/UnitTests/` | — |
| Test File | `{ServiceName}Tests.cs` | `StudentServiceTests.cs` |
| Validator Test File | `{ValidatorName}Tests.cs` | `CreateStudentRequestValidatorTests.cs` |
| Test Class | `{ServiceName}Tests` | `public class StudentServiceTests` |

### 2.2 Test Method Naming

```
Pattern: MethodName_Scenario_ExpectedResult
```

| ✅ Correct | ❌ Wrong |
|-----------|---------|
| `GetByIdAsync_StudentNotFound_ThrowsNotFoundException` | `TestGetStudent` |
| `CreateEnrollmentAsync_DuplicateEmail_ThrowsConflictException` | `Test1` |
| `ValidateSchedule_RoomConflict_ReturnsFalse` | `ScheduleTest_Happy` |

### 2.3 Test Case ID

<naming_unit_test_id>
  Pattern : UTC-{ModuleCode}-{SequentialNumber}
  Examples :
    UTC-STU-001  → Unit Test, Student module, case 001
    UTC-SCH-002  → Unit Test, Schedule module, case 002
    UTC-ENR-003  → Unit Test, Enrollment module, case 003
  Module Codes :
    STU = Student       | TCH = Teacher      | ENR = Enrollment
    SCH = Schedule      | ATT = Attendance   | PKG = Package
    PAY = Payment       | FBK = Feedback     | NTF = Notification
    AUTH = Auth         | USR = User
</naming_unit_test_id>

</naming_unit_test>

---

## 3. Testing Rules

<unit_test_rules>
  <rule id="TR-UT01">PHẢI mock tất cả external dependencies (IRepository, IEmailSender, ILogger).</rule>
  <rule id="TR-UT02">KHÔNG gọi DbContext thực, không gọi HTTP thực.</rule>
  <rule id="TR-UT03">Mỗi Service method phải có ít nhất: 1 Normal case + 1 Abnormal case.</rule>
  <rule id="TR-UT04">Validator tests phải cover 100% — mỗi validation rule 1 test case riêng.</rule>
  <rule id="TR-UT05">Dùng FluentAssertions thay vì Assert.Equal khi kiểm tra Exception message.</rule>
  <rule id="TR-UT06">Arrange-Act-Assert (AAA) pattern bắt buộc, phân cách bằng comment // Arrange / // Act / // Assert.</rule>
  <rule id="TR-UT07">Coverage tối thiểu: Services ≥ 70%, Validators = 100%.</rule>
</unit_test_rules>

### Code Pattern (AAA)

```csharp
[Fact]
[Trait("Category", "Unit")]
public async Task GetByIdAsync_StudentNotFound_ThrowsNotFoundException()
{
    // Arrange
    var mockRepo = new Mock<IStudentRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);
    var service = new StudentService(mockRepo.Object, _mapper);

    // Act
    var act = async () => await service.GetByIdAsync(99);

    // Assert
    await act.Should().ThrowAsync<NotFoundException>()
        .WithMessage("*Student 99*");
}
```

---

## 4. Output Format — Report 5.1

<output_unit_test>
  <template_file>Testing/01_Unit_Test/Report5.1_CLS_Unit Test.xls</template_file>

  <sheets>

    <sheet name="Cover">
      Project Name : CLS — Classroom Management System
      Project Code : CLS-2026
      Document Code: CLS-2026_UT_v{version}
      Creator      : {Developer Name}
      Issue Date   : {YYYY-MM-DD}
    </sheet>

    <sheet name="MethodList">
      Mục đích : Liệt kê tất cả Class và Method được test
      Columns  : No | Module Name | Method Name | Sheet Name | Description | Pre-Condition
      Ghi chú  :
        - Module Name = tên class C# (StudentService, ScheduleValidator, ...)
        - Method Name = tên method (GetByIdAsync, ValidateConflict, ...)
        - Sheet Name  = tên sheet chứa test cases (đặt theo Method Name)
    </sheet>

    <sheet name="Statistics">
      Mục đích : Tổng hợp kết quả tất cả methods
      Columns  : No | Function Code | Passed | Failed | Untested | N | A | B | Total Test Cases
      Function Code = {ClassName}.{MethodName}  (vd: StudentService.GetByIdAsync)
      Dùng formula COUNTIF — không nhập tay
    </sheet>

    <sheet name="{MethodName}">
      Header block:
        Row 0: Code Module: {ClassName}          | Method: {MethodName}
        Row 1: Created By : {Developer Name}      | Executed By: {QA Name}
        Row 2: Test Requirement: {Brief description of what is being tested}

      Statistics block:
        Passed | Failed | Untested | N count | A count | B count | Total Test Cases

      Test Matrix (1 column = 1 test case):
        Column header : UTCID  (UTC-{MODULE}-{NNN})
        Rows:
          Condition → Precondition → [Input parameters, 1 row each]
          → Expected Output → Actual Output → Result → Execution Date
        Result values : Passed | Failed | Untested
        Type (N/A/B)  : must be labeled in each test case column
    </sheet>

  </sheets>

  <output_rules>
    <rule>Mỗi Method = 1 Sheet riêng trong file XLS</rule>
    <rule>UTC ID phải unique toàn document, không trùng giữa các sheets</rule>
    <rule>N/A/B type phải fill đủ cho mỗi test case column</rule>
    <rule>Statistics sheet dùng formula COUNTIF — không nhập tay</rule>
    <rule>Mỗi method phải có ≥ 1 Normal (N) + ≥ 1 Abnormal (A)</rule>
  </output_rules>

</output_unit_test>

---

> **Maintained by:** QA Lead  
> **Output file:** [Report5.1_CLS_Unit Test.xls](../../Testing/01_Unit_Test/Report5.1_CLS_Unit%20Test.xls)
