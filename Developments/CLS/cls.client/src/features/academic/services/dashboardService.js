import { apiClient } from '@/shared/services/apiClient';

/** GET /api/v1/dashboard/stats — Thống kê tổng quan */
export const getDashboardStats = () => apiClient.get('/dashboard/stats');
