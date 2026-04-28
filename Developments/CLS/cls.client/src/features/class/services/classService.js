import { apiClient } from '@/shared/services/apiClient';

/**
 * Service layer cho Class Management.
 */

/** GET /api/v1/classes?page=&pageSize= — Danh sách lớp (phân trang) */
export const getClasses = ({ page = 1, pageSize = 10 } = {}) =>
  apiClient.get('/classes', { params: { page, pageSize } });

/** GET /api/v1/classes/dropdown — Danh sách lớp active (cho dropdown) */
export const getClassesDropdown = () =>
  apiClient.get('/classes/dropdown');

/** GET /api/v1/classes/:id — Chi tiết lớp */
export const getClassById = (id) =>
  apiClient.get(`/classes/${id}`);

/** POST /api/v1/classes — Tạo lớp mới */
export const createClass = (payload) =>
  apiClient.post('/classes', payload);

/** PUT /api/v1/classes/:id — Sửa tên lớp */
export const updateClass = (id, payload) =>
  apiClient.put(`/classes/${id}`, payload);

/** PATCH /api/v1/classes/:id/status — Đổi trạng thái */
export const updateClassStatus = (id, payload) =>
  apiClient.patch(`/classes/${id}/status`, payload);

/** GET /api/v1/classes/:id/students — Học sinh trong lớp */
export const getClassStudents = (classId) =>
  apiClient.get(`/classes/${classId}/students`);

/** POST /api/v1/classes/:id/students — Đăng ký nhiều HS vào lớp */
export const enrollStudents = (classId, studentIds) =>
  apiClient.post(`/classes/${classId}/students`, { studentIds });

/** DELETE /api/v1/classes/:id/students/:studentId — Hủy đăng ký */
export const unenrollStudent = (classId, studentId) =>
  apiClient.delete(`/classes/${classId}/students/${studentId}`);
