# P11 + P_FE7: Code Review — Retention Management (CLS-006 + CLS-010)

> **Reviewer Role:** Senior .NET Tech Lead + Senior Frontend Reviewer

---

## 📋 P11: Backend Code Review Checklist

### ✅ N+1 Query Problem

**Repository layer — PASS ✅**
- [GetAlertsPagedAsync](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.DAL/Repositories/RenewalAlertRepository.cs#L19-L26): Correct use of `.Include().ThenInclude()` chains + `AsNoTracking()` for read-only queries.
- [GetByIdAsync](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.DAL/Repositories/RenewalAlertRepository.cs#L68-L75): Proper Include chain for update path (tracking enabled — correct).

**Service layer — ISSUE FOUND ❌ (Critical)**

> [!CAUTION]
> **ISSUE #1: N+1 Query in `ScanAndCreateAlertsAsync`** (Line 95-132 of [RenewalAlertService.cs](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.BLL/Services/RenewalAlertService.cs#L95-L132))
>
> ```csharp
> var allPackages = await _spRepo.GetAllAsync(ct);  // Query 1: lấy TẤT CẢ packages
> foreach (var sp in activePackages)
> {
>     await _alertRepo.ExistsForPackageAsync(sp.Id, ct);   // Query N: 1 per package
>     await _spRepo.GetByIdWithDetailsAsync(sp.Id, ct);    // Query N: 1 per package
> }
> ```
> **Problem:** Với 100 active packages, sẽ phát sinh **200+ database roundtrips** trong vòng lặp.
>
> **Fix:** Batch load existing alert IDs + load packages with details in a single query.

**Proposed refactor:**
```csharp
// Thay thế toàn bộ method ScanAndCreateAlertsAsync:
public async Task<int> ScanAndCreateAlertsAsync(CancellationToken ct = default)
{
    _logger.LogInformation("Starting depletion scan for near-expiry student packages...");

    // 1. Single query: lấy active packages kèm Student → Parent
    var activePackages = await _spRepo.GetActiveWithDetailsAsync(ct);

    // 2. Single query: lấy tất cả package IDs đã có pending alert
    var existingAlertPackageIds = await _alertRepo.GetExistingPendingPackageIdsAsync(ct);

    var newAlerts = new List<AlertNotification>();

    foreach (var sp in activePackages)
    {
        var remainingDays = (sp.EndDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;
        var isLowSessions = sp.RemainingSessions <= AppConstants.DepletionThresholds.MinSessions;
        var isNearExpiry = remainingDays <= AppConstants.DepletionThresholds.MinDays;

        if (!isLowSessions && !isNearExpiry) continue;
        if (existingAlertPackageIds.Contains(sp.Id)) continue;
        if (sp.Student?.Parent is null) continue;

        var reason = isLowSessions
            ? $"Gói học còn {sp.RemainingSessions} buổi (ngưỡng: ≤{AppConstants.DepletionThresholds.MinSessions})"
            : $"Gói học còn {remainingDays} ngày trước khi hết hạn (ngưỡng: ≤{AppConstants.DepletionThresholds.MinDays} ngày)";

        newAlerts.Add(new AlertNotification { ... });
    }
    // ... batch save
}
```

**Required new repository methods:**
```csharp
// IStudentPackageRepository:
Task<List<StudentPackage>> GetActiveWithDetailsAsync(CancellationToken ct);

// IRenewalAlertRepository:
Task<HashSet<int>> GetExistingPendingPackageIdsAsync(CancellationToken ct);
```

---

### ✅ Data Leakage

**PASS ✅** — No sensitive fields (`PasswordHash`, tokens) exposed in `RenewalAlertResponse`. Only exposes: `StudentName`, `ParentName`, `ParentEmail`, `ParentPhone`, `PackageName`, `RemainingSessions`, `RemainingDays`, `Status`, `Message`, `CreatedAt`, `ConsultedAt`.

---

### ✅ Entity Integrity

**PASS ✅** — `AlertNotification` correctly does NOT extend `BaseEntity` (append-only table per schema design, matching `activity_logs` pattern). No `HasQueryFilter` needed since there's no soft-delete.

---

### ✅ Async Compliance

**PASS ✅** — No `.Result` or `.Wait()` blocking calls found. All I/O operations use `async/await`.

---

### ✅ Logging

**PASS ✅** — All log statements use structured message templates (e.g., `"Alert {AlertId} status updated to {Status}"`). No string interpolation in log calls.

---

### ✅ SOLID Violations

**PASS ✅** — `IRenewalAlertService` has 3 focused methods. Controller is thin (delegates to service). Repository handles data access only.

---

### ⚠️ Minor Issues

> [!WARNING]
> **ISSUE #2: Magic string in Repository** ([RenewalAlertRepository.cs:80](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/CLS.DAL/Repositories/RenewalAlertRepository.cs#L80))
>
> ```csharp
> .AnyAsync(a => a.Status == "pending", ct);  // ❌ Magic string
> ```
> Should use constant. But since DAL cannot reference BLL's `AppConstants`, either:
> - (A) Move status constants to DAL, or
> - (B) Pass status as parameter from Service layer

**Proposed fix (option B — minimal change):**
```csharp
// Interface:
Task<bool> ExistsForPackageAsync(int studentPackageId, string pendingStatus, CancellationToken ct = default);

// Implementation:
.AnyAsync(a => a.StudentPackageId == studentPackageId && a.Status == pendingStatus, ct);

// Caller (Service):
await _alertRepo.ExistsForPackageAsync(sp.Id, AppConstants.AlertNotificationStatus.Pending, ct);
```

---

## 📋 P_FE7: Frontend Code Review Checklist

### 1. Prop Drilling — PASS ✅
Không có props truyền sâu quá 2 cấp. `RenewalAlertTable` là self-contained component, tự fetch data qua hooks.

### 2. Axios trong UI Component — PASS ✅
Không có `apiClient`/`axios` trực tiếp trong components. Data fetching qua `useRenewalAlerts` → `renewalAlertService` → `apiClient`.

### 3. TanStack Query — PASS ✅
Sử dụng `useQuery` và `useMutation` đúng pattern. Không dùng `useEffect + useState` để fetch.

### 4. Form (React Hook Form) — N/A ✅
Feature này không có form input (chỉ có toggle button). Không áp dụng.

### 5. Key Prop — PASS ✅
- List render dùng `alert.id` làm key ✅ ([RenewalAlertTable.jsx:160](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/src/features/retention/components/RenewalAlertTable.jsx#L160))
- Filter tabs dùng `tab.value ?? 'all'` ✅

> [!NOTE]
> **ISSUE #3: Loading skeleton dùng `index` làm key** (Line 72-73)
> ```jsx
> {[...Array(5)].map((_, i) => (
>   <div key={i} className="..." />
> ))}
> ```
> Chấp nhận được vì đây là static placeholder list (không re-order), nhưng ghi nhận.

### 6. Missing States — PASS ✅
- `isLoading` → Skeleton animation ✅
- `error` → Error message display ✅
- Empty state → "Không có cảnh báo nào" + emoji ✅
- Mutation loading → `disabled={updateStatus.isPending}` ✅

---

### ⚠️ FE Minor Issues

> [!NOTE]
> **ISSUE #4: `SortIcon` component defined inside render** ([Line 63-66](file:///d:/DH%202022%20-%202026/Tai%20lieu%20hoc/8.%20Spring%202026/AISDLC/Classroom_Management_System/Developments/CLS/cls.client/src/features/retention/components/RenewalAlertTable.jsx#L63-L66))
>
> `SortIcon` is a component defined inside `RenewalAlertTable` — re-created every render. Should be extracted outside or memoized.

> [!NOTE]
> **ISSUE #5: Missing Zod schema** — Convention requires `src/features/retention/schemas/` directory with validation schema. Not strictly needed for this read-only feature, but noted for consistency.

---

## 📊 Review Summary

| Category | Status | Issues |
|----------|--------|--------|
| N+1 Query | ❌ **CRITICAL** | #1: Loop queries in ScanAndCreateAlertsAsync |
| Data Leakage | ✅ PASS | — |
| Entity Integrity | ✅ PASS | — |
| Async Compliance | ✅ PASS | — |
| Logging | ✅ PASS | — |
| SOLID | ✅ PASS | — |
| Magic Strings | ⚠️ WARNING | #2: "pending" hardcoded in Repository |
| FE Prop Drilling | ✅ PASS | — |
| FE Axios in Component | ✅ PASS | — |
| FE TanStack Query | ✅ PASS | — |
| FE Key Props | ✅ PASS | #3: Minor (skeleton index key) |
| FE Missing States | ✅ PASS | — |
| FE Component Structure | ⚠️ NOTE | #4: SortIcon inside render |
| FE Schema Convention | ⚠️ NOTE | #5: Missing Zod schema folder |

## 🔧 Recommended Actions

| Priority | Issue | Action |
|----------|-------|--------|
| 🔴 **MUST FIX** | #1 N+1 Query | Refactor `ScanAndCreateAlertsAsync` to batch queries |
| 🟡 **SHOULD FIX** | #2 Magic String | Pass status as parameter to `ExistsForPackageAsync` |
| 🟢 **OPTIONAL** | #3-#5 | Minor optimizations, can defer |

> Bạn muốn tôi thực hiện fix cho **Issue #1** và **Issue #2** không?
