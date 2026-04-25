import axios from 'axios';
import { useAuthStore } from '@/app/provider/authStore';

export class ApiError extends Error {
  constructor(message, { status, code, data } = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.data = data;
  }
}

/**
 * Axios instance cốt lõi của CLS Frontend.
 * - Tự động đính JWT Bearer Token vào mọi request.
 * - Tự động unwrap định dạng ApiResponse<T> { code, message, data } từ Backend.
 * - Xử lý lỗi 401 (hết phiên) và 403 (không có quyền) tập trung.
 */
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  // Render free tier cold start có thể mất 30-60s → timeout 60s để tránh false timeout
  timeout: 60000,
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
        return Promise.reject(
          new ApiError(apiResponse.message || 'Có lỗi xảy ra từ máy chủ.', {
            status: apiResponse.code,
            code: apiResponse.code,
            data: apiResponse.data,
          })
        );
      }
      // Unwrap: trả về phần data (LoginResponse, PagedResult, entity object, v.v.)
      return apiResponse.data;
    }

    // Response không theo chuẩn ApiResponse (ví dụ: file download)
    return response.data;
  },
  (error) => {
    // ── Timeout — request không nhận được response ────────────────────────
    if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
      return Promise.reject(
        new ApiError('Yêu cầu quá thời gian chờ. Vui lòng kiểm tra kết nối và thử lại.', {
          code: 'TIMEOUT',
        })
      );
    }

    // ── Network error — không kết nối được tới server ────────────────────
    if (!error.response) {
      return Promise.reject(
        new ApiError('Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.', {
          code: 'NETWORK_ERROR',
        })
      );
    }

    const status  = error.response.status;
    const body = error.response.data;
    const message = body?.message;
    const errorMeta = {
      status,
      code: body?.code ?? status,
      data: body?.data,
    };

    if (status === 400) {
      // Validation error từ backend
      return Promise.reject(new ApiError(message || 'Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.', errorMeta));
    }

    if (status === 401) {
      const requestUrl = error.config?.url || '';
      const isLoginRequest = requestUrl.includes('/auth/login');
      if (!isLoginRequest) {
        useAuthStore.getState().logout();
        window.location.href = '/login';
      }
      return Promise.reject(
        new ApiError(
          isLoginRequest
            ? message || 'Email hoặc mật khẩu không chính xác.'
            : 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
          errorMeta
        )
      );
    }

    if (status === 403) {
      return Promise.reject(new ApiError('Bạn không có quyền thực hiện thao tác này.', errorMeta));
    }

    if (status === 404) {
      return Promise.reject(new ApiError(message || 'Không tìm thấy dữ liệu yêu cầu.', errorMeta));
    }

    if (status === 409) {
      // Conflict — scheduling conflict, duplicate, state transition error
      return Promise.reject(new ApiError(message || 'Xung đột dữ liệu. Vui lòng thử lại.', errorMeta));
    }

    if (status >= 500) {
      return Promise.reject(new ApiError('Lỗi máy chủ. Vui lòng thử lại sau.', errorMeta));
    }

    // Fallback
    return Promise.reject(new ApiError(message || error.message || 'Có lỗi không xác định xảy ra.', errorMeta));
  }
);

export const apiClient = axiosInstance;
