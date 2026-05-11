import { useQuery } from '@tanstack/react-query';
import { getActivityLogs } from '../services/activityLogService';
import { DEFAULT_PAGINATION } from '@/shared/utils/constants';

export const activityLogKeys = {
  all: ['activity-logs'],
  list: (params) => ['activity-logs', 'list', params],
};

/**
 * Hook lấy danh sách Activity Logs phân trang + lọc.
 * @param {object} params - { page, pageSize, userId, actionType, from, to }
 */
export function useActivityLogList({ page = DEFAULT_PAGINATION.PAGE, pageSize = DEFAULT_PAGINATION.PAGE_SIZE, userId, actionType, from, to } = {}) {
  return useQuery({
    queryKey: activityLogKeys.list({ page, pageSize, userId, actionType, from, to }),
    queryFn: () => getActivityLogs({ page, pageSize, userId, actionType, from, to }),
  });
}
