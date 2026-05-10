import { getStatusBadge, formatCurrency } from '@/shared/utils/formatters';

export function PackageTable({ packages, isLoading, page, onPageChange, onEdit, onToggleStatus, isTogglingStatus }) {
  const items = packages?.items || [];
  const totalPages = packages?.totalPages || 1;

  if (isLoading) return <div className="space-y-3">{[1,2,3].map(i => <div key={i} className="h-12 bg-gray-100 rounded-lg animate-pulse" />)}</div>;
  if (items.length === 0) return <p className="text-sm text-gray-400 text-center py-8">Chưa có gói học nào.</p>;

  return (
    <>
      <table className="w-full text-sm">
        <thead><tr className="text-left text-gray-500 border-b border-gray-100">
          <th className="pb-3 font-medium">Tên gói</th>
          <th className="pb-3 font-medium text-center">Số buổi</th>
          <th className="pb-3 font-medium text-center">Thời hạn</th>
          <th className="pb-3 font-medium text-right">Giá</th>
          <th className="pb-3 font-medium text-center">HS đang dùng</th>
          <th className="pb-3 font-medium text-center">Trạng thái</th>
          <th className="pb-3 font-medium text-right">Thao tác</th>
        </tr></thead>
        <tbody className="divide-y divide-gray-50">
          {items.map(pkg => {
            const badge = getStatusBadge(pkg.status);
            const isActive = pkg.status === 'active';
            return (
              <tr key={pkg.id} className="hover:bg-gray-50 transition-colors">
                <td className="py-3 font-medium text-gray-800">{pkg.name}</td>
                <td className="py-3 text-center text-gray-600">{pkg.totalSessions}</td>
                <td className="py-3 text-center text-gray-600">{pkg.durationDays} ngày</td>
                <td className="py-3 text-right font-medium text-gray-800">{formatCurrency(pkg.price)}</td>
                <td className="py-3 text-center text-gray-600">{pkg.studentCount}</td>
                <td className="py-3 text-center"><span className={`inline-block px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>{badge.label}</span></td>
                <td className="py-3 text-right space-x-2">
                  <button onClick={() => onEdit(pkg)} className="text-indigo-600 hover:text-indigo-800 text-xs font-medium">✏ Sửa</button>
                  <button onClick={() => onToggleStatus(pkg)} disabled={isTogglingStatus} className={`text-xs font-medium ${isActive ? 'text-yellow-600 hover:text-yellow-800' : 'text-green-600 hover:text-green-800'}`}>
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
