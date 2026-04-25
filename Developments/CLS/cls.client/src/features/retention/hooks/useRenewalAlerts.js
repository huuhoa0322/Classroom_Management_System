import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { getRenewalAlerts, updateAlertStatus } from '../services/renewalAlertService';

/**
 * Hook: Lấy danh sách cảnh báo gia hạn (phân trang, sort, filter).
 * @param {number} page
 * @param {number} pageSize
 * @param {string|null} status - 'pending' | 'consulted' | null (all)
 * @param {string|null} sortBy - 'studentName' | 'remainingSessions' | 'remainingDays' | 'createdAt'
 * @param {string|null} sortDir - 'asc' | 'desc'
 */
export function useRenewalAlerts(page = 1, pageSize = 10, status = null, sortBy = null, sortDir = null) {
  return useQuery({
    queryKey: ['renewalAlerts', page, pageSize, status, sortBy, sortDir],
    queryFn: () => getRenewalAlerts({ page, pageSize, status, sortBy, sortDir }),
  });
}

/**
 * Hook: Cập nhật trạng thái alert (pending ↔ consulted).
 * Sau khi thành công → invalidate cache.
 */
export function useUpdateAlertStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => updateAlertStatus(id, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['renewalAlerts'] });
    },
  });
}
