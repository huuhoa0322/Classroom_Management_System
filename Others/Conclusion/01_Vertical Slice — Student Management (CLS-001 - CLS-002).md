# 🗂️ Giải phẫu Vertical Slice — Student Management (CLS-001 & CLS-002)

Tài liệu này giải thích chi tiết từng file vừa được tạo, lý do tồn tại và cơ chế hoạt động của chúng trong kiến trúc Layered Monolith của CLS.

---

## 📐 Sơ đồ luồng dữ liệu

```
[React UI] → [studentService.js] → [apiClient.js] → [HTTP]
                                                         ↓
                                              [StudentsController]
                                                         ↓
                                              [StudentService]
                                                    ↙       ↘
                                     [StudentRepository] [ParentRepository]
                                              ↓
                                       [AppDbContext]
                                              ↓
                                         [Supabase DB]
```

---

## 🗄️ BACKEND — P7: Data Layer

### `CLS.DAL/Entities/Parent.cs`
**Nhiệm vụ:** Entity đại diện cho phụ huynh học sinh.

**Điểm quan trọng:** Đây là **Contact Record thuần túy** — không có `PasswordHash`, không có `Role`. Phụ huynh không đăng nhập vào hệ thống mà chỉ nhận thông báo qua Email. Thiết kế này phản ánh đúng nghiệp vụ: chỉ Admin và Teacher mới tương tác trực tiếp với CLS.

---

### `CLS.DAL/Entities/Student.cs`
**Nhiệm vụ:** Entity học sinh với lifecycle `active ↔ inactive`.

**Điểm quan trọng:** Navigation property `Parent parent = null!` dùng `null!` thay vì `null` — đây là .NET nullable reference type convention, báo với compiler rằng "property này sẽ luôn được EF Core populate, đừng cảnh báo null".

---

### `CLS.DAL/Configurations/ParentConfiguration.cs`
**Nhiệm vụ:** Fluent API ánh xạ `Parent` entity → bảng `parents` trên DB.

**Điểm quan trọng:** `HasIndex(x => x.Email).IsUnique()` — Email phụ huynh là duy nhất trong DB. Đây là nền tảng để thực hiện **upsert** (tìm theo email → nếu chưa có thì tạo mới, nếu có rồi thì dùng lại). Không có index unique này thì có thể tạo trùng phụ huynh.

---

### `CLS.DAL/Configurations/StudentConfiguration.cs`
**Nhiệm vụ:** Fluent API ánh xạ `Student` entity.

**Điểm quan trọng:** `OnDelete(DeleteBehavior.Restrict)` — Khi xóa phụ huynh, DB sẽ **từ chối** thay vì cascade xóa tất cả con. Điều này bảo vệ tính toàn vẹn dữ liệu: không thể vô tình xóa mất hồ sơ học sinh.

---

## 🔌 BACKEND — P8: Persistence & Transfer Layer

### `CLS.DAL/Repositories/IRepository.cs`
**Nhiệm vụ:** Generic CRUD interface — nền tảng cho mọi repository.

**Điểm quan trọng:** Dùng Generic `<T>` để tái sử dụng. Tất cả repository đặc thù (`IStudentRepository`, `IParentRepository`...) đều **extend interface này** thay vì viết lại từ đầu. Khi muốn thêm cache layer, chỉ cần wrap implementation chứ không đụng vào interface.

---

### `CLS.DAL/Repositories/IStudentRepository.cs` + `StudentRepository.cs`
**Nhiệm vụ:** Truy vấn database cho entity `Student`.

**Điểm quan trọng:**
- `GetPagedAsync` trả về `(List<Student>, int TotalCount)` — **tuple** thay vì `PagedResult<T>` để tránh circular dependency (DAL không được reference BLL).
- Mọi read query đều dùng `AsNoTracking()` — EF Core sẽ không theo dõi object, tăng hiệu năng đáng kể cho các query chỉ đọc.
- `Include(s => s.Parent)` — Eager loading phụ huynh trong cùng 1 query SQL, tránh N+1 problem.

---

### `CLS.DAL/Repositories/IParentRepository.cs` + `ParentRepository.cs`
**Nhiệm vụ:** Truy vấn database cho entity `Parent`.

