# 🏗️ Giải phẫu Frontend Infrastructure (cls.client)

Tài liệu này giải thích chi tiết từng tệp tin và cơ chế hoạt động của toàn bộ nền móng Frontend mà chúng ta vừa xây dựng ở Phase 1. Các tệp này đóng vai trò như "xương sống", giúp việc phát triển các tính năng sau này trở nên chuẩn mực và tự động hóa cao.

---

## 1. Lõi Giao Tiếp Mạng (Networking Layer)

### 📄 `src/shared/services/apiClient.js`
**Nhiệm vụ:** Là cánh cổng duy nhất để Frontend gọi API xuống Backend.
**Cách hoạt động:**
- **Request Interceptor (Gửi đi):** Tự động vào kho lưu trữ (Zustand) lấy `accessToken` và gắn vào Header `Authorization: Bearer ...`. Từ nay, khi gọi API, lập trình viên không bao giờ phải viết code gắn token thủ công.
- **Response Interceptor (Nhận về):** 
  - Backend của chúng ta luôn trả về chuẩn `ApiResponse<T>` (gồm `success`, `message`, `data`). Trình chặn này sẽ tự động **unwrap** (giải nén), bóc lớp vỏ ngoài và chỉ trả về phần `.data` lõi cho Component.
  - **Auto-Logout:** Nếu Backend trả về mã lỗi `401 Unauthorized` (Token hết hạn), nó sẽ tự động xóa sạch phiên đăng nhập và ép trình duyệt quay về trang `/login` ngay lập tức.

---

## 2. Quản Lý Trạng Thái Toàn Cục (Global State)

### 📄 `src/app/provider/authStore.js`
**Nhiệm vụ:** Lưu trữ thông tin người dùng đang đăng nhập.
**Cách hoạt động:**
- Sử dụng **Zustand**, một thư viện quản lý state cực kỳ nhẹ và nhanh gọn hơn Redux.
- Tích hợp middleware `persist` để lưu state xuống `localStorage`.
- **Cơ chế bảo mật đặc biệt:** Khác với các dự án thông thường lưu cả thông tin User vào localStorage (dễ bị tấn công XSS lấy cắp email/tên), chúng ta áp dụng hàm `partialize`. Hàm này chỉ cho phép lưu token xuống ổ cứng, còn đối tượng `user` hoàn toàn chỉ bay lơ lửng trên RAM (bộ nhớ tạm). F5 lại trang sẽ phải móc token gọi API lấy lại User.

---

## 3. Hệ Thống Tiện Ích & Định Chuẩn (Shared Utils)

### 📄 `src/shared/utils/constants.js`
**Nhiệm vụ:** Đồng bộ hóa các "Magic Strings" với Backend.
**Cách hoạt động:** Mọi role (`Admin`, `Teacher`) hay URL route (`/students`) đều được khai báo hằng số tại đây. Nếu sau này URL đổi từ `/students` thành `/hoc-sinh`, ta chỉ cần sửa đúng 1 dòng ở đây, toàn bộ các nút bấm và Route trên app sẽ tự động cập nhật.

### 📄 `src/shared/utils/formatters.js`
**Nhiệm vụ:** Biến đổi dữ liệu thô thành định dạng hiển thị cho người dùng.
**Cách hoạt động:**
- Cung cấp các hàm dùng chung: `formatDate()` (Ngày tháng kiểu Việt Nam), `formatCurrency()` (Tiền tệ VNĐ).
- Hàm `getStatusBadge(status)`: Nhận vào chữ `active`, nhả ra HTML class Tailwind màu Xanh lục; nhận `expired`, nhả ra class màu Đỏ. Rất tiện để vẽ nhãn (Badge) trên bảng.

---

## 4. Hệ Thống Điều Hướng & Bảo Vệ (Routing & Guards)

### 📄 `src/app/guards/PrivateRoute.jsx`
**Nhiệm vụ:** Làm người bảo vệ (Bouncer) gác cửa các trang nội bộ.
**Cách hoạt động:** Bọc quanh một Component. Khi có người truy cập, nó quét trạng thái `isAuthenticated` từ Zustand. 
- Đã đăng nhập? Cho qua. 
- Chưa đăng nhập? Đá văng về `/login`. 
- Thậm chí lưu lại biến `state={{ from: location }}` để sau khi họ đăng nhập xong, nó tự động dắt họ quay đúng lại trang họ vừa muốn vào.

### 📄 `src/app/layouts/MainLayout.jsx` & `AuthLayout.jsx`
**Nhiệm vụ:** Chia khung (Layout) màn hình.
**Cách hoạt động:**
- `AuthLayout`: Dùng riêng cho trang Login. Vẽ nền Gradient và một thẻ Card nằm ngay chính giữa.
- `MainLayout`: Vẽ thanh Sidebar bên trái và thanh Topbar ở trên cùng. Tích hợp nút Đăng xuất gọi thẳng vào hàm `logout()` của Zustand. Giao diện Sidebar sử dụng Flexbox và `translate-x` của Tailwind để tự động trượt ra trượt vào trên màn hình điện thoại (Responsive mượt mà).

### 📄 `src/app/routers/AppRouter.jsx`
**Nhiệm vụ:** Bản đồ dẫn đường.
**Cách hoạt động:** Phân bổ rõ ràng:
- Nhóm Public: Bọc bằng `<AuthLayout>`
- Nhóm Nội bộ (Cần bảo vệ): Bọc bằng `<PrivateRoute>` + `<MainLayout>`
- Trang 404 Fallback: Bất kỳ URL nào gõ sai (`*`) đều bị điều hướng ngược về trang chủ `/`.

---

## 5. Trái Tim Của Ứng Dụng (Root Entry)

### 📄 `src/App.jsx`
**Nhiệm vụ:** Khởi động hệ sinh thái.
**Cách hoạt động:** Xóa sạch file mặc định của Vite, bọc toàn bộ dự án bằng 2 lớp áo giáp:
1. `<QueryClientProvider>`: Khởi động TanStack Query (React Query) với cấu hình bộ đệm (`staleTime: 5 phút`). Giúp ứng dụng nhớ dữ liệu tạm thời, không phải tải lại API liên tục làm chậm máy chủ.
2. `<BrowserRouter>`: Khởi động hệ thống Router.

### 📄 `vite.config.js` & `index.css`
**Nhiệm vụ:** Biên dịch và Cấu hình CSS.
**Cách hoạt động:** 
- `vite.config.js` đã được tích hợp bộ dịch (compiler) siêu tốc `@tailwindcss/vite` mới nhất của Tailwind v4.
- `index.css` nhập khẩu bộ font chữ quốc dân `Inter` từ Google Fonts, mang lại vẻ ngoài vô cùng chuyên nghiệp và sắc nét cho toàn bộ UI.
