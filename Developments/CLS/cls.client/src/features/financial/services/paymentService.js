import { apiClient } from '@/shared/services/apiClient';

/**
 * Service layer cho Financial Administration (CLS-003).
 * Gọi API endpoints liên quan đến payment, student package, tuition package.
 */

/** POST /api/v1/payments — Ghi nhận thanh toán mới */
export const recordPayment = (payload) =>
  apiClient.post('/payments', payload);

/** PATCH /api/v1/payments/:id/status — Cập nhật trạng thái thanh toán */
export const updatePaymentStatus = (id, payload) =>
  apiClient.patch(`/payments/${id}/status`, payload);

/** GET /api/v1/payments?studentId=...&page=...&pageSize=... */
export const getStudentPayments = (studentId, { page = 1, pageSize = 10 } = {}) =>
  apiClient.get('/payments', { params: { studentId, page, pageSize } });

/** GET /api/v1/students/:id/packages — Danh sách gói học của học sinh */
export const getStudentPackages = (studentId) =>
  apiClient.get(`/students/${studentId}/packages`);

/** GET /api/v1/tuition-packages — Catalog gói học */
export const getAvailablePackages = () =>
  apiClient.get('/tuition-packages');
