import { useParams, useNavigate } from 'react-router-dom';
import { useAttendanceSheet, useSubmitAttendance } from '../hooks/useAttendance';
import AttendanceSheet from '../components/AttendanceSheet';
import { useToastStore } from '@/shared/stores/toastStore';
import { formatDateTime } from '@/shared/utils/formatters';

/**
 * AttendancePage — UC-08: Ghi nhận điểm danh cho 1 buổi học.
 * Route: /timetable/:sessionId/attendance
 */
export default function AttendancePage() {
  const { sessionId } = useParams();
  const navigate = useNavigate();
  const addToast = useToastStore((s) => s.addToast);

  const { data: sheet, isLoading, isError, error } = useAttendanceSheet(sessionId);
  const submitMutation = useSubmitAttendance();

  const handleSubmit = (records) => {
    submitMutation.mutate(
      { sessionId: Number(sessionId), records },
      {
        onSuccess: () => {
          const isEdit = sheet?.existingRecords?.length > 0;
          addToast({
            type: 'success',
            message: isEdit
              ? 'Cập nhật điểm danh thành công!'
              : 'Điểm danh thành công! Buổi học đã được hoàn thành.',
          });
          navigate('/timetable');
        },
        onError: (err) => {
          addToast({
            type: 'error',
            message: err.message || 'Có lỗi xảy ra khi điểm danh.',
          });
        },
      }
    );
  };

  if (isLoading) {
    return (
      <div className="flex justify-center items-center py-20">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-6">
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 text-red-700 text-sm">
          {error?.message || 'Không thể tải sheet điểm danh.'}
        </div>
      </div>
    );
  }

  if (!sheet) return null;

  const hasExisting = sheet.existingRecords && sheet.existingRecords.length > 0;

  // Cho phép sửa điểm danh nếu session là ngày hôm nay
  const isToday = new Date(sheet.startTime).toDateString() === new Date().toDateString();
  const readOnly = hasExisting && !isToday;

  return (
    <div className="p-6 space-y-6">
      {/* Back + Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/timetable')}
          className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5 text-gray-600">
            <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 19.5 3 12m0 0 7.5-7.5M3 12h18" />
          </svg>
        </button>
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-emerald-100 rounded-xl">
            <span className="text-xl">✅</span>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-800">
              {readOnly ? 'Kết quả điểm danh' : 'Điểm danh'}
            </h1>
            <p className="text-sm text-gray-500">
              {sheet.className} • {sheet.roomName}
            </p>
          </div>
        </div>
      </div>

      {/* Session Info Card */}
      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
          <div>
            <p className="text-gray-400 text-xs mb-1">Lớp</p>
            <p className="font-medium text-gray-800">{sheet.className}</p>
          </div>
          <div>
            <p className="text-gray-400 text-xs mb-1">Giáo viên</p>
            <p className="font-medium text-gray-800">{sheet.teacherName}</p>
          </div>
          <div>
            <p className="text-gray-400 text-xs mb-1">Thời gian</p>
            <p className="font-medium text-gray-800">
              {formatDateTime(sheet.startTime)} – {new Date(sheet.endTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
            </p>
          </div>
          <div>
            <p className="text-gray-400 text-xs mb-1">Phòng</p>
            <p className="font-medium text-gray-800">{sheet.roomName}</p>
          </div>
        </div>
      </div>

      {/* Read-only / editable notice */}
      {readOnly && (
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-3 text-blue-700 text-sm">
          ℹ️ Buổi học đã được điểm danh. Hiển thị ở chế độ chỉ đọc.
        </div>
      )}
      {hasExisting && isToday && !readOnly && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-3 text-amber-700 text-sm">
          ✏️ Buổi học đã được điểm danh. Bạn có thể chỉnh sửa và lưu lại trong ngày hôm nay.
        </div>
      )}

      {/* Attendance Sheet */}
      <AttendanceSheet
        students={sheet.students}
        existingRecords={sheet.existingRecords}
        readOnly={readOnly}
        onSubmit={handleSubmit}
        isSubmitting={submitMutation.isPending}
      />
    </div>
  );
}
