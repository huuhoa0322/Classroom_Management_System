# Báo Cáo Review Main Workflows v0.2 – CLS System
**Reviewer:** Senior BA (15 năm kinh nghiệm)  
**Phiên bản được review:** v0.2  
**Ngày review:** 2026-04-23  
**So sánh với:** Kết quả review v0.1 (cùng ngày)  
**Nguồn tham chiếu:** CLS_Business_Goals_v0.1.html, project_context.md

---

## Tiêu Chí Review

| # | Tiêu chí | Mô tả |
|---|---|---|
| C1 | **Business Alignment** | Workflow phục vụ đúng KPI trong Business Goals |
| C2 | **Actor Completeness** | Đủ đúng các actor tham gia |
| C3 | **Happy Path** | Luồng chính rõ ràng, đủ bước, không đứt đoạn |
| C4 | **Exception / Unhappy Path** | Có đủ nhánh ngoại lệ quan trọng |
| C5 | **Automation Trigger** | Xác định rõ tự động / thủ công |
| C6 | **Pre/Post Condition** | Điều kiện khởi động và kết thúc rõ ràng |
| C7 | **Diagram Integrity** | XML hợp lệ, không có edge mồ côi, node orphan |

---

## WF-01 v0.2 — Student Enrollment

| Tiêu chí | v0.1 | v0.2 | Ghi chú |
|---|---|---|---|
| C1 Business Alignment | ✅ | ✅ | Không đổi |
| C2 Actor Completeness | ⚠️ | ✅ | Email System implicit qua System lane |
| C3 Happy Path | ✅ | ✅ | Rõ ràng, đủ 8 bước |
| C4 Exception Path | ⚠️ | ✅ | **Đã thêm**: Email bounce → Log + Alert Admin (cell 209, 210, 106) |
| C5 Automation Trigger | ✅ | ✅ | Validate và email là tự động |
| C6 Pre/Post Condition | ⚠️ | ✅ | **Đã thêm**: Pre-condition (cell 4) và Post-condition (cell 5) |
| C7 Diagram Integrity | ✅ | ⚠️ | **Còn 1 vấn đề nhỏ**: Cell 208 (y=670) và Cell 209 (y=628) bị đặt ngược thứ tự y-coordinate — 209 ở trên 208 trong code nhưng edge `208→209` đúng hướng. Không ảnh hưởng logic, chỉ render có thể không đẹp. |

**Status v0.2:** 🟢 **Approved** — Các fix chính đã hoàn thành. Vấn đề còn lại (y-coordinate) là cosmetic, không ảnh hưởng logic.

---

## WF-02 v0.2 — Class Scheduling & Conflict Check

| Tiêu chí | v0.1 | v0.2 | Ghi chú |
|---|---|---|---|
| C1 Business Alignment | ✅ | ✅ | Không đổi |
| C2 Actor Completeness | ✅ | ✅ | Không đổi |
| C3 Happy Path | ✅ | ✅ | Đủ bước, rõ ràng |
| C4 Exception Path | ⚠️ | ✅ | **Đã thêm**: Admin hủy lớp (cell 107) → System Rollback (cell 209) → Teacher nhận thông báo hủy (cell 302) |
| C5 Automation Trigger | ✅ | ✅ | Conflict check tự động |
| C6 Pre/Post Condition | ⚠️ | ✅ | **Đã thêm**: Pre/Post condition boxes |
| C7 Diagram Integrity | ⚠️ | ⚠️ | **Còn tồn tại**: Cell 207n (Parallel note, y=650) và Cell 208 (Send email, y=690) và Cell 209 (Rollback, y=680) cùng nằm trong System lane với overlapping y-coordinates (208: y=690, 209: y=680). Edge 209 không có incoming trigger trực tiếp từ luồng chính — chỉ được trigger từ Admin lane 107. Logic đúng nhưng placement gây nhầm lẫn khi render. |

**Status v0.2:** 🟢 **Approved** — Logic nghiệp vụ đúng, exception flow hoàn chỉnh. Cosmetic overlap không ảnh hưởng tới BA deliverable.

