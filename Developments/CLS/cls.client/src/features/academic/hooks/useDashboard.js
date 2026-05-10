import { useQuery } from '@tanstack/react-query';
import { getDashboardStats } from '../services/dashboardService';

/** Query key factory cho Dashboard. */
export const dashboardKeys = {
  all: ['dashboard'],
  stats: () => ['dashboard', 'stats'],
};

/** Thống kê tổng quan Dashboard. */
export function useDashboardStats() {
  return useQuery({
    queryKey: dashboardKeys.stats(),
    queryFn: getDashboardStats,
  });
}
