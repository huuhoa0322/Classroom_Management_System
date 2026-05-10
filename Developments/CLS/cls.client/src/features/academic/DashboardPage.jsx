import { Link } from 'react-router-dom';
import { useDashboardStats } from './hooks/useDashboard';
import { ROUTE_PATHS } from '@/shared/utils/constants';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { formatCurrency } from '@/shared/utils/formatters';

/**
 * DashboardPage — Trang tổng quan cho Admin với thống kê thực tế.
 */
export default function DashboardPage() {
  const { data: stats, isLoading, isError, error, refetch } = useDashboardStats();

  const cards = [
    { label: 'Học sinh', value: stats?.totalStudents, icon: '👨‍🎓', color: 'bg-indigo-50 text-indigo-600', to: ROUTE_PATHS.STUDENTS },
    { label: 'Lớp hoạt động', value: stats?.totalClasses, icon: '🏫', color: 'bg-blue-50 text-blue-600', to: ROUTE_PATHS.CLASSES },
    { label: 'Giáo viên', value: stats?.totalTeachers, icon: '👤', color: 'bg-emerald-50 text-emerald-600', to: ROUTE_PATHS.USERS },
    { label: 'Buổi sắp tới', value: stats?.upcomingSessions, icon: '📅', color: 'bg-amber-50 text-amber-600', to: ROUTE_PATHS.SESSIONS },
    { label: 'Doanh thu', value: stats ? formatCurrency(stats.totalRevenue) : undefined, icon: '💰', color: 'bg-green-50 text-green-600', to: ROUTE_PATHS.PAYMENTS },
    { label: 'Cần gia hạn', value: stats?.pendingAlerts, icon: '⚠️', color: 'bg-red-50 text-red-600', to: ROUTE_PATHS.RENEWAL_ALERTS, highlight: (stats?.pendingAlerts || 0) > 0 },
  ];

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-1">Dashboard</h1>
      <p className="text-sm text-gray-500">Tổng quan Hệ thống Quản lý Lớp học CLS</p>

      {/* Connection / server error banner */}
      {isError && (
        <div className="mt-4">
          <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
        </div>
      )}

      <div className="mt-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {cards.map((card) => (
          <Link
            key={card.label}
            to={card.to}
            className={`rounded-xl p-5 ${card.color} border border-white shadow-sm hover:shadow-md transition-all hover:scale-[1.02] ${card.highlight ? 'ring-2 ring-red-300' : ''}`}
          >
            <div className="text-2xl mb-2">{card.icon}</div>
            <div className="text-2xl font-bold">
              {isLoading ? (
                <div className="h-7 w-16 bg-gray-200/50 rounded animate-pulse" />
              ) : isError ? (
                <span className="text-base text-gray-400">—</span>
              ) : (
                card.value ?? '—'
              )}
            </div>
            <div className="text-sm font-medium mt-1">{card.label}</div>
          </Link>
        ))}
      </div>
    </div>
  );
}

