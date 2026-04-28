import { getStatusBadge } from '@/shared/utils/formatters';

/**
 * ClassTable — Bảng danh sách lớp học.
 *
 * @param {{
 *   classes: { items: Array, totalPages: number, pageSize: number },
 *   isLoading: boolean,
 *   page: number,
 *   onPageChange: (page: number) => void,
 *   onEdit: (cls: Object) => void,
 *   onManageStudents: (cls: Object) => void,
 *   onToggleStatus: (cls: Object) => void,
 *   isTogglingStatus: boolean
 * }} props
 */
export function ClassTable({
  classes,
  isLoading,
  page,
  onPageChange,
  onEdit,
  onManageStudents,
  onToggleStatus,
  isTogglingStatus,
}) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-14 bg-gray-100 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  const items = classes?.items || [];
  const totalPages = classes?.totalPages || 1;

  if (!items.length) {
    return (
      <div className="text-center py-16 text-gray-400">
        <div className="text-4xl mb-3">🏫</div>
        <p className="font-medium">Chưa có lớp học nào.</p>
        <p className="text-sm mt-1">Nhấn &quot;Tạo lớp học&quot; để bắt đầu.</p>
      </div>
    );
  }

  return (
    <>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200">
              <th className="text-left py-3 px-4 font-medium text-gray-500 w-12">STT</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Tên lớp</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Học sinh</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Buổi học</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Trạng thái</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {items.map((cls, idx) => {
              const badge = getStatusBadge(cls.status);
              const rowNum = (page - 1) * (classes?.pageSize || 10) + idx + 1;
              return (
                <tr key={cls.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 text-gray-400">{rowNum}</td>
                  <td className="py-3 px-4 font-medium text-gray-800">{cls.name}</td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-flex items-center gap-1 text-blue-600 font-medium">
                      👨‍🎓 {cls.studentCount}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-flex items-center gap-1 text-indigo-600 font-medium">
                      📅 {cls.sessionCount}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className={`text-xs font-medium px-2.5 py-0.5 rounded-full ${badge.className}`}>
                      {badge.label}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex justify-center gap-1">
                      <button
                        onClick={() => onEdit(cls)}
                        className="px-2 py-1 text-xs bg-blue-50 text-blue-700 rounded hover:bg-blue-100 transition-colors"
                      >
                        ✏️ Sửa
                      </button>
                      <button
                        onClick={() => onManageStudents(cls)}
                        className="px-2 py-1 text-xs bg-purple-50 text-purple-700 rounded hover:bg-purple-100 transition-colors"
                      >
                        👥 Học sinh
                      </button>
                      <button
                        onClick={() => onToggleStatus(cls)}
                        disabled={isTogglingStatus}
                        className={`px-2 py-1 text-xs rounded transition-colors disabled:opacity-50 ${
                          cls.status === 'active'
                            ? 'bg-yellow-50 text-yellow-700 hover:bg-yellow-100'
                            : 'bg-green-50 text-green-700 hover:bg-green-100'
                        }`}
                      >
                        {cls.status === 'active' ? '⏸ Tạm dừng' : '▶ Kích hoạt'}
                      </button>
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
