import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useStudentList, useUpdateStudentStatus } from '../hooks/useStudents';
import { StatusBadge } from './StatusBadge';
import { formatDate } from '@/shared/utils/formatters';
import { toast } from '@/shared/stores/toastStore';

/**
 * Bảng danh sách học sinh có phân trang và đổi trạng thái.
 * @param {{ onEdit: (student) => void }} props
 */
export const StudentList = ({ onEdit }) => {
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState('');

  const { data, isLoading, isError } = useStudentList({
    page,
    pageSize: 10,
    status: statusFilter || undefined,
  });

  const { mutate: updateStatus, isPending: isUpdatingStatus } = useUpdateStudentStatus();

  const students = data?.items ?? [];
  const totalPages = data?.totalPages ?? 1;

  const handleToggleStatus = (student) => {
    const newStatus = student.status === 'active' ? 'inactive' : 'active';
    updateStatus(
      { id: student.id, status: newStatus },
      {
        onSuccess: () => {
          toast.success(`Đã ${newStatus === 'active' ? 'kích hoạt' : 'cho nghỉ'} học sinh "${student.fullName}".`);
        },
        onError: (err) => {
          toast.error(err.message || 'Không thể đổi trạng thái. Vui lòng thử lại.');
        },
      }
    );
  };

  // ── Skeleton loading ──────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <div className="space-y-3 animate-pulse">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-14 rounded-lg bg-gray-100" />
        ))}
      </div>
    );
  }

  if (isError) {
    return (
      <div className="text-center py-12 text-red-500">
        <p className="text-lg font-medium">Có lỗi xảy ra khi tải dữ liệu.</p>
        <p className="text-sm mt-1">Vui lòng thử lại sau.</p>
      </div>
    );
  }

  return (
    <div>
      {/* ── Filter Bar ────────────────────────────────────────────────── */}
      <div className="flex items-center gap-3 mb-4">
        <select
          value={statusFilter}
          onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
          className="text-sm border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
        >
          <option value="">Tất cả trạng thái</option>
          <option value="active">Đang học</option>
          <option value="inactive">Đã nghỉ</option>
        </select>
        <span className="text-sm text-gray-500">
          {data?.totalCount ?? 0} học sinh
        </span>
      </div>

      {/* ── Table ─────────────────────────────────────────────────────── */}
      <div className="overflow-x-auto rounded-xl border border-gray-200 shadow-sm">
        <table className="w-full text-sm text-left">
          <thead className="bg-gray-50 text-gray-600 uppercase text-xs tracking-wider">
            <tr>
              <th className="px-4 py-3">Họ và tên</th>
              <th className="px-4 py-3">Phụ huynh</th>
              <th className="px-4 py-3">Email phụ huynh</th>
              <th className="px-4 py-3">Ngày nhập học</th>
              <th className="px-4 py-3">Trạng thái</th>
              <th className="px-4 py-3 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 bg-white">
            {students.length === 0 ? (
              <tr>
                <td colSpan={6} className="text-center py-12 text-gray-400">
                  Chưa có học sinh nào.
                </td>
              </tr>
            ) : (
              students.map((s) => (
                <tr key={s.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 font-medium text-gray-900">{s.fullName}</td>
                  <td className="px-4 py-3 text-gray-600">{s.parentFullName}</td>
                  <td className="px-4 py-3 text-gray-600">{s.parentEmail}</td>
                  <td className="px-4 py-3 text-gray-500">{formatDate(s.enrolledAt)}</td>
                  <td className="px-4 py-3">
                    <StatusBadge status={s.status} />
                  </td>
                  <td className="px-4 py-3 text-right">
                    <div className="flex justify-end gap-2">
                      <Link
                        to={`/students/${s.id}/financials`}
                        className="text-xs px-3 py-1.5 rounded-lg text-blue-600 border border-blue-200 hover:bg-blue-50 transition-colors"
                      >
                        Tài chính
                      </Link>
                      <button
                        onClick={() => onEdit(s)}
                        className="text-xs px-3 py-1.5 rounded-lg text-indigo-600 border border-indigo-200 hover:bg-indigo-50 transition-colors"
                      >
                        Sửa
                      </button>
                      <button
                        onClick={() => handleToggleStatus(s)}
                        disabled={isUpdatingStatus}
                        className={`text-xs px-3 py-1.5 rounded-lg border transition-colors disabled:opacity-50 ${
                          s.status === 'active'
                            ? 'text-red-600 border-red-200 hover:bg-red-50'
                            : 'text-emerald-600 border-emerald-200 hover:bg-emerald-50'
                        }`}
                      >
                        {s.status === 'active' ? 'Cho nghỉ' : 'Kích hoạt'}
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* ── Pagination ───────────────────────────────────────────────── */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-4">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="px-3 py-1.5 text-sm rounded-lg border border-gray-300 disabled:opacity-40 hover:bg-gray-50"
          >
            ← Trước
          </button>
          <span className="text-sm text-gray-600">
            Trang {page} / {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="px-3 py-1.5 text-sm rounded-lg border border-gray-300 disabled:opacity-40 hover:bg-gray-50"
          >
            Sau →
          </button>
        </div>
      )}
    </div>
  );
};
