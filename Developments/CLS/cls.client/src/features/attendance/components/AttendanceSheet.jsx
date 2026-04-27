import { useState } from 'react';

const ATTENDANCE_STATUS = {
  PRESENT: 'present',
  ABSENT: 'absent',
  LATE: 'late',
};

/**
 * AttendanceSheet — Danh sách học sinh + toggle Present/Absent/Late.
 * @param {Array} students - [{ studentId, studentName }]
 * @param {Array|null} existingRecords - Null nếu chưa điểm danh
 * @param {boolean} readOnly - True nếu đã điểm danh (chỉ view)
 * @param {Function} onSubmit - Callback khi submit records
 * @param {boolean} isSubmitting - Loading state
 */
export default function AttendanceSheet({
  students = [],
  existingRecords = null,
  readOnly = false,
  onSubmit,
  isSubmitting = false,
}) {
  // Initialize records: default absent nếu chưa điểm danh, map existing nếu có
  const [records, setRecords] = useState(() => {
    if (existingRecords) {
      return existingRecords.map((r) => ({
        studentId: r.studentId,
        status: r.status,
        note: r.note || '',
      }));
    }
    return students.map((s) => ({
      studentId: s.studentId,
      status: ATTENDANCE_STATUS.ABSENT,
      note: '',
    }));
  });

  const updateStatus = (studentId, status) => {
    setRecords((prev) =>
      prev.map((r) => (r.studentId === studentId ? { ...r, status } : r))
    );
  };

  const updateNote = (studentId, note) => {
    setRecords((prev) =>
      prev.map((r) => (r.studentId === studentId ? { ...r, note } : r))
    );
  };

  const handleSubmit = () => {
    onSubmit?.(records);
  };

  // Merge student names with records
  const studentMap = Object.fromEntries(
    students.map((s) => [s.studentId, s.studentName])
  );

  // Stats
  const stats = {
    present: records.filter((r) => r.status === ATTENDANCE_STATUS.PRESENT).length,
    absent: records.filter((r) => r.status === ATTENDANCE_STATUS.ABSENT).length,
    late: records.filter((r) => r.status === ATTENDANCE_STATUS.LATE).length,
  };

  return (
    <div>
      {/* Stats summary */}
      <div className="flex gap-4 mb-4">
        <div className="flex items-center gap-1.5 text-sm">
          <span className="w-3 h-3 rounded-full bg-green-500" />
          <span className="text-gray-600">Có mặt: <strong>{stats.present}</strong></span>
        </div>
        <div className="flex items-center gap-1.5 text-sm">
          <span className="w-3 h-3 rounded-full bg-red-500" />
          <span className="text-gray-600">Vắng: <strong>{stats.absent}</strong></span>
        </div>
        <div className="flex items-center gap-1.5 text-sm">
          <span className="w-3 h-3 rounded-full bg-yellow-500" />
          <span className="text-gray-600">Muộn: <strong>{stats.late}</strong></span>
        </div>
        <div className="ml-auto text-sm text-gray-400">
          Tổng: {records.length} học sinh
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto rounded-xl border border-gray-200">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-600 text-xs uppercase">
            <tr>
              <th className="px-4 py-3 text-left font-semibold w-10">#</th>
              <th className="px-4 py-3 text-left font-semibold">Họ và tên</th>
              <th className="px-4 py-3 text-center font-semibold">Có mặt</th>
              <th className="px-4 py-3 text-center font-semibold">Vắng</th>
              <th className="px-4 py-3 text-center font-semibold">Muộn</th>
              <th className="px-4 py-3 text-left font-semibold">Ghi chú</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {records.map((record, idx) => {
              const name = existingRecords
                ? existingRecords.find((r) => r.studentId === record.studentId)?.studentName
                : studentMap[record.studentId];

              return (
                <tr key={record.studentId} className="hover:bg-gray-50/50">
                  <td className="px-4 py-3 text-gray-400">{idx + 1}</td>
                  <td className="px-4 py-3 font-medium text-gray-800">{name || `#${record.studentId}`}</td>
                  {Object.values(ATTENDANCE_STATUS).map((status) => (
                    <td key={status} className="px-4 py-3 text-center">
                      <label className="inline-flex items-center cursor-pointer">
                        <input
                          type="radio"
                          name={`attendance-${record.studentId}`}
                          value={status}
                          checked={record.status === status}
                          onChange={() => updateStatus(record.studentId, status)}
                          disabled={readOnly}
                          className={`w-4 h-4 ${
                            status === ATTENDANCE_STATUS.PRESENT ? 'accent-green-600' :
                            status === ATTENDANCE_STATUS.ABSENT ? 'accent-red-600' : 'accent-yellow-600'
                          }`}
                        />
                      </label>
                    </td>
                  ))}
                  <td className="px-4 py-3">
                    <input
                      type="text"
                      value={record.note}
                      onChange={(e) => updateNote(record.studentId, e.target.value)}
                      placeholder="Ghi chú..."
                      disabled={readOnly}
                      className="w-full text-xs border border-gray-200 rounded-md px-2 py-1 focus:outline-none focus:ring-1 focus:ring-indigo-300 disabled:bg-gray-50"
                    />
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Submit button */}
      {!readOnly && (
        <div className="flex justify-end mt-4">
          <button
            onClick={handleSubmit}
            disabled={isSubmitting}
            className="px-6 py-2.5 bg-indigo-600 text-white rounded-lg font-medium text-sm hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            {isSubmitting ? 'Đang gửi...' : '✅ Xác nhận điểm danh'}
          </button>
        </div>
      )}
    </div>
  );
}