---

## WF-03 v0.2 — Daily Attendance & Parent Notification

| Tiêu chí | v0.1 | v0.2 | Ghi chú |
|---|---|---|---|
| C1 Business Alignment | ✅ | ✅ | Không đổi |
| C2 Actor Completeness | ⚠️ | ✅ | **Đã thêm**: Lane Academic Admin (id=40) với Dashboard view (cell 401), SLA alert (cell 402), Correction Mode (cell 403) |
| C3 Happy Path | ✅ | ✅ | Không đổi |
| C4 Exception Path | ❌ | ✅ | **Đã thêm đầy đủ**: (1) SLA 30-min Timer (cell 203) → Decision "Chưa submit?" (cell 204) → Reminder Teacher (cell 205) + Alert Admin (cell 206). (2) Teacher nhận nhắc nhở (cell 105). (3) Admin Correction Mode (cell 403). |
| C5 Automation Trigger | ✅ | ✅ | SLA timer và email là tự động |
| C6 Pre/Post Condition | ⚠️ | ✅ | **Đã thêm**: Pre/Post condition boxes |
| C7 Diagram Integrity | ✅ | ⚠️ | **Quan sát**: Cell 211 (decision "Có học viên vắng?") và cell 212n (parallel note) cùng có y=218 trong System lane — chúng bị overlap nhau. Tuy nhiên logic flow không bị ảnh hưởng vì edge đi từ 210→211. Cell 212n là annotation, không tham gia vào edge logic. Ngoài ra: e9 và e10 đều có source=211 và target=302 (duplicate edge) → cần xóa e10 là duplicate của e9. |

**Điểm cần ghi chú thêm:** Edge `e13` (dashed: `101→203`) biểu thị SLA Timer được khởi động song song ngay khi buổi học bắt đầu/kết thúc — đây là thiết kế đúng về mặt nghiệp vụ nhưng đặt trong diagram có thể gây hiểu nhầm rằng SLA timer chờ Teacher không submit. Cần thêm label rõ hơn: **"Async: Bắt đầu đếm ngay"**.

**Status v0.2:** 🟢 **Approved** — Đã fix xong Major Gap từ v0.1. Lưu ý nhỏ về duplicate edge và annotation overlap không ảnh hưởng logic nghiệp vụ.

---

## WF-04 v0.2 — Teacher Feedback SLA 12h

| Tiêu chí | v0.1 | v0.2 | Ghi chú |
|---|---|---|---|
| C1 Business Alignment | ✅ | ✅ | Không đổi |
| C2 Actor Completeness | ✅ | ✅ | Không đổi |
| C3 Happy Path | ✅ | ✅ | Đủ bước: Notify → Teacher submit → Validate → Record → Email Parent |
| C4 Exception Path | ⚠️ | ✅ | **Đã thêm**: (1) Validation content rỗng/ngắn (cell 106v) → Reject + yêu cầu nhập lại (cell 106r); (2) SLA 12h timer parallel track rõ ràng (dashed edge e11: `102→104`) |
| C5 Automation Trigger | ✅ | ✅ | SLA timer, email Parent là tự động |
| C6 Pre/Post Condition | ⚠️ | ✅ | **Đã thêm**: Pre/Post condition boxes + SLA parallel track label (cell 6) |
| C7 Diagram Integrity | ❌ | ✅ | **Đã fix**: Cell 104 (y=640) và Cell 106 (y=460) tách biệt hoàn toàn. Cell 106v (y=400) là validation step nằm giữa Teacher submit và Record — logic đúng. Edge e11 dashed (`102→104`) biểu thị async SLA timer. Edge e15 (`104→302`, "Đã submit") → End — đúng cho trường hợp timer check thấy Teacher đã submit trước 12h. |

**Điểm BA đánh giá cao v0.2:**
- Label "PARALLEL SLA TIMER TRACK" (cell 6) ở header rất rõ ràng — reviewer / developer hiểu ngay ý đồ thiết kế.
- Validation gate (cell 106v) trước khi ghi DB phản ánh đúng yêu cầu Quality Gate của Academic Admin.
- Admin lane (cell 40) có flow logic: Nhận alert (401) → Nhắc Teacher (402) — đúng escalation path.

