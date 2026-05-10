import { getStatusBadge } from '@/shared/utils/formatters';

/**
 * @param {{ rooms: Object, isLoading: boolean, page: number, onPageChange: Function, onEdit: Function, onToggleStatus: Function, isTogglingStatus: boolean }} props
 */
export function RoomTable({ rooms, isLoading, page, onPageChange, onEdit, onToggleStatus, isTogglingStatus }) {
  const items = rooms?.items || [];
  const totalPages = rooms?.totalPages || 1;

  if (isLoading) return <div className="space-y-3">{[1,2,3].map(i => <div key={i} className="h-12 bg-gray-100 rounded-lg animate-pulse" />)}</div>;
  if (items.length === 0) return <p className="text-sm text-gray-400 text-center py-8">Chưa có phòng học nào.</p>;

  return (
    <>
      <table className="w-full text-sm">
        <thead><tr className="text-left text-gray-500 border-b border-gray-100">
          <th className="pb-3 font-medium">Tên phòng</th>
          <th className="pb-3 font-medium text-center">Sức chứa</th>
          <th className="pb-3 font-medium text-center">Trạng thái</th>
          <th className="pb-3 font-medium text-right">Thao tác</th>
        </tr></thead>
        <tbody className="divide-y divide-gray-50">
          {items.map(room => {
            const badge = getStatusBadge(room.status);
            const isActive = room.status === 'active';
            return (
              <tr key={room.id} className="hover:bg-gray-50 transition-colors">
                <td className="py-3 font-medium text-gray-800">{room.name}</td>
                <td className="py-3 text-center text-gray-600">{room.capacity}</td>
                <td className="py-3 text-center"><span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>{badge.label}</span></td>
                <td className="py-3 text-right space-x-2">
                  <button onClick={() => onEdit(room)} className="text-indigo-600 hover:text-indigo-800 text-xs font-medium">✏ Sửa</button>
                  <button onClick={() => onToggleStatus(room)} disabled={isTogglingStatus} className={`text-xs font-medium ${isActive ? 'text-yellow-600 hover:text-yellow-800' : 'text-green-600 hover:text-green-800'}`}>
                    {isActive ? 'ǁ Tạm dừng' : '▶ Kích hoạt'}
                  </button>
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
