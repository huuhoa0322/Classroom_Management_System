import { apiClient } from '@/shared/services/apiClient';

/** GET /api/v1/rooms?page=&pageSize= */
export const getRooms = ({ page = 1, pageSize = 10 } = {}) =>
  apiClient.get('/rooms', { params: { page, pageSize } });

/** POST /api/v1/rooms */
export const createRoom = (payload) => apiClient.post('/rooms', payload);

/** PUT /api/v1/rooms/:id */
export const updateRoom = (id, payload) => apiClient.put(`/rooms/${id}`, payload);

/** PATCH /api/v1/rooms/:id/status */
export const updateRoomStatus = (id, payload) => apiClient.patch(`/rooms/${id}/status`, payload);
