import { apiClient } from '@/shared/services/apiClient';

/**
 * Service layer cho Schedule Management (CLS-004 + CLS-005).
 */

/** POST /api/v1/sessions — Tạo buổi học mới */
export const createSession = (payload) =>
  apiClient.post('/sessions', payload);

/** PUT /api/v1/sessions/:id — Cập nhật buổi học */
export const updateSession = (id, payload) =>
  apiClient.put(`/sessions/${id}`, payload);

/** DELETE /api/v1/sessions/:id — Xóa buổi học */
export const deleteSession = (id) =>
  apiClient.delete(`/sessions/${id}`);

/** GET /api/v1/sessions?page=...&pageSize=... — Danh sách sessions */
export const getAllSessions = ({ page = 1, pageSize = 10 } = {}) =>
  apiClient.get('/sessions', { params: { page, pageSize } });

/** GET /api/v1/classes — Danh sách lớp (dropdown) */
export const getClasses = () =>
  apiClient.get('/classes');

/** GET /api/v1/rooms — Danh sách phòng (dropdown) */
export const getRooms = () =>
  apiClient.get('/rooms');

/** GET /api/v1/teachers — Danh sách giáo viên (dropdown) */
export const getTeachers = () =>
  apiClient.get('/teachers');
