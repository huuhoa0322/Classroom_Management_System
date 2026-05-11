import { useState } from 'react';
import { useActivityLogList } from '../hooks/useActivityLog';
import { ActivityLogTable } from '../components/ActivityLogTable';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { DEFAULT_PAGINATION } from '@/shared/utils/constants';

/** Danh sách action types cho dropdown lọc. */
const ACTION_TYPE_OPTIONS = [
  { value: '',              label: 'Tất cả hành động' },
  { value: 'create',        label: '➕ Tạo mới' },
  { value: 'update',        label: '✏️ Cập nhật' },
  { value: 'delete',        label: '🗑️ Xóa' },
  { value: 'login',         label: '🔑 Đăng nhập' },
  { value: 'logout',        label: '🚪 Đăng xuất' },
  { value: 'status_change', label: '🔄 Đổi trạng thái' },
];

/**
 * ActivityLogPage — Trang quản lý nhật ký hoạt động (Admin only).
 * Bao gồm: bộ lọc (actionType, date range) + bảng phân trang.
 */
export default function ActivityLogPage() {
  const [page, setPage] = useState(1);
  const [actionType, setActionType] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  // Parse date strings sang DateTime cho API
  const from = fromDate ? new Date(`${fromDate}T00:00:00`).toISOString() : undefined;
  const to = toDate ? new Date(`${toDate}T23:59:59`).toISOString() : undefined;

  const { data: logs, isLoading, isError, error, refetch } = useActivityLogList({
    page,
    pageSize: DEFAULT_PAGINATION.PAGE_SIZE,
    actionType: actionType || undefined,
    from,
    to,
  });

  /** Reset tất cả bộ lọc */
  const handleClearFilters = () => {
    setActionType('');
    setFromDate('');
    setToDate('');
    setPage(1);
  };

  const hasActiveFilters = actionType || fromDate || toDate;

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Nhật ký hoạt động</h1>
        <p className="text-sm text-gray-500 mt-1">
          Theo dõi lịch sử thao tác của người dùng trên hệ thống
        </p>
      </div>

      {/* Error Banner */}
      {isError && <ConnectionErrorBanner error={error} onRetry={() => refetch()} />}

      {/* Filters */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4 mb-4">
        <div className="flex flex-wrap items-end gap-4">
          {/* Action Type Filter */}
          <div className="flex-1 min-w-[180px]">
            <label htmlFor="filter-action-type" className="block text-xs font-medium text-gray-500 mb-1">
              Loại hành động
            </label>
            <select
              id="filter-action-type"
              value={actionType}
              onChange={(e) => { setActionType(e.target.value); setPage(1); }}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-400 transition-colors"
            >
              {ACTION_TYPE_OPTIONS.map(opt => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
              ))}
            </select>
          </div>

          {/* Date From */}
          <div className="min-w-[160px]">
            <label htmlFor="filter-from" className="block text-xs font-medium text-gray-500 mb-1">
              Từ ngày
            </label>
            <input
              id="filter-from"
              type="date"
              value={fromDate}
              onChange={(e) => { setFromDate(e.target.value); setPage(1); }}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-400 transition-colors"
            />
          </div>

          {/* Date To */}
          <div className="min-w-[160px]">
            <label htmlFor="filter-to" className="block text-xs font-medium text-gray-500 mb-1">
              Đến ngày
            </label>
            <input
              id="filter-to"
              type="date"
              value={toDate}
              onChange={(e) => { setToDate(e.target.value); setPage(1); }}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-400 transition-colors"
            />
          </div>

          {/* Clear Filters */}
          {hasActiveFilters && (
            <button
              onClick={handleClearFilters}
              className="px-3 py-2 text-xs font-medium text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
            >
              ✕ Xóa bộ lọc
            </button>
          )}
        </div>
      </div>

      {/* Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <ActivityLogTable
          logs={logs}
          isLoading={isLoading}
          page={page}
          onPageChange={setPage}
        />
      </div>
    </div>
  );
}
