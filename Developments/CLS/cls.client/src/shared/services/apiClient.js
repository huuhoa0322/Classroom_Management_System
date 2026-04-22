import axios from 'axios';
import { useAuthStore } from '@/app/provider/authStore';

/**
 * Axios instance cốt lõi của CLS Frontend.
 * - Tự động đính JWT Bearer Token vào mọi request.
 * - Tự động unwrap định dạng ApiResponse<T> { code, message, data } từ Backend.
 * - Xử lý lỗi 401 (hết phiên) và 403 (không có quyền) tập trung.
 */
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// ── Request Interceptor: Đính JWT Token vào Header ──────────────────────────
axiosInstance.interceptors.request.use(
  (config) => {
    const token = useAuthStore.getState().accessToken;
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Response Interceptor: Unwrap ApiResponse<T> + Xử lý lỗi ────────────────
axiosInstance.interceptors.response.use(
  (response) => {
    // Backend luôn trả về { code, message, data }
    const apiResponse = response.data;

    if (apiResponse && typeof apiResponse === 'object' && 'code' in apiResponse) {
      // Nếu code không phải 2xx → reject với message từ backend
      if (apiResponse.code < 200 || apiResponse.code >= 300) {
        return Promise.reject(new Error(apiResponse.message || 'Có lỗi xảy ra từ máy chủ.'));
      }
      // Unwrap: trả về phần data (LoginResponse, PagedResult, entity object, v.v.)
      return apiResponse.data;
    }

    // Response không theo chuẩn ApiResponse (ví dụ: file download)
    return response.data;
  },
  (error) => {
    const status  = error.response?.status;
    const message = error.response?.data?.message;

    if (status === 401) {
      // Token hết hạn hoặc không hợp lệ → Logout và redirect về trang đăng nhập
      useAuthStore.getState().logout();
      window.location.href = '/login';
      return Promise.reject(new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.'));
    }

    if (status === 403) {
      return Promise.reject(new Error('Bạn không có quyền thực hiện thao tác này.'));
    }

    if (status === 404) {
      return Promise.reject(new Error(message || 'Không tìm thấy dữ liệu yêu cầu.'));
    }

    if (status >= 500) {
      return Promise.reject(new Error('Lỗi máy chủ. Vui lòng thử lại sau.'));
    }

    return Promise.reject(new Error(message || error.message || 'Có lỗi không xác định xảy ra.'));
  }
);

export const apiClient = axiosInstance;
