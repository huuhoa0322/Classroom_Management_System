import { apiClient } from '@/shared/services/apiClient';

/**
 * Service layer cho Academic Operations (UC-07 + UC-08).
 * Gọi API endpoints dành cho Teacher.
 */

/** GET /api/v1/teacher/timetable — Lịch dạy theo tuần */
export const getTimetable = ({ from, to } = {}) =>
  apiClient.get('/teacher/timetable', { params: { from, to } });

/** GET /api/v1/teacher/sessions/:id/attendance — Sheet điểm danh */
export const getAttendanceSheet = (sessionId) =>
  apiClient.get(`/teacher/sessions/${sessionId}/attendance`);

/** POST /api/v1/teacher/sessions/:id/attendance — Submit điểm danh */
export const submitAttendance = (sessionId, records) =>
  apiClient.post(`/teacher/sessions/${sessionId}/attendance`, { records });
