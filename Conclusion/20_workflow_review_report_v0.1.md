# Báo Cáo Review Main Workflows – CLS System
**Reviewer:** Senior BA (15 năm kinh nghiệm)  
**Ngày review:** 2026-04-23  
**Nguồn tham chiếu:** CLS_Business_Goals_v0.1.html, project_context.md  
**Phạm vi:** 6 workflow diagrams (.drawio) tại `Documents/02_Requirements/Workflows/`

---

## Tiêu Chí Review

| # | Tiêu chí | Mô tả |
|---|---|---|
| C1 | **Business Alignment** | Workflow phục vụ đúng KPI trong Business Goals |
| C2 | **Actor Completeness** | Đủ đúng các actor tham gia (không thừa, không thiếu) |
| C3 | **Happy Path** | Luồng chính rõ ràng, đủ bước, không đứt đoạn |
| C4 | **Exception / Unhappy Path** | Có ít nhất 2 nhánh ngoại lệ quan trọng |
| C5 | **Automation Trigger** | Xác định rõ bước nào là tự động (System) / thủ công (Human) |
| C6 | **Pre/Post Condition** | Điều kiện khởi động và kết thúc rõ ràng |
| C7 | **Diagram Integrity** | Cấu trúc XML hợp lệ, không có edge mồ côi, node orphan |

---

## Bảng Kết Quả Review

### WF-01 — Student Enrollment

| Mục | Nội dung |
|---|---|
| **Workflow ID** | WF-01 |
| **Name** | Student Enrollment – Luồng Nhập Học Học Viên |
| **File** | `CLS_WF01_Student_Enrollment.drawio` |
| **KPI liên kết** | Admin Efficiency (Reclaim 15 man-hours/week) |
| **Description** | Mô tả luồng từ lúc Admin tiếp nhận yêu cầu nhập học → nhập thông tin học viên → System validate → Admin chọn lớp + gói học phí → System kiểm tra xung đột lịch → tạo hồ sơ trong DB → gửi email chào mừng phụ huynh. |

**Kết quả đánh giá theo tiêu chí:**

| Tiêu chí | Kết quả | Ghi chú |
|---|---|---|
| C1 Business Alignment | ✅ Đạt | Phục vụ Admin Efficiency, cũng chạm Parent Trust qua welcome email |
| C2 Actor Completeness | ⚠️ Cần bổ sung | Thiếu **Center Director** nếu cần phê duyệt học viên đặc biệt; thiếu **Email System** là secondary actor |
| C3 Happy Path | ✅ Đạt | Đủ 8 bước: tiếp nhận → nhập → validate → chọn gói → conflict check → confirm → tạo DB → email |
| C4 Exception Path | ⚠️ Thiếu 1 case | Có: (1) Dữ liệu không hợp lệ, (2) Xung đột lịch. **Thiếu:** trường hợp email chào mừng gửi thất bại (bounce) → cần retry/alert Admin |
| C5 Automation Trigger | ✅ Đạt | Validate và gửi email rõ là System tự động |
| C6 Pre/Post Condition | ⚠️ Không tường minh | Pre-condition (Admin đã login, phụ huynh đã liên hệ) và Post-condition (hồ sơ học viên Active, email đã gửi) không được ghi chú trong diagram |
| C7 Diagram Integrity | ✅ Đạt | XML hợp lệ; edges đầy đủ nguồn/đích. Lưu ý: cell 207 và 208 có y-coordinate chồng nhau (625 vs 645) → overlap trong render |

**Status:** 🟡 **Cần bổ sung nhỏ (Minor Fix)**

**Solution:**
1. Thêm bước exception: "Email gửi thất bại → System log lỗi + thông báo Admin retry"
2. Tách cell 207 và 208 cách nhau tối thiểu 60px (207: y=625, 208: y=700)
3. Thêm ghi chú Pre/Post-condition dạng text box ngoài swimlane

---

### WF-02 — Class Scheduling & Conflict Check

| Mục | Nội dung |
|---|---|
| **Workflow ID** | WF-02 |
| **Name** | Class Scheduling & Conflict Check – Xếp Lịch & Kiểm Tra Xung Đột |
| **File** | `CLS_WF02_Class_Scheduling.drawio` |
| **KPI liên kết** | 0 Scheduling Conflicts Target |
| **Description** | Luồng Admin tạo lớp mới → gán Giáo viên/Phòng/Khung giờ → System kiểm tra lần lượt: (1) giáo viên có bị trùng không → (2) phòng có bị trùng không → lưu lịch vào DB → gửi email thông báo Giáo viên. |

