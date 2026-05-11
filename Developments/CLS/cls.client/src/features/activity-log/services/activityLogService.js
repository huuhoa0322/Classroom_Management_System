import { apiClient } from '@/shared/services/apiClient';

/** GET /api/v1/activity-logs?page=&pageSize=&userId=&actionType=&from=&to= */
export const getActivityLogs = ({ page = 1, pageSize = 10, userId, actionType, from, to } = {}) =>
  apiClient.get('/activity-logs', {
    params: { page, pageSize, userId, actionType, from, to },
  });
