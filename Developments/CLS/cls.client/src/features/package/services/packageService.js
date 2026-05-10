import { apiClient } from '@/shared/services/apiClient';

/** GET /api/v1/tuition-packages?page=&pageSize= */
export const getPackages = ({ page = 1, pageSize = 10 } = {}) =>
  apiClient.get('/tuition-packages', { params: { page, pageSize } });

/** POST /api/v1/tuition-packages */
export const createPackage = (payload) => apiClient.post('/tuition-packages', payload);

/** PUT /api/v1/tuition-packages/:id */
export const updatePackage = (id, payload) => apiClient.put(`/tuition-packages/${id}`, payload);

/** PATCH /api/v1/tuition-packages/:id/status */
export const updatePackageStatus = (id, payload) => apiClient.patch(`/tuition-packages/${id}/status`, payload);