**Điểm quan trọng:** `GetByEmailAsync` dùng `.ToLower()` ở cả 2 vế để **case-insensitive lookup** — đảm bảo `PHUhuynh@gmail.com` và `phuhuynh@gmail.com` được coi là cùng 1 phụ huynh.

---

### `CLS.BLL/DTOs/Students/`

| File | Mục đích |
|------|---------|
| `CreateStudentRequest.cs` | Nhận dữ liệu từ FE: cả thông tin học sinh + phụ huynh trong 1 object |
| `UpdateStudentRequest.cs` | Chỉ cho phép sửa `FullName`, `DateOfBirth` — status dùng endpoint riêng |
| `UpdateStudentStatusRequest.cs` | Endpoint `PATCH /status` nhận object `{ "status": "inactive" }` |
| `StudentResponse.cs` | Trả về FE: flatten thông tin phụ huynh vào cùng object, không nest |

**Tại sao flatten?** FE chỉ cần render `student.parentEmail` thay vì `student.parent.email` — đơn giản hơn, tránh null check.

---

### `CLS.BLL/Mappings/StudentMappingProfile.cs`
**Nhiệm vụ:** AutoMapper — tự động chuyển đổi giữa Entity và DTO.

**Điểm quan trọng:** Map `CreateStudentRequest → Parent` chỉ map các field `ParentXxx` → `Xxx`. Map `CreateStudentRequest → Student` phải `Ignore()` trường `ParentId` và `Parent` — vì chúng sẽ được gán thủ công sau khi upsert Parent thành công.

---

## 🧠 BACKEND — P9: Business Logic

### `CLS.BLL/Services/StudentService.cs`
**Nhiệm vụ:** Xử lý toàn bộ nghiệp vụ học sinh.

**Luồng CreateAsync (CLS-001):**
```
1. Tìm Parent theo Email trong DB
2a. Nếu chưa có → tạo mới Parent, SaveChanges để lấy Id
2b. Nếu có rồi → dùng lại, không tạo trùng
3. Tạo Student, gán ParentId = parent.Id
4. SaveChanges
5. Reload với Include(Parent) để map response
```

**Tại sao SaveChanges 2 lần?** Khi Parent là mới (Id = 0), phải save trước để DB cấp `Id` thì mới gán `student.ParentId = parent.Id` được. Đây là trường hợp bắt buộc, không thể tránh.

**Luồng UpdateStatusAsync (CLS-002 AC1):**
```
1. Validate status (chỉ chấp nhận 'active', 'inactive')
2. Load Student từ DB
3. Đổi Status
4. Nếu inactive → Log cảnh báo (stub cho archive packages sau này)
5. SaveChanges
```

---

### `CLS.BLL/Validators/CreateStudentRequestValidator.cs`
**Nhiệm vụ:** FluentValidation kiểm tra input trước khi vào Service.

**Điểm quan trọng:** Error message của `ParentEmail` được viết **nguyên văn từ AC2**:
> *"Parent Email is strictly required to enable zero-touch automated notifications."*

Đây là yêu cầu bắt buộc từ User Story CLS-001 — message phải khớp chính xác.

---

## 🌐 BACKEND — P10: API Surface

### `CLS.Server/Controllers/StudentsController.cs`
**Nhiệm vụ:** Publish 5 HTTP endpoints.

| Endpoint | Use Case |
|----------|---------|
| `GET /api/v1/students` | Danh sách phân trang, filter status |
| `GET /api/v1/students/{id}` | Chi tiết 1 học sinh |
| `POST /api/v1/students` | CLS-001: Tạo mới |
| `PUT /api/v1/students/{id}` | CLS-002: Sửa thông tin |
| `PATCH /api/v1/students/{id}/status` | CLS-002 AC1: Đổi lifecycle |

**Tại sao PATCH riêng cho status?** Nếu để status trong PUT, Frontend có thể vô tình gửi `status: null` và xóa mất trạng thái. PATCH riêng buộc Admin phải có ý thức **chủ động** đổi lifecycle — tránh thao tác nhầm.

**Controller chỉ làm 3 việc:**
1. Nhận request
2. Gọi Service
3. Wrap response vào `ApiResponse<T>` và trả về

Không có logic nghiệp vụ hay gọi DB trực tiếp.

---

## ⚙️ Environment Configuration

### `appsettings.json`
Mọi sensitive value (`ConnectionString`, `SecretKey`) để **trống** → an toàn commit lên Git.