**Kết quả đánh giá theo tiêu chí:**

| Tiêu chí | Kết quả | Ghi chú |
|---|---|---|
| C1 Business Alignment | ✅ Đạt | KPI 0 Scheduling Conflicts là mục tiêu cốt lõi của G4 |
| C2 Actor Completeness | ✅ Đạt | Admin, System, Teacher – đủ 3 actor cho MVP |
| C3 Happy Path | ✅ Đạt | Rõ ràng, đủ bước từ tạo lớp đến Teacher nhận thông báo |
| C4 Exception Path | ⚠️ Thiếu 1 case quan trọng | Có: (1) Giáo viên trùng lịch, (2) Phòng trùng lịch. **Thiếu nghiêm trọng:** trường hợp **Admin hủy lớp học đã tạo** → cần luồng rollback và thông báo Giáo viên |
| C5 Automation Trigger | ✅ Đạt | Conflict check là System tự động, gán là Manual (Admin) |
| C6 Pre/Post Condition | ⚠️ Không tường minh | Pre: Giáo viên và Phòng đã tồn tại trong DB. Post: Lịch học Active, Teacher đã nhận email |
| C7 Diagram Integrity | ⚠️ Logic lỗi nhỏ | Cell 207 (Save DB) có 2 outgoing edge tới cả 106 (Admin confirm) lẫn 208 (System notify) – đây là **parallel fork** nhưng không được ký hiệu rõ bằng parallel gateway |

**Status:** 🟡 **Cần bổ sung nhỏ (Minor Fix)**

**Solution:**
1. Thêm exception: "Admin hủy lớp" → "System rollback lịch + Notify Teacher: Lịch đã bị hủy"
2. Dùng ký hiệu annotation hoặc tách edge rõ ràng để biểu thị parallel output từ cell 207 (gửi email Teacher VÀ Admin xem xác nhận xảy ra đồng thời)
3. Bổ sung điều kiện: "Teacher phải tồn tại và Active trong hệ thống" làm Pre-condition

---

### WF-03 — Daily Attendance & Parent Notification

| Mục | Nội dung |
|---|---|
| **Workflow ID** | WF-03 |
| **Name** | Daily Attendance & Parent Notification – Điểm Danh Hàng Ngày & Thông Báo Phụ Huynh |
| **File** | `CLS_WF03_Daily_Attendance.drawio` |
| **KPI liên kết** | 100% Automated Communication (G1) |
| **Description** | Teacher mở màn hình điểm danh → đánh dấu từng học viên (Có mặt/Vắng/Trễ) → Submit → System ghi nhận + cập nhật số buổi còn lại → kiểm tra danh sách vắng → gửi email tự động tới phụ huynh. |

**Kết quả đánh giá theo tiêu chí:**

| Tiêu chí | Kết quả | Ghi chú |
|---|---|---|
| C1 Business Alignment | ✅ Đạt | Phục vụ trực tiếp G1 (Parent Engagement) và gián tiếp G2 (package depletion tracking) |
| C2 Actor Completeness | ⚠️ Thiếu actor phụ | **Thiếu Academic Admin** trong luồng: Admin cần có khả năng xem dashboard điểm danh tổng hợp. Parent không thể chủ động yêu cầu sửa điểm danh nếu sai → phải qua Admin |
| C3 Happy Path | ✅ Đạt | Rõ ràng, đủ bước |
| C4 Exception Path | ❌ Thiếu nhiều case | Có: (1) Có học viên vắng (email riêng). **Thiếu:** (2) Teacher quên điểm danh sau buổi học (không submit) → System cần cảnh báo; (3) Teacher điểm danh sai → cơ chế chỉnh sửa; (4) Lớp bị hủy buổi → điểm danh N/A |
| C5 Automation Trigger | ✅ Đạt | Email tự động qua System rõ ràng |
| C6 Pre/Post Condition | ⚠️ Không tường minh | Pre: Buổi học tồn tại và ở trạng thái "In Progress". Post: Tất cả học viên đã có bản ghi điểm danh |
| C7 Diagram Integrity | ✅ Đạt | XML hợp lệ, edge đủ và đúng hướng. Lưu ý: email "Có vắng" (205) dẫn vào "email tổng kết" (206) → cần review: nên là **song song** (parallel), không phải tuần tự |

