import { apiClient } from '@/shared/services/apiClient';

/** GET /api/v1/users?page=&pageSize= */
export const getUsers = ({ page = 1, pageSize = 10 } = {}) =>
  apiClient.get('/users', { params: { page, pageSize } });

/** POST /api/v1/users */
export const createUser = (payload) => apiClient.post('/users', payload);

/** PUT /api/v1/users/:id */
export const updateUser = (id, payload) => apiClient.put(`/users/${id}`, payload);

/** PATCH /api/v1/users/:id/status */
export const updateUserStatus = (id, payload) => apiClient.patch(`/users/${id}/status`, payload);

/** PATCH /api/v1/users/:id/reset-password */
export const resetUserPassword = (id) => apiClient.patch(`/users/${id}/reset-password`);

/** PATCH /api/v1/users/:id/toggle-lock */
export const toggleUserLock = (id) => apiClient.patch(`/users/${id}/toggle-lock`);
