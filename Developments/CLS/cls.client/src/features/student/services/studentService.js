import { apiClient } from '@/shared/services/apiClient';

/**
 * Service giao tiếp với Students API.
 * Mọi lệnh gọi đều qua apiClient — response đã được unwrap ApiResponse<T>.
 */
export const studentService = {
  /** GET /api/v1/students — danh sách phân trang */
  getAll: ({ page = 1, pageSize = 10, status } = {}) => {
    const params = { page, pageSize };
    if (status) params.status = status;
    return apiClient.get('/students', { params });
  },

  /** GET /api/v1/students/:id */
  getById: (id) => apiClient.get(`/students/${id}`),

  /** POST /api/v1/students — Tạo mới (CLS-001) */
  create: (data) => apiClient.post('/students', data),

  /** PUT /api/v1/students/:id — Cập nhật thông tin */
  update: (id, data) => apiClient.put(`/students/${id}`, data),

  /** PATCH /api/v1/students/:id/status — Đổi lifecycle status (CLS-002) */
  updateStatus: (id, status) => apiClient.patch(`/students/${id}/status`, { status }),
};
