/**
 * Badge config cho từng loại action_type.
 * Màu sắc giúp Admin phân biệt nhanh loại hành động.
 */
const ACTION_BADGES = {
  create:        { label: 'Tạo mới',        className: 'bg-emerald-100 text-emerald-700', icon: '➕' },
  update:        { label: 'Cập nhật',       className: 'bg-blue-100 text-blue-700',       icon: '✏️' },
  delete:        { label: 'Xóa',            className: 'bg-red-100 text-red-700',         icon: '🗑️' },
  login:         { label: 'Đăng nhập',      className: 'bg-amber-100 text-amber-700',     icon: '🔑' },
  logout:        { label: 'Đăng xuất',      className: 'bg-orange-100 text-orange-700',   icon: '🚪' },
  status_change: { label: 'Đổi trạng thái', className: 'bg-purple-100 text-purple-700',   icon: '🔄' },
};

const DEFAULT_BADGE = { label: 'Khác', className: 'bg-gray-100 text-gray-600', icon: '📌' };

/**
 * Format thời gian dạng ngắn gọn: "11/05/2026 23:05"
 */
function formatDateTime(dateStr) {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
}

/**
 * ActivityLogTable — Bảng hiển thị nhật ký hoạt động với badge màu cho action type.
 *
 * @param {{ logs: Object, isLoading: boolean, page: number, onPageChange: Function }} props
 */
export function ActivityLogTable({ logs, isLoading, page, onPageChange }) {
  const items = logs?.items || [];
  const totalPages = logs?.totalPages || 1;
  const totalCount = logs?.totalCount || 0;

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3, 4, 5].map(i => (
          <div key={i} className="h-12 bg-gray-100 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <div className="text-center py-16">
        <div className="text-4xl mb-3">📋</div>
        <p className="text-sm text-gray-400">Chưa có nhật ký hoạt động nào.</p>
      </div>
    );
  }

  return (
    <>
      {/* Summary */}
      <div className="flex items-center justify-between mb-3">
        <p className="text-xs text-gray-400">
          Tổng cộng <span className="font-semibold text-gray-600">{totalCount}</span> bản ghi
        </p>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-gray-500 border-b border-gray-100">
              <th className="pb-3 font-medium w-44">Thời gian</th>
              <th className="pb-3 font-medium">Người dùng</th>
              <th className="pb-3 font-medium text-center w-40">Hành động</th>
              <th className="pb-3 font-medium">Mô tả</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-50">
            {items.map(log => {
              const badge = ACTION_BADGES[log.actionType] || DEFAULT_BADGE;
              return (
                <tr key={log.id} className="hover:bg-gray-50/60 transition-colors">
                  <td className="py-3 text-gray-500 text-xs tabular-nums">
                    {formatDateTime(log.createdAt)}
                  </td>
                  <td className="py-3">
                    <div className="flex items-center gap-2">
                      <div className="w-7 h-7 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 font-semibold text-xs flex-shrink-0">
                        {log.userFullName?.[0] ?? '?'}
                      </div>
                      <span className="font-medium text-gray-800 truncate max-w-[180px]">
                        {log.userFullName}
                      </span>
                    </div>
                  </td>
                  <td className="py-3 text-center">
                    <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>
                      <span>{badge.icon}</span>
                      {badge.label}
                    </span>
                  </td>
                  <td className="py-3 text-gray-600 text-xs max-w-xs truncate" title={log.description || ''}>
                    {log.description || '—'}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4 pt-3 border-t border-gray-100">
          <button
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            className="px-3 py-1.5 text-xs font-medium border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            ← Trước
          </button>
          <span className="px-3 py-1.5 text-xs text-gray-500">
            Trang {page} / {totalPages}
          </span>
          <button
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            className="px-3 py-1.5 text-xs font-medium border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            Tiếp →
          </button>
        </div>
      )}
    </>
  );
}
