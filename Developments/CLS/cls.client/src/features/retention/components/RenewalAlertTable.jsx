import { useState } from 'react';
import { useRenewalAlerts, useUpdateAlertStatus } from '../hooks/useRenewalAlerts';
import { formatDateTime, getStatusBadge } from '@/shared/utils/formatters';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { useToastStore } from '@/shared/stores/toastStore';
import { ALERT_STATUS } from '@/shared/utils/constants';

/**
 * RenewalAlertTable — Bảng thông báo gia hạn gói học (CLS-006).
 * Features: phân trang, sort, filter status, toggle "Đã tư vấn".
 */
export function RenewalAlertTable() {
  const addToast = useToastStore((s) => s.addToast);

  // ── State ──────────────────────────────────────────────────────────────
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [statusFilter, setStatusFilter] = useState(null);
  const [sortBy, setSortBy] = useState('createdAt');
  const [sortDir, setSortDir] = useState('desc');

  // ── Data ───────────────────────────────────────────────────────────────
  const { data, isLoading, error, refetch } = useRenewalAlerts(page, pageSize, statusFilter, sortBy, sortDir);
  const updateStatus = useUpdateAlertStatus();

  const alerts = data?.items ?? [];
  const totalPages = data?.totalPages ?? 0;
  const totalCount = data?.totalCount ?? 0;

  // ── Handlers ───────────────────────────────────────────────────────────
  const handleSort = (column) => {
    if (sortBy === column) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortBy(column);
      setSortDir('asc');
    }
    setPage(1);
  };

  const handleToggleStatus = (alert) => {
    const newStatus = alert.status === ALERT_STATUS.PENDING
      ? ALERT_STATUS.CONSULTED
      : ALERT_STATUS.PENDING;

    updateStatus.mutate(
      { id: alert.id, status: newStatus },
      {
        onSuccess: () => {
          addToast({
            type: 'success',
            message: newStatus === ALERT_STATUS.CONSULTED
              ? `Đã đánh dấu "Đã tư vấn" cho ${alert.studentName}`
              : `Đã mở lại cảnh báo cho ${alert.studentName}`,
          });
        },
        onError: (err) => {
          addToast({ type: 'error', message: err.message || 'Có lỗi xảy ra.' });
        },
      }
    );
  };

  const SortIcon = ({ column }) => {
    if (sortBy !== column) return <span className="text-gray-300 ml-1">↕</span>;
    return <span className="ml-1">{sortDir === 'asc' ? '↑' : '↓'}</span>;
  };

  // ── Loading ────────────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <div key={i} className="h-14 bg-gray-100 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  // ── Error ──────────────────────────────────────────────────────────────
  if (error) {
    return (
      <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
    );
  }

  return (
    <div>
      {/* ── Filter Tabs ──────────────────────────────────────────────── */}
      <div className="flex gap-2 mb-4">
        {[
          { label: `Tất cả (${totalCount})`, value: null },
          { label: 'Chờ xử lý', value: ALERT_STATUS.PENDING },
          { label: 'Đã tư vấn', value: ALERT_STATUS.CONSULTED },
        ].map((tab) => (
          <button
            key={tab.value ?? 'all'}
            onClick={() => { setStatusFilter(tab.value); setPage(1); }}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
              statusFilter === tab.value
                ? 'bg-indigo-600 text-white'
                : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* ── Table ─────────────────────────────────────────────────────── */}
      {alerts.length === 0 ? (
        <div className="text-center py-16 text-gray-400">
          <span className="text-4xl">🎉</span>
          <p className="mt-3 text-lg font-medium">Không có cảnh báo nào</p>
          <p className="text-sm">Tất cả gói học đang ở trạng thái tốt.</p>
        </div>
      ) : (
        <div className="overflow-x-auto border border-gray-200 rounded-xl">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left text-gray-600">
                <th
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-indigo-600"
                  onClick={() => handleSort('studentName')}
                >
                  Học sinh <SortIcon column="studentName" />
                </th>
                <th className="px-4 py-3 font-semibold">Phụ huynh</th>
                <th className="px-4 py-3 font-semibold">Gói học</th>
                <th
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-indigo-600 text-center"
                  onClick={() => handleSort('remainingSessions')}
                >
                  Buổi còn lại <SortIcon column="remainingSessions" />
                </th>
                <th
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-indigo-600 text-center"
                  onClick={() => handleSort('remainingDays')}
                >
                  Ngày còn lại <SortIcon column="remainingDays" />
                </th>
                <th
                  className="px-4 py-3 font-semibold cursor-pointer hover:text-indigo-600"
                  onClick={() => handleSort('createdAt')}
                >
                  Ngày tạo <SortIcon column="createdAt" />
                </th>
                <th className="px-4 py-3 font-semibold">Chi tiết</th>
                <th className="px-4 py-3 font-semibold text-center">Trạng thái</th>
                <th className="px-4 py-3 font-semibold text-center">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {alerts.map((alert) => {
                const badge = getStatusBadge(alert.status);
                const isConsulted = alert.status === ALERT_STATUS.CONSULTED;

                return (
                  <tr key={alert.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 font-medium text-gray-900">{alert.studentName}</td>
                    <td className="px-4 py-3">
                      <div className="text-gray-800">{alert.parentName}</div>
                      <div className="text-xs text-gray-400">{alert.parentEmail}</div>
                      {alert.parentPhone && (
                        <div className="text-xs text-gray-400">{alert.parentPhone}</div>
                      )}
                    </td>
                    <td className="px-4 py-3 text-gray-700">{alert.packageName}</td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-block min-w-[2rem] font-bold ${
                        alert.remainingSessions <= 2 ? 'text-red-600' : 'text-orange-500'
                      }`}>
                        {alert.remainingSessions}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-block min-w-[2rem] font-bold ${
                        alert.remainingDays <= 7 ? 'text-red-600' : 'text-orange-500'
                      }`}>
                        {alert.remainingDays}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-gray-500">{formatDateTime(alert.createdAt)}</td>
                    <td className="px-4 py-3 text-gray-500 text-xs max-w-[220px]">
                      <span className="line-clamp-2" title={alert.message}>{alert.message}</span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>
                        {badge.label}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <button
                        onClick={() => handleToggleStatus(alert)}
                        disabled={updateStatus.isPending}
                        className={`px-3 py-1.5 text-xs font-medium rounded-lg transition-all border ${
                          isConsulted
                            ? 'border-gray-300 text-gray-500 hover:bg-gray-100'
                            : 'border-emerald-300 text-emerald-700 bg-emerald-50 hover:bg-emerald-100'
                        } disabled:opacity-50`}
                      >
                        {isConsulted ? '↩ Mở lại' : '✓ Đã tư vấn'}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* ── Pagination ─────────────────────────────────────────────────── */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">
          <p className="text-sm text-gray-500">
            Trang {page} / {totalPages} ({totalCount} cảnh báo)
          </p>
          <div className="flex gap-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
              className="px-3 py-1.5 text-sm border rounded-lg hover:bg-gray-50 disabled:opacity-40"
            >
              ← Trước
            </button>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages}
              className="px-3 py-1.5 text-sm border rounded-lg hover:bg-gray-50 disabled:opacity-40"
            >
              Sau →
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
