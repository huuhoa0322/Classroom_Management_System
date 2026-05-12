import { getStatusBadge } from '@/shared/utils/formatters';

/**
 * Bảng hiển thị danh sách tài khoản (Admin/Teacher).
 * @param {{ users: Object, isLoading: boolean, page: number, onPageChange: Function, onEdit: Function, onToggleStatus: Function, onResetPassword: Function, onToggleLock: Function, isTogglingStatus: boolean }} props
 */
export function UserTable({ users, isLoading, page, onPageChange, onEdit, onToggleStatus, onResetPassword, onToggleLock, isTogglingStatus }) {
  const items = users?.items || [];
  const totalPages = users?.totalPages || 1;

  if (isLoading) return <div className="space-y-3">{[1,2,3].map(i => <div key={i} className="h-12 bg-gray-100 rounded-lg animate-pulse" />)}</div>;
  if (items.length === 0) return <p className="text-sm text-gray-400 text-center py-8">Chưa có tài khoản nào.</p>;

  const roleBadge = (role) => role === 'Admin'
    ? 'bg-purple-100 text-purple-700'
    : 'bg-blue-100 text-blue-700';

  return (
    <>
      <table className="w-full text-sm">
        <thead><tr className="text-left text-gray-500 border-b border-gray-100">
          <th className="pb-3 font-medium">Họ tên</th>
          <th className="pb-3 font-medium">Email</th>
          <th className="pb-3 font-medium">SĐT</th>
          <th className="pb-3 font-medium text-center">Vai trò</th>
          <th className="pb-3 font-medium text-center">Trạng thái</th>
          <th className="pb-3 font-medium text-right">Thao tác</th>
        </tr></thead>
        <tbody className="divide-y divide-gray-50">
          {items.map(user => {
            const badge = getStatusBadge(user.status);
            const isActive = user.status === 'active';
            const isAdmin = user.role === 'Admin';
            return (
              <tr key={user.id} className="hover:bg-gray-50 transition-colors">
                <td className="py-3 font-medium text-gray-800">{user.fullName}</td>
                <td className="py-3 text-gray-600">{user.email}</td>
                <td className="py-3 text-gray-600">{user.phone || '—'}</td>
                <td className="py-3 text-center"><span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-medium ${roleBadge(user.role)}`}>{user.role}</span></td>
                <td className="py-3 text-center">
                  {user.isLocked
                    ? <span className="inline-block px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-700">🔒 Đã khóa</span>
                    : <span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>{badge.label}</span>
                  }
                </td>
                <td className="py-3 text-right space-x-2">
                  {!isAdmin && (
                    <>
                      <button onClick={() => onEdit(user)} className="text-indigo-600 hover:text-indigo-800 text-xs font-medium">✏ Sửa</button>
                      <button onClick={() => onResetPassword(user)} className="text-amber-600 hover:text-amber-800 text-xs font-medium">🔑 Đặt lại MK</button>
                      <button onClick={() => onToggleStatus(user)} disabled={isTogglingStatus} className={`text-xs font-medium ${isActive ? 'text-yellow-600 hover:text-yellow-800' : 'text-green-600 hover:text-green-800'}`}>
                        {isActive ? 'ǁ Tạm dừng' : '▶ Kích hoạt'}
                      </button>
                      <button onClick={() => onToggleLock(user)} disabled={isTogglingStatus} className={`text-xs font-medium ${user.isLocked ? 'text-green-600 hover:text-green-800' : 'text-red-600 hover:text-red-800'}`}>
                        {user.isLocked ? '🔓 Mở khóa' : '🔒 Khóa'}
                      </button>
                    </>
                  )}
                  {isAdmin && <span className="text-xs text-gray-400">—</span>}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-4">
          <button onClick={() => onPageChange(page - 1)} disabled={page <= 1} className="px-3 py-1 text-xs border rounded disabled:opacity-40">Trước</button>
          <span className="px-3 py-1 text-xs text-gray-500">Trang {page} / {totalPages}</span>
          <button onClick={() => onPageChange(page + 1)} disabled={page >= totalPages} className="px-3 py-1 text-xs border rounded disabled:opacity-40">Tiếp</button>
        </div>
      )}
    </>
  );
}

