import { useState } from 'react';
import { useClassStudents, useEnrollStudents, useUnenrollStudent } from '../hooks/useClass';
import { useStudentList } from '@/features/student/hooks/useStudents';
import { toast } from '@/shared/stores/toastStore';

/**
 * ClassStudentManager — Modal quản lý học sinh trong lớp.
 * Hỗ trợ đăng ký nhiều HS cùng lúc + hủy đăng ký từng HS.
 *
 * @param {{
 *   classInfo: { id: number, name: string },
 *   onClose: () => void
 * }} props
 */
export function ClassStudentManager({ classInfo, onClose }) {
  const classId = classInfo?.id;

  // Danh sách HS trong lớp
  const { data: classStudents, isLoading: loadingStudents } = useClassStudents(classId);

  // Danh sách tất cả HS active (để chọn thêm)
  const { data: allStudents } = useStudentList({ page: 1, pageSize: 200, status: 'active' });

  // Mutations
  const enrollMutation = useEnrollStudents();
  const unenrollMutation = useUnenrollStudent();

  // State: HS đang được chọn để thêm (multi-select)
  const [selectedIds, setSelectedIds] = useState([]);

  // Lọc HS chưa có trong lớp
  const enrolledStudentIds = new Set((classStudents || []).map((cs) => cs.studentId));
  const availableStudents = (allStudents?.items || []).filter(
    (s) => !enrolledStudentIds.has(s.id)
  );

  const toggleSelect = (studentId) => {
    setSelectedIds((prev) =>
      prev.includes(studentId) ? prev.filter((id) => id !== studentId) : [...prev, studentId]
    );
  };

  const handleSelectAll = () => {
    if (selectedIds.length === availableStudents.length) {
      setSelectedIds([]);
    } else {
      setSelectedIds(availableStudents.map((s) => s.id));
    }
  };

  const handleEnroll = () => {
    if (selectedIds.length === 0) return;
    enrollMutation.mutate(
      { classId, studentIds: selectedIds },
      {
        onSuccess: () => {
          setSelectedIds([]);
          toast.success(`Đã đăng ký ${selectedIds.length} học sinh vào lớp!`);
        },
        onError: (err) => {
          toast.error(err.message || 'Không thể đăng ký học sinh.');
        },
      }
    );
  };

  const handleUnenroll = (studentId, studentName) => {
    if (!window.confirm(`Hủy đăng ký "${studentName}" khỏi lớp?`)) return;
    unenrollMutation.mutate(
      { classId, studentId },
      {
        onSuccess: () => toast.success(`Đã hủy đăng ký "${studentName}".`),
        onError: (err) => toast.error(err.message || 'Không thể hủy đăng ký.'),
      }
    );
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl mx-4 max-h-[90vh] flex flex-col overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-6 pt-5 pb-4 border-b border-gray-100">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">
              Quản lý học sinh — {classInfo?.name}
            </h2>
            <p className="text-sm text-gray-500 mt-0.5">
              {(classStudents || []).length} học sinh đang theo học
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-2xl leading-none"
          >
            ×
          </button>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-4 space-y-6">
          {/* ── Section 1: Thêm học sinh ──────────────────────────────── */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
              ➕ Thêm học sinh vào lớp
            </h3>

            {availableStudents.length === 0 ? (
              <p className="text-sm text-gray-400 italic">
                Tất cả học sinh đã được đăng ký vào lớp này.
              </p>
            ) : (
              <>
                {/* Select all + action bar */}
                <div className="flex items-center justify-between mb-2">
                  <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={selectedIds.length === availableStudents.length && availableStudents.length > 0}
                      onChange={handleSelectAll}
                      className="w-4 h-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                    />
                    Chọn tất cả ({availableStudents.length})
                  </label>
                  {selectedIds.length > 0 && (
                    <button
                      onClick={handleEnroll}
                      disabled={enrollMutation.isPending}
                      className="px-3 py-1.5 text-xs font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors flex items-center gap-1"
                    >
                      {enrollMutation.isPending ? (
                        <>
                          <span className="w-3 h-3 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                          Đang thêm...
                        </>
                      ) : (
                        <>✓ Thêm {selectedIds.length} học sinh</>
                      )}
                    </button>
                  )}
                </div>

                {/* Student checkbox list */}
                <div className="max-h-48 overflow-y-auto border border-gray-200 rounded-lg divide-y divide-gray-100">
                  {availableStudents.map((student) => (
                    <label
                      key={student.id}
                      className="flex items-center gap-3 px-3 py-2.5 hover:bg-gray-50 cursor-pointer transition-colors"
                    >
                      <input
                        type="checkbox"
                        checked={selectedIds.includes(student.id)}
                        onChange={() => toggleSelect(student.id)}
                        className="w-4 h-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                      />
                      <span className="text-sm text-gray-800">{student.fullName}</span>
                    </label>
                  ))}
                </div>
              </>
            )}
          </div>

          {/* ── Section 2: Học sinh trong lớp ────────────────────────── */}
          <div>
            <h3 className="text-sm font-semibold text-gray-700 mb-3 flex items-center gap-2">
              👥 Học sinh trong lớp
            </h3>

            {loadingStudents ? (
              <div className="space-y-2">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="h-10 bg-gray-100 rounded-lg animate-pulse" />
                ))}
              </div>
            ) : (classStudents || []).length === 0 ? (
              <p className="text-sm text-gray-400 italic">Chưa có học sinh nào trong lớp.</p>
            ) : (
              <div className="border border-gray-200 rounded-lg divide-y divide-gray-100">
                {(classStudents || []).map((cs) => (
                  <div
                    key={cs.id}
                    className="flex items-center justify-between px-3 py-2.5 hover:bg-gray-50"
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 font-semibold text-xs">
                        {cs.studentName?.[0] ?? '?'}
                      </div>
                      <div>
                        <div className="text-sm font-medium text-gray-800">{cs.studentName}</div>
                        <div className="text-xs text-gray-400">Ngày ĐK: {cs.enrollmentDate}</div>
                      </div>
                    </div>
                    <button
                      onClick={() => handleUnenroll(cs.studentId, cs.studentName)}
                      disabled={unenrollMutation.isPending}
                      className="px-2 py-1 text-xs text-red-600 bg-red-50 rounded hover:bg-red-100 transition-colors disabled:opacity-50"
                    >
                      ✕ Hủy
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-gray-100">
          <button
            onClick={onClose}
            className="w-full px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
          >
            Đóng
          </button>
        </div>
      </div>
    </div>
  );
}