### `appsettings.Development.json`
Chỉ chứa log level cho môi trường dev — không có secret.

### `.env.example`
Template tài liệu hóa các biến môi trường cần thiết cho developer mới clone repo.

**Cơ chế ưu tiên ASP.NET Core:**
```
appsettings.json (thấp nhất)
  → appsettings.Development.json
    → User Secrets (local dev)  ← secret thật ở đây
      → Environment Variables (staging/prod)  ← cao nhất
```

---

## ⚛️ FRONTEND — P_FE5: Data Access Layer

### `studentService.js`
**Nhiệm vụ:** Mapping 1-1 với các HTTP endpoint của Backend.

**Điểm quan trọng:** File này không biết về TanStack Query hay React — chỉ là các hàm gọi `apiClient`. Điều này giúp tái sử dụng: nếu sau này muốn dùng `fetch` thay vì Axios, chỉ cần sửa ở đây.

---

### `useStudents.js`
**Nhiệm vụ:** TanStack Query hooks — bridge giữa Service và Component.

**Query Key Factory `studentKeys`:**
```js
studentKeys.all      → ['students']
studentKeys.list(p)  → ['students', 'list', { page: 1, ... }]
studentKeys.detail(1)→ ['students', 'detail', 1]
```
Khi `createStudent` thành công, `invalidateQueries({ queryKey: studentKeys.all })` sẽ invalidate **tất cả query bắt đầu bằng `['students']`** → danh sách tự refresh mà không cần reload trang.

---

### `student.schema.js`
**Nhiệm vụ:** Zod validation schema — mirror chính xác FluentValidation của Backend.

**Điểm quan trọng:** Error message `parentEmail` được viết **nguyên văn từ AC2** — đồng bộ với Backend validator. Người dùng nhìn thấy cùng 1 message dù validate ở FE hay BE.

---

## 🎨 FRONTEND — P_FE6: UI Presentation Layer

### `StatusBadge.jsx`
**Nhiệm vụ:** Hiển thị trạng thái bằng pill có màu.

**Cách hoạt động:** Nhận prop `status` (string) → lookup trong object `styles` → trả ra Tailwind classes tương ứng. Nếu `status` không khớp → fallback về màu xám trung tính. Hoàn toàn **presentational** — không có state, không gọi API.

---

### `StudentForm.jsx`
**Nhiệm vụ:** Form dùng chung cho cả tạo mới và sửa.

**Cách phân biệt mode:** Prop `student` — nếu có thì Edit mode, nếu null thì Create mode.
- Create: hiển thị cả section Phụ huynh + dùng `createStudentSchema`
- Edit: chỉ hiển thị thông tin học sinh + dùng `updateStudentSchema`

**React Hook Form + Zod:** Form không tự quản lý state — `useForm` với `zodResolver` xử lý tất cả: validation, error state, submit handling.

---

### `StudentList.jsx`
**Nhiệm vụ:** Bảng danh sách học sinh với filter, phân trang và inline status toggle.

**Skeleton loading:** Khi `isLoading = true`, render 5 ô `div` với `animate-pulse` thay vì spinner — UX tốt hơn vì người dùng thấy "bóng" của nội dung sắp hiện ra.

**Inline toggle status:** Nút "Cho nghỉ" / "Kích hoạt" gọi thẳng `useUpdateStudentStatus` ngay tại hàng — không cần mở dialog, nhanh gọn cho Admin.

---

### `StudentPage.jsx`
**Nhiệm vụ:** Trang đầy đủ — orchestrator kết hợp List + Modal Form.

**Cách hoạt động:**
- State `selectedStudent` = null → Modal hiện Create form
- State `selectedStudent` = object → Modal hiện Edit form
- `onSuccess` callback của mutation → gọi `handleClose()` đóng modal + TanStack Query tự invalidate list

---

## 🔗 Kết nối các tầng

```
StudentPage
  ├── useCreateStudent()    → studentService.create()  → POST /api/v1/students
  ├── useUpdateStudent()    → studentService.update()  → PUT  /api/v1/students/:id
  └── StudentList
        └── useUpdateStudentStatus() → studentService.updateStatus() → PATCH /api/v1/students/:id/status
```

Mọi mutation thành công đều trigger `queryClient.invalidateQueries` → `useStudentList` tự fetch lại → UI cập nhật **tự động, không cần reload**.
