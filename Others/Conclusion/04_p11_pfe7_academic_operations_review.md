# P11 + P_FE7: Code Review — Academic Operations (UC-07 + UC-08)

> **Feature:** View Personalized Timetables + Record Class Attendance  
> **Role:** Teacher  
> **Reviewer:** Senior .NET Tech Lead + Senior Frontend Reviewer

---

## P11: Backend Checklist

| # | Check | Status | Notes |
|---|-------|--------|-------|
| 1 | **N+1 Query** | ✅ PASS | `GetTeacherScheduleAsync` uses `Include(Class)` + `Include(Room)` in single query. `GetByIdWithClassStudentsAsync` uses chained `ThenInclude` for `Class→ClassStudents→Student`. All read queries use `AsNoTracking()`. |
| 2 | **Data Leakage** | ✅ PASS | DTOs (`AttendanceDto`, `AttendanceSheetDto`) only expose business fields. No `PasswordHash`, token, or PII leaks. |
| 3 | **Entity Integrity** | ✅ PASS | `Attendance` và `ClassStudent` extend `BaseEntity`. Fluent API configs have `HasQueryFilter` soft-delete (inherited from `BaseEntityConfiguration`). UNIQUE indexes match schema constraints. |
| 4 | **Async Compliance** | ✅ PASS | No `.Result` or `.Wait()` blocking. All I/O is `async/await`. `CancellationToken` propagated in all methods. |
| 5 | **Logging** | ✅ PASS | Structured message templates: `"Timetable loaded for Teacher {TeacherId}: {Count} sessions"`. No string interpolation in log calls. |
| 6 | **SOLID** | ⚠️ MINOR | `AttendanceService` has 3 focused methods — acceptable. Tuy nhiên, `SubmitAttendanceAsync` dài ~70 dòng, nên extract timezone logic ra helper. Xem **Issue #1**. |
| 7 | **Magic Strings** | ⚠️ MINOR | Hardcoded `"active"` và `"cancelled"` trong 2 vị trí. Xem **Issue #2**. |
| 8 | **Include chains** | ✅ PASS | Read queries use `AsNoTracking()`. `GetByIdWithDetailsAsync` (tracking mode) used correctly for write operations. |
| 9 | **Controller Pattern** | ✅ PASS | Controller là thin wrapper, chỉ delegate sang `IAttendanceService`. `GetTeacherId()` từ JWT claims. `ToOkResponse` pattern tuân thủ chuẩn. |
| 10 | **Validation** | ✅ PASS | `SubmitAttendanceRequestValidator` validates records not empty + valid status values. Injected via DI. |

---

## P_FE7: Frontend Checklist

| # | Check | Status | Notes |
|---|-------|--------|-------|
| 1 | **Prop Drilling** (max 2 levels) | ✅ PASS | `AttendancePage → AttendanceSheet` (1 level). `TimetablePage → TimetableWeekView → SessionCard` (2 levels). Không vượt quá. |
| 2 | **No Axios in UI Component** | ✅ PASS | Tất cả API calls qua `attendanceService.js`. Không có `axios`/`apiClient` trực tiếp trong components. |
| 3 | **TanStack Query** (no useEffect+useState for data) | ✅ PASS | `useTimetable`, `useAttendanceSheet`, `useSubmitAttendance` — tất cả dùng `useQuery`/`useMutation`. |
| 4 | **React Hook Form** | ✅ N/A | Attendance sheet dùng controlled radio inputs — phù hợp vì không phải dạng form phức tạp. |
| 5 | **Key prop** (entity ID, not index) | ⚠️ ISSUE | `TimetableWeekView` dùng `key={idx}` cho day columns. Xem **Issue #3**. |
| 6 | **Loading + Error states** | ✅ PASS | Cả 2 pages đều xử lý `isLoading` (spinner), `isError` (error box), `!data` (null guard). |
| 7 | **Unused import** | ⚠️ ISSUE | `SESSION_STATUS` imported nhưng không sử dụng trong `AttendancePage.jsx`. Xem **Issue #4**. |

---

## Issues Found & Fixes

### Issue #1: Timezone logic nên extract thành helper (SOLID — SRP) ⚠️ LOW