**Status v0.2:** 🟢 **Approved** — Đây là workflow có sự cải thiện đáng kể nhất so với v0.1. Structural Fix hoàn thành.

---

## WF-05 v0.2 — Package Depletion & Renewal Alert

| Tiêu chí | v0.1 | v0.2 | Ghi chú |
|---|---|---|---|
| C1 Business Alignment | ✅ | ✅ | Không đổi |
| C2 Actor Completeness | ✅ | ✅ | Không đổi |
| C3 Happy Path | ✅ | ✅ | Đủ bước: Job → Scan → Duplicate check → Threshold check → Alert Admin → Contact Parent → Admin tạo gói → System update |
| C4 Exception Path | ⚠️ | ✅ | **Đã thêm**: (1) Duplicate alert check (cell 103dup) → Skip nếu đã có Pending alert (cell 103skip); (2) Parent từ chối → At-Risk (cell 302, 108, 304) |
| C5 Automation Trigger | ✅ | ✅ | Daily Job, tạo alert record là tự động; liên hệ Phụ huynh và tạo gói là Admin thủ công |
| C6 Pre/Post Condition | ⚠️ | ✅ | **Đã thêm**: Pre/Post condition boxes |
| C7 Diagram Integrity | ⚠️ | ⚠️ | **Còn tồn tại**: Cell 107 (System: Cập nhật gói mới, y=700) và Cell 108 (System: At-Risk, y=720) chỉ cách nhau 20px → overlap khi render. Cần tách 2 cell này ra ít nhất 70px. Ngoài ra cell 203 (Admin decision: "Phụ huynh đồng ý?") có y=572 nhưng cell 201 (Admin receive alert, y=615) và 202 (Admin contact, y=695) — thứ tự logic đúng nhưng y-coordinate của 203 đặt trước 201 về mặt layout → đọc diagram sẽ thấy flow ngược. |

**Ghi chú quan trọng:** Logic của luồng Renewal sau khi sửa v0.2 đúng hơn nhiều: **Parent xác nhận (301) → Admin tạo gói (204) → System update (107)**. Đây là đúng nghiệp vụ — Admin là người chủ động tạo gói trong hệ thống sau khi nhận xác nhận từ Phụ huynh, không phải System tự tạo.

**Status v0.2:** 🟢 **Approved** — Logic nghiệp vụ đạt chuẩn. Y-coordinate overlap là cosmetic issue không ảnh hưởng tới trình bày nghiệp vụ.

---

## WF-06 v0.2 — Tuition Payment Tracking

| Tiêu chí | v0.1 | v0.2 | Ghi chú |
|---|---|---|---|
| C1 Business Alignment | ✅ | ✅ | Không đổi |
| C2 Actor Completeness | ⚠️ | ✅ | Parent giờ có 2 luồng riêng: nhận Full Receipt (cell 301) và Partial Receipt (cell 302) |
| C3 Happy Path | ✅ | ✅ | Đủ bước |
| C4 Exception Path | ⚠️ | ✅ | **Đã thêm**: (1) Partial Payment decision (cell 202) → Partial handling (cell 203p) → Parent nhận Partial Receipt (cell 302); (2) Admin correction/void (cell 104c) → Nhập lại |
| C5 Automation Trigger | ✅ | ✅ | Receipt email và daily job là tự động |
| C6 Pre/Post Condition | ⚠️ | ✅ | **Đã thêm**: Pre/Post condition boxes |
| C7 Diagram Integrity | ⚠️ | ⚠️ | **Còn tồn tại**: Sub-flow A và Sub-flow B không có trigger kết nối rõ ràng — Cell 204 (Daily Job, Sub-flow B) không có incoming edge từ bất kỳ node nào trong diagram → **Cell 204 là orphan về edge, chỉ có label divider**. Sub-flow B cần có một Start Event riêng (ellipse) hoặc ghi chú rõ "Trigger: Scheduled independently". Ngoài ra cell 203 (Full email, y=420) và 202 (decision, y=375) bị gần nhau với cell 203p (y=462) — layout có thể chồng lên nhau. |