**Status:** 🔴 **Cần sửa đáng kể (Major Gap)**

**Solution:**
1. **Bổ sung exception quan trọng nhất:** "Teacher không submit điểm danh trong 30 phút sau buổi học → System alert Teacher + alert Admin" — đây là rủi ro vận hành thực tế cao nhất
2. Bổ sung bước: "Admin có thể chỉnh sửa điểm danh (Correction Mode)" với audit trail
3. Sửa logic luồng: Email vắng (205) và Email tổng kết (206) nên là **parallel fork** thay vì sequential (206 nên gửi ngay sau khi submit, không cần chờ luồng vắng xong)
4. Thêm lane Academic Admin để xem dashboard điểm danh

---

### WF-04 — Teacher Feedback SLA 12h

| Mục | Nội dung |
|---|---|
| **Workflow ID** | WF-04 |
| **Name** | Teacher Feedback SLA 12h – Giáo Viên Gửi Nhận Xét Học Viên |
| **File** | `CLS_WF04_Teacher_Feedback_SLA12h.drawio` |
| **KPI liên kết** | Academic QA (SLA 12h Feedback) |
| **Description** | Buổi học kết thúc → System tạo Pending Feedback Record + khởi động bộ đếm 12h → notify Teacher → Teacher mở form mobile → nhập nhận xét → submit → System ghi DB + gửi email feedback tới Phụ huynh. Exception: quá 12h → cảnh báo Admin. |

**Kết quả đánh giá theo tiêu chí:**

| Tiêu chí | Kết quả | Ghi chú |
|---|---|---|
| C1 Business Alignment | ✅ Đạt | Phục vụ chính xác G4 (Academic QA, SLA 12h) và G1 (Parent Trust qua feedback email) |
| C2 Actor Completeness | ✅ Đạt | System, Teacher, Parent, Academic Admin – đầy đủ 4 actor |
| C3 Happy Path | ✅ Đạt | Rõ ràng: notify → Teacher submit → System record → Parent nhận email |
| C4 Exception Path | ⚠️ Thiếu 1 case | Có: (1) SLA vi phạm → Admin cảnh báo. **Thiếu:** (2) Teacher submit nhưng nội dung nhận xét rỗng/quá ngắn → System validate và reject trước khi gửi Phụ huynh |
| C5 Automation Trigger | ✅ Đạt | SLA timer, gửi email Parent đều là System tự động |
| C6 Pre/Post Condition | ⚠️ Không tường minh | Pre: Buổi học đã kết thúc và có danh sách học viên. Post: Feedback linked tới buổi học, Parent đã nhận email trong vòng 12h |
| C7 Diagram Integrity | ❌ Lỗi cấu trúc nghiêm trọng | **Cell 104 (Decision: "Giáo viên đã submit chưa?") và Cell 106 (Record feedback) bị đặt trùng vùng y-coordinate (450 vs 430) trong cùng một lane.** Edge từ Teacher submit (204) đi thẳng tới 106 (record) nhưng Decision 104 không được kết nối vào happy path → **Decision node bị orphan với happy path**, chỉ dùng cho exception path nhưng chưa rõ trigger |

**Status:** 🔴 **Cần sửa cấu trúc (Structural Fix Required)**

**Solution:**
1. **Tái cấu trúc luồng SLA check:** Bộ đếm SLA 12h cần được thiết kế như một **parallel track** (concurrent timer chạy song song với Teacher action), không phải sequential. Đề xuất tách thành 2 nhánh: (a) Happy path: Teacher submit trong 12h; (b) Exception path: Timer hết hạn
2. Sửa y-coordinate: Cell 104 (y=450) và 106 (y=430) phải cách nhau ít nhất 70px
3. Thêm validation step: "System kiểm tra nội dung feedback (không rỗng, tối thiểu X ký tự)" trước khi gửi email Parent
4. Ghi rõ trigger cho Decision 104: "Sau 12h, System job kiểm tra Pending Feedback records"

---

### WF-05 — Package Depletion & Renewal Alert