**File:** [AttendanceService.cs](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/AttendanceService.cs#L117-L120)

Timezone conversion `TimeSpan.FromHours(7)` hardcoded trong service. Nếu mở rộng sang timezone khác hoặc cần reuse, nên extract ra `DateTimeHelper`.

**Đề xuất:** Tạm chấp nhận cho single-tenant VN system. Đánh dấu `// TODO: extract to DateTimeHelper if multi-tenant`.

**Severity:** LOW — không block push.

---

### Issue #2: Magic strings `"active"` và `"cancelled"` ⚠️ MEDIUM

**Files:**
- [SessionRepository.cs](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.DAL/Repositories/SessionRepository.cs) — `s.Status != "cancelled"` in `GetTeacherScheduleAsync`
- [SessionRepository.cs](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.DAL/Repositories/SessionRepository.cs) — `cs.Status == "active"` in `GetByIdWithClassStudentsAsync`

DAL layer không access được `AppConstants` (nằm ở BLL). Tuy nhiên, những strings này trùng khớp với DB constraint values và đã được dùng ở các repo khác.

**Đề xuất:** Tạo `CLS.DAL.Common.DbConstants` chứa status strings cho DAL layer, hoặc chấp nhận hiện trạng vì DAL layer vốn match DB schema.

**Severity:** MEDIUM — nên fix nhưng không block push.

---

### Issue #3: `key={idx}` trong TimetableWeekView ⚠️ LOW

**File:** [TimetableWeekView.jsx](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/src/features/attendance/components/TimetableWeekView.jsx#L33)

```diff
-key={idx}
+key={day.toISOString()}
```

Tuy nhiên, 7 ngày trong tuần luôn render theo thứ tự cố định nên `idx` không gây re-render sai. **LOW severity**.

**Severity:** LOW — cosmetic fix.

---

### Issue #4: `hasExisting` reference trước khi khai báo + unused import ⚠️ HIGH

**File:** [AttendancePage.jsx](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/src/features/attendance/pages/AttendancePage.jsx#L27)

`hasExisting` được sử dụng trong `handleSubmit` callback (line 27) nhưng chỉ được khai báo ở line 63. JavaScript hoisting cho `const` sẽ gây `ReferenceError` nếu submit được gọi trước khi component render xong. Tuy hành vi runtime bình thường (callback chỉ gọi sau render), đây là **code smell** và có thể gây confusion.

Ngoài ra, `SESSION_STATUS` import ở line 6 nhưng không sử dụng (đã xóa `isCompleted` logic).

```diff
// Line 6: Xóa unused import
-import { SESSION_STATUS } from '@/shared/utils/constants';

// Line 27: Di chuyển hasExisting check vào callback scope
 onSuccess: () => {
+  const hasExisting = sheet?.existingRecords?.length > 0;
   addToast({
     type: 'success',
     message: hasExisting
```

**Severity:** HIGH — nên fix trước push (runtime safe nhưng code quality issue).

---

### Issue #5: `statusLabels` object khai báo nhưng không sử dụng ⚠️ LOW

**File:** [AttendanceSheet.jsx](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/src/features/attendance/pages/AttendancePage.jsx#L9-L13)

`statusLabels` object ở line 9-13 được khai báo nhưng không sử dụng ở đâu trong component.

**Severity:** LOW — dead code, nên xóa.

---

## File Integrity Summary

### Backend (15 files)

| Layer | File | Status |
|-------|------|--------|
| Entity | `Attendance.cs` | ✅ |
| Entity | `ClassStudent.cs` | ✅ |
| Config | `AttendanceConfiguration.cs` | ✅ |
| Config | `ClassStudentConfiguration.cs` | ✅ |
| Entity (mod) | `Class.cs` | ✅ |
| DbContext (mod) | `AppDbContext.cs` | ✅ |
| Repo Interface | `IAttendanceRepository.cs` | ✅ |
| Repo Impl | `AttendanceRepository.cs` | ✅ |
| Repo Interface (mod) | `ISessionRepository.cs` | ✅ |
| Repo Impl (mod) | `SessionRepository.cs` | ⚠️ #2 |
| DTO | `AttendanceDto.cs` / `AttendanceSheetDto.cs` / `SubmitAttendanceRequest.cs` | ✅ |
| Service Interface | `IAttendanceService.cs` | ✅ |
| Service Impl | `AttendanceService.cs` | ⚠️ #1 |
| Validator | `SubmitAttendanceRequestValidator.cs` | ✅ |
| Mapper | `AttendanceMappingProfile.cs` | ✅ |
| Constants (mod) | `AppConstants.cs` | ✅ |
| Controller | `TeacherController.cs` | ✅ |
| DI (mod) | `Program.cs` | ✅ |

### Frontend (10 files)

| Layer | File | Status |
|-------|------|--------|
| Service | `attendanceService.js` | ✅ |
| Hook | `useAttendance.js` | ✅ |
| Page | `TimetablePage.jsx` | ✅ |
| Page | `AttendancePage.jsx` | ⚠️ #4 |
| Component | `TimetableWeekView.jsx` | ⚠️ #3 |
| Component | `WeekNavigator.jsx` | ✅ |
| Component | `AttendanceSheet.jsx` | ⚠️ #5 |
| Component | `SessionCard.jsx` | ✅ |
| Router (mod) | `AppRouter.jsx` | ✅ |
| Constants (mod) | `constants.js` | ✅ |
| Layout (mod) | `MainLayout.jsx` | ✅ |

---

## Verdict

> **⚠️ 1 issue cần fix trước push (Issue #4), 4 issues LOW/MEDIUM có thể fix ngay hoặc defer.**

| Priority | Issue | Action |
|----------|-------|--------|
| 🔴 HIGH | #4 — `hasExisting` scope + unused import | **Fix ngay** |
| 🟡 MEDIUM | #2 — Magic strings trong DAL | Defer — đánh dấu TODO |
| 🟢 LOW | #1 — Timezone helper | Defer |
| 🟢 LOW | #3 — `key={idx}` | Fix ngay (1 dòng) |
| 🟢 LOW | #5 — Dead `statusLabels` | Fix ngay (xóa) |