**Điểm cần lưu ý cho Implementation:**
- Partial Payment (cell 203p) ghi nhận: "Tạo reminder cho số tiền còn lại" — cần xác định trigger của reminder này: **manual (Admin nhắc) hay automated (System job)?** Đây là ambiguity cần clarify trước khi develop.
- Cell 107 (Admin "End: Theo dõi quá hạn") bị đặt ở y=628 trong Admin lane nhưng edge `106→107` ngược chiều với flow từ `105→106` — layout ngược.

**Status v0.2:** 🟢 **Approved with Note** — Logic đạt chuẩn. Cần clarify trigger của Partial Payment reminder trước khi implement. Cell 204 (orphan) cần thêm Start ellipse cho Sub-flow B.

---

## Tổng Hợp So Sánh v0.1 → v0.2

| WF ID | Tên | Status v0.1 | Status v0.2 | Cải thiện |
|---|---|---|---|---|
| WF-01 | Student Enrollment | 🟡 Minor Fix | 🟢 **Approved** | ✅ Email bounce exception + Pre/Post |
| WF-02 | Class Scheduling | 🟡 Minor Fix | 🟢 **Approved** | ✅ Cancel exception + Parallel note |
| WF-03 | Daily Attendance | 🔴 Major Gap | 🟢 **Approved** | ✅ Admin lane + SLA 30-min + Correction Mode |
| WF-04 | Teacher Feedback SLA | 🔴 Structural Fix | 🟢 **Approved** | ✅ Parallel SLA timer + Validation gate + Cell overlap fixed |
| WF-05 | Package Renewal Alert | 🟡 Minor Fix | 🟢 **Approved** | ✅ Duplicate check + Admin creates package |
| WF-06 | Tuition Payment | 🟡 Minor Fix | 🟢 **Approved with Note** | ✅ Partial Payment flow + Correction |

---

## Kết Luận & Khuyến Nghị

### ✅ Kết luận chính

> **Tất cả 6 workflow v0.2 đạt tiêu chuẩn BA deliverable và được phê duyệt để chuyển sang giai đoạn Development.**  
> Các vấn đề nghiêm trọng từ v0.1 (cấu trúc WF-04, thiếu exception WF-03, thiếu duplicate check WF-05) đã được giải quyết hoàn toàn.

### 📋 Danh sách việc cần làm trước Development

| # | Workflow | Action | Mức độ |
|---|---|---|---|
| 1 | WF-06 | Clarify: Partial Payment reminder là automated hay manual? → Cập nhật vào diagram | **Cao** |
| 2 | WF-06 | Thêm Start Event (ellipse) cho Sub-flow B (Daily Job), hiện đang orphan | Trung bình |
| 3 | WF-03 | Xóa duplicate edge e10 (source=211, target=302 bị trùng với e9) | Thấp |
| 4 | WF-05 | Tách cell 107 và 108 trong System lane (hiện overlap y=700 và y=720) | Thấp |
| 5 | WF-01 | Sắp xếp lại y-coordinate cell 208 và 209 cho đúng thứ tự visual | Thấp |

### 🚀 Thứ tự Implement đề xuất

Dựa trên độ phức tạp kỹ thuật và dependency:

1. **WF-01** (Student Enrollment) → Foundation, data entry vào DB  
2. **WF-02** (Class Scheduling) → Depend on WF-01 (Student & Teacher exist)  
3. **WF-06** (Tuition Payment) → Depend on WF-01 (Student enrolled)  
4. **WF-03** (Daily Attendance) → Depend on WF-02 (Class schedule exist)  
5. **WF-05** (Package Renewal) → Depend on WF-01 + WF-06 (Package data)  
6. **WF-04** (Teacher Feedback) → Depend on WF-02 + WF-03 (Session completed)
