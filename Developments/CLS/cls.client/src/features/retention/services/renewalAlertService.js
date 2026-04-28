import { apiClient } from '@/shared/services/apiClient';

/**
 * Service layer cho Retention Management (CLS-006 + CLS-010).
 * Gọi API endpoints liên quan đến renewal alerts.
 */

/** GET /api/v1/renewal-alerts — Danh sách thông báo gia hạn (phân trang, sort, filter) */
export const getRenewalAlerts = ({ page = 1, pageSize = 10, status, sortBy, sortDir } = {}) =>
  apiClient.get('/renewal-alerts', {
    params: { page, pageSize, status, sortBy, sortDir },
  });

/** PATCH /api/v1/renewal-alerts/:id/status — Toggle trạng thái alert */
export const updateAlertStatus = (id, payload) =>
  apiClient.patch(`/renewal-alerts/${id}/status`, payload);
