# CLS Product Backlog — Jira Import Guide

## 📄 File được tạo

**Đường dẫn:**
`Documents\04_Project_Management\01_Plans\CLS_Product_Backlog.csv`

**Encoding:** UTF-8 with BOM (tương thích Jira)

---

## 📊 Tổng quan Backlog

| Epic | Tên | Stories | Tasks | Story Points (est.) |
|------|-----|---------|-------|---------------------|
| EPIC-01 | Student Management | 2 | 4 | 21 |
| EPIC-02 | Financial Administration | 1 | 2 | 11 |
| EPIC-03 | Schedule Management | 2 | 4 | 19 |
| EPIC-04 | Retention Management | 1 | 3 | 11 |
| EPIC-05 | Academic Operations | 2 | 4 | 21 |
| EPIC-06 | Academic Quality Assurance | 1 | 2 | 11 |
| EPIC-07 | System Automation | 2 | 3 | 24 |
| EPIC-08 | Non-Functional Requirements | 1 | 8 | 23 |
| EPIC-09 | Infrastructure & DevOps | 0 | 7 | 21 |
| **Tổng** | | **12** | **37** | **~162 SP** |

> [!NOTE]
> File CSV có **61 dòng** (1 header + 9 epics + 12 stories + 39 tasks).

---

## 🚀 Hướng dẫn Import lên Jira

### Bước 1 — Tạo Jira Project

1. Mở Jira → **Projects** → **Create project**
2. Chọn template **Scrum** hoặc **Kanban**
3. Đặt tên project: `Classroom Management System` — Key: `CLS`
4. Click **Create**

---

### Bước 2 — Cấu hình Issue Types (quan trọng!)

Trước khi import, đảm bảo project có các Issue Types sau:

| Issue Type trong CSV | Issue Type cần có trong Jira |
|----------------------|------------------------------|
| `Epic` | Epic |
| `Story` | Story |
| `Task` | Task |

**Kiểm tra:** Project Settings → Issue types → thêm nếu thiếu.

> [!IMPORTANT]
> Nếu Jira project dùng **Next-gen (Team-managed)**, Epic có thể không cần tạo riêng — thay vào đó dùng **Label** hoặc **Parent link**.

---

### Bước 3 — Import CSV

1. Vào **Jira Settings (⚙️)** → **System** → **External System Import**
   *(hoặc tìm kiếm "CSV" trong Jira admin)*

2. Chọn **CSV** → **Next**

3. Upload file `CLS_Product_Backlog.csv`

4. **Mapping columns** — ánh xạ như sau:

| Cột CSV | Jira Field |
|---------|-----------|
| `Summary` | Summary |
| `Issue Type` | Issue Type |
| `Priority` | Priority |
| `Story Points` | Story Points |
| `Description` | Description |
| `Acceptance Criteria` | Acceptance Criteria *(hoặc Description nếu field chưa có)* |
| `Epic Name` | Epic Name *(dùng để link Epic)* |
| `Labels` | Labels |
| `Components` | Component/s |
| `Fix Version/s` | Fix Version/s |

5. Click **Next** → **Begin Import**

---

### Bước 4 — Xử lý sau Import

#### 4.1 Link Stories/Tasks vào đúng Epic
Jira CSV import sẽ dùng cột `Epic Name` để tự động link. Nếu không tự link:
1. Filter issues theo Epic Name label
2. Dùng **Bulk Edit** → set **Epic Link**

#### 4.2 Tạo Versions/Releases
1. Project → **Releases** → **Create version**
2. Tạo version `v1.0` (Release date: theo Sprint plan)

#### 4.3 Tạo Components
Tạo các Components sau trong Project Settings:

| Component | Mô tả |
|-----------|-------|
| `Backend` | API, Business Logic, Database |
| `Frontend` | UI screens, client-side logic |
| `DevOps` | Infrastructure, CI/CD, deployment |

---

### Bước 5 — Tạo Sprints (Scrum)

Đề xuất Sprint plan dựa trên Story Points:

| Sprint | Epics chính | Est. Story Points |
|--------|------------|-------------------|
| Sprint 1 | EPIC-09 (Setup) + EPIC-01 | ~34 SP |
| Sprint 2 | EPIC-02 + EPIC-03 | ~30 SP |
| Sprint 3 | EPIC-04 + EPIC-05 | ~32 SP |
| Sprint 4 | EPIC-06 + EPIC-07 | ~35 SP |
| Sprint 5 | EPIC-08 (NFR) + Tech Debt | ~31 SP |

---

## ⚠️ Lưu ý quan trọng

> [!WARNING]
> **Acceptance Criteria** có ký tự xuống dòng (`\n`). Jira CSV import có thể hiển thị thành một khối text. Sau import, review và format lại trong Jira nếu cần.

> [!TIP]
> **Alternative import:** Dùng **Jira Automation** hoặc **Jira API** (`POST /rest/api/3/issue`) để import có kiểm soát tốt hơn, đặc biệt khi cần set parent-child links chính xác.

> [!NOTE]
> Với Jira **Cloud** (team-managed projects), bạn có thể dùng tính năng **Import** tích hợp sẵn:  
> Project Board → **⋯ More** → **Import issues from CSV**

---

## 🔗 Nguồn tham khảo

- UC-01 → UC-11: SRS Section 2 (Use Case Specifications)
- Functional Requirements: SRS Section 3
- NFRs: SRS Section 4 (Quality Attributes)
- Business Rules (BR-01 → BR-05): SRS Section 5.1
- Database schema: SDS Section 1.3 (14 tables)
- Architecture: SDS Section 1.1 (Layered Architecture, Supabase/PostgreSQL)