| Mục | Nội dung |
|---|---|
| **Workflow ID** | WF-05 |
| **Name** | Package Depletion & Renewal Alert – Cảnh Báo Hết Gói & Tái Tục |
| **File** | `CLS_WF05_Package_Renewal_Alert.drawio` |
| **KPI liên kết** | Giảm 20% Churn Rate (G2) |
| **Description** | Scheduled daily job (00:00) → quét toàn bộ học viên active → tính số buổi còn lại → nếu ≤ ngưỡng 2 tuần → tạo Renewal Alert Record + gửi cảnh báo tới Admin → Admin liên hệ Phụ huynh → nếu đồng ý: Admin tạo gói mới → System cập nhật; nếu từ chối: mark At-Risk/Churn. |

**Kết quả đánh giá theo tiêu chí:**

| Tiêu chí | Kết quả | Ghi chú |
|---|---|---|
| C1 Business Alignment | ✅ Đạt | Phục vụ trực tiếp G2 (Churn Reduction, Cash Flow Security) với trigger "2 tuần trước khi cạn gói" đúng Business Goal |
| C2 Actor Completeness | ✅ Đạt | System (Automated Job), Academic Admin, Parent – đủ cho MVP |
| C3 Happy Path | ✅ Đạt | Luồng từ Job → Alert → Admin contact → Phụ huynh đồng ý → Cập nhật hệ thống rõ ràng |
| C4 Exception Path | ⚠️ Thiếu 1 case | Có: (1) Không đủ ngưỡng → skip; (2) Phụ huynh từ chối → At-Risk. **Thiếu:** (3) **Alert đã gửi trước đó, chưa xử lý** → System cần check duplicate alert trước khi tạo mới (tránh spam Admin mỗi ngày với cùng 1 học viên) |
| C5 Automation Trigger | ✅ Đạt | Daily job, alert gửi Admin là tự động; liên hệ Phụ huynh là thủ công (Admin) – rõ ràng |
| C6 Pre/Post Condition | ⚠️ Không tường minh | Pre: Học viên ở trạng thái Active với gói học hợp lệ. Post: Renewal Alert Record được tạo, Admin đã được thông báo |
| C7 Diagram Integrity | ⚠️ Logic không nhất quán | Cell 107 (Cập nhật gói mới) nằm trong lane System (parent=10) nhưng được trigger từ edge của Admin decision (cell 203 trong lane 20) → **cross-lane edge nguồn từ Admin decision nhưng đích là System action, logic đúng về nghiệp vụ nhưng edge nguồn cần review để tránh nhầm ownership**. Ngoài ra cell 303 (End) có 2 incoming edges từ cả 301 lẫn 107 → End node được share đúng cách |

**Status:** 🟡 **Cần bổ sung nhỏ (Minor Fix)**

**Solution:**
1. Thêm bước kiểm tra đầu Job: "Học viên đã có Renewal Alert chưa xử lý?" → Nếu có, skip tạo mới (chống duplicate alert)
2. Thêm trạng thái trung gian: "Renewal Alert Status: Pending → Contacted → Confirmed / Declined"
3. Làm rõ trong diagram: Sau khi Phụ huynh đồng ý, việc "Tạo gói mới" do Admin thực hiện trên hệ thống (không phải System tự tạo) → bổ sung bước "Admin tạo gói mới cho học viên" trong lane Admin trước khi System cập nhật

---

### WF-06 — Tuition Payment Tracking

| Mục | Nội dung |
|---|---|
| **Workflow ID** | WF-06 |
| **Name** | Tuition Payment Tracking – Theo Dõi & Ghi Nhận Học Phí |
| **File** | `CLS_WF06_Tuition_Payment_Tracking.drawio` |
| **KPI liên kết** | Cash Flow Security (G2, G3) |
| **Description** | Admin ghi nhận thanh toán (số tiền, ngày, phương thức) → chọn học viên và kỳ thanh toán → xác nhận → System cập nhật trạng thái (Paid/Partial/Unpaid) + gửi email biên nhận → Phụ huynh nhận xác nhận. Song song: daily job quét học phí quá hạn → alert Admin → Admin follow-up. |

**Kết quả đánh giá theo tiêu chí:**

