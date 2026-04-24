import { formatDateTime, getStatusBadge } from '@/shared/utils/formatters';

/**
 * SessionTable — Bảng danh sách buổi học.
 *
 * @param {{
 *   sessions: Object,
 *   isLoading: boolean,
 *   page: number,
 *   onPageChange: function,
 *   onEdit: function,
 *   onDelete: function,
 *   isDeleting: boolean
 * }} props
 */
export function SessionTable({
  sessions,
  isLoading,
  page,
  onPageChange,
  onEdit,
  onDelete,
  isDeleting,
}) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-14 bg-gray-100 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  const items = sessions?.items || [];
  const totalPages = sessions?.totalPages || 1;

  if (!items.length) {
    return (
      <div className="text-center py-10 text-gray-400">
        <p>Chưa có buổi học nào được lên lịch.</p>
      </div>
    );
  }

  return (
    <>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200">
              <th className="text-left py-3 px-4 font-medium text-gray-500">Thời gian</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Lớp</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Giáo viên</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Phòng</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Trạng thái</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {items.map((session) => {
              const badge = getStatusBadge(session.status);
              return (
                <tr key={session.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 whitespace-nowrap">
                    <div className="text-sm font-medium">{formatDateTime(session.startTime)}</div>
                    <div className="text-xs text-gray-400">→ {formatDateTime(session.endTime)}</div>
                  </td>
                  <td className="py-3 px-4 font-medium text-gray-800">{session.className}</td>
                  <td className="py-3 px-4 text-gray-600">{session.teacherName}</td>
                  <td className="py-3 px-4 text-gray-600">{session.roomName}</td>
                  <td className="py-3 px-4 text-center">
                    <span className={`text-xs font-medium px-2.5 py-0.5 rounded-full ${badge.className}`}>
                      {badge.label}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <div className="flex justify-center gap-1">
                      {session.status === 'scheduled' && (
                        <>
                          <button
                            onClick={() => onEdit(session)}
                            className="px-2 py-1 text-xs bg-blue-50 text-blue-700 rounded hover:bg-blue-100 transition-colors"
                          >
                            ✏️ Sửa
                          </button>
                          <button
                            onClick={() => onDelete(session.id)}
                            disabled={isDeleting}
                            className="px-2 py-1 text-xs bg-red-50 text-red-700 rounded hover:bg-red-100 transition-colors disabled:opacity-50"
                          >
                            🗑️ Xóa
                          </button>
                        </>
                      )}
                      {session.status !== 'scheduled' && (
                        <span className="text-xs text-gray-400">—</span>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-4">
          <button
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            ← Trước
          </button>
          <span className="text-sm text-gray-500">
            Trang {page} / {totalPages}
          </span>
          <button
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            Tiếp →
          </button>
        </div>
      )}
    </>
  );
}