| Tiêu chí | Kết quả | Ghi chú |
|---|---|---|
| C1 Business Alignment | ✅ Đạt | Phục vụ G2 (Cash Flow Security) và G3 (Admin Efficiency – thay thế nhắc nợ thủ công qua Zalo) |
| C2 Actor Completeness | ⚠️ Thiếu | Workflow bắt đầu từ "Phụ huynh nộp học phí" nhưng Phụ huynh không có action gì trong phần đầu (chỉ nhận email ở phần sau). **Thiếu:** luồng Phụ huynh chủ động hỏi về tình trạng học phí (inbound query) |
| C3 Happy Path | ✅ Đạt | Luồng ghi nhận thanh toán rõ ràng, đủ bước |
| C4 Exception Path | ⚠️ Thiếu | Có: (1) Học phí quá hạn → Admin follow-up. **Thiếu nghiêm trọng:** (2) **Thanh toán một phần (Partial Payment)** → trạng thái Partial đã định nghĩa trong System nhưng không có luồng xử lý tiếp theo; (3) Admin ghi nhận sai số tiền → cần luồng correction/void |
| C5 Automation Trigger | ✅ Đạt | Gửi email biên nhận và daily job quét quá hạn là System tự động; ghi nhận là Admin thủ công |
| C6 Pre/Post Condition | ⚠️ Không tường minh | Pre: Học viên đã nhập học, có invoice/kỳ thanh toán. Post: PaymentRecord tạo thành công, trạng thái cập nhật, email gửi |
| C7 Diagram Integrity | ⚠️ Cấu trúc phức tạp | Diagram có **2 luồng song song độc lập** (Happy Path thanh toán + Daily Job quét quá hạn) merge chung vào các End node khác nhau → cần tách thành 2 page/diagram riêng hoặc dùng ký hiệu rõ hơn để phân biệt 2 luồng. Hiện tại edge từ 202 tới 203 tạo ra jump quá xa trong cùng lane System (y=405 → y=490) |

**Status:** 🟡 **Cần bổ sung nhỏ (Minor Fix)**

**Solution:**
1. Thêm luồng xử lý **Partial Payment**: "Admin ghi nhận còn nợ còn lại → System tạo reminder cho ngày đến hạn tiếp theo"
2. Tách WF-06 thành **2 sub-workflow** hoặc **2 page trong cùng file**: WF-06a (Payment Recording) và WF-06b (Overdue Detection Job) để tăng readability
3. Thêm bước "Admin xác minh Partial payment" với decision: "Đủ số tiền quy định?" → Yes: Paid → No: Partial/flag for follow-up

---

## Tổng Hợp Review

| WF ID | Tên Workflow | Kết quả tổng thể | Ưu tiên sửa | Ghi chú nhanh |
|---|---|---|---|---|
| WF-01 | Student Enrollment | 🟡 Minor Fix | Thấp | Thiếu email bounce exception + overlap cell |
| WF-02 | Class Scheduling | 🟡 Minor Fix | Trung bình | Thiếu luồng hủy lớp + parallel fork không rõ |
| WF-03 | Daily Attendance | 🔴 Major Gap | **Cao nhất** | Thiếu exception Teacher không submit; parallel email logic sai |
| WF-04 | Teacher Feedback SLA | 🔴 Structural Fix | **Cao** | SLA timer cần parallel track; cell overlap; orphan decision |
| WF-05 | Package Renewal Alert | 🟡 Minor Fix | Trung bình | Thiếu duplicate alert check; Admin action cần rõ hơn |
| WF-06 | Tuition Payment Tracking | 🟡 Minor Fix | Trung bình | Thiếu Partial Payment flow; 2 luồng nên tách riêng |

### Nhận xét chung của BA Senior

> **Điểm mạnh:** Toàn bộ 6 workflow đều có KPI mapping rõ ràng, phân lane actor đúng, happy path đầy đủ và cấu trúc swimlane nhất quán. Đây là nền tảng tốt cho MVP.
>
> **Điểm cần cải thiện ưu tiên cao:**
> 1. **WF-03 và WF-04** là 2 workflow có vấn đề nghiêm trọng nhất về logic và exception coverage — đây là 2 workflow vận hành hàng ngày, bất kỳ thiếu sót nào sẽ phát sinh ngay trong tuần đầu go-live.
> 2. **Chung cho tất cả:** Cần bổ sung Pre/Post Condition dưới dạng text annotation bên ngoài swimlane — đây là yêu cầu bắt buộc theo chuẩn BA deliverable.
> 3. **SLA timer trong WF-04** cần được tái thiết kế theo mô hình Boundary Event (song song) thay vì sequential check để phản ánh đúng bản chất thực tế.

**Đề xuất thứ tự sửa:**
1. 🔴 WF-04 → 🔴 WF-03 → 🟡 WF-02 → 🟡 WF-05 → 🟡 WF-06 → 🟡 WF-01
