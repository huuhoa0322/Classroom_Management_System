import { useNavigate } from 'react-router-dom';
import { SESSION_STATUS } from '@/shared/utils/constants';

const statusConfig = {
  [SESSION_STATUS.SCHEDULED]: {
    label: 'Chưa bắt đầu',
    bg: 'bg-blue-50 border-blue-200',
    badge: 'bg-blue-100 text-blue-700',
  },
  [SESSION_STATUS.IN_PROGRESS]: {
    label: 'Đang diễn ra',
    bg: 'bg-yellow-50 border-yellow-200',
    badge: 'bg-yellow-100 text-yellow-700',
  },
  [SESSION_STATUS.COMPLETED]: {
    label: 'Đã hoàn thành',
    bg: 'bg-green-50 border-green-200',
    badge: 'bg-green-100 text-green-700',
  },
  [SESSION_STATUS.CANCELLED]: {
    label: 'Đã hủy',
    bg: 'bg-red-50 border-red-200',
    badge: 'bg-red-100 text-red-600',
  },
};

/**
 * SessionCard — Card 1 buổi học trong timetable.
 * Hiển thị: giờ, lớp, phòng, trạng thái, nút "Điểm danh".
 * @param {boolean} isFuture - Nếu true, card ở trạng thái mờ + không thao tác được
 */
export default function SessionCard({ session, isFuture = false }) {
  const navigate = useNavigate();
  const config = statusConfig[session.status] || statusConfig[SESSION_STATUS.SCHEDULED];

  const timeStr = (iso) =>
    new Date(iso).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

  const canAttend = !isFuture
    && (session.status === SESSION_STATUS.SCHEDULED
      || session.status === SESSION_STATUS.IN_PROGRESS);

  return (
    <div className={`rounded-lg border p-3 ${config.bg} transition-shadow ${
      isFuture ? 'pointer-events-none select-none' : 'hover:shadow-md'
    }`}>
      <div className="flex items-start justify-between mb-2">
        <div>
          <p className="font-semibold text-gray-800 text-sm">{session.className}</p>
          <p className="text-xs text-gray-500">
            {timeStr(session.startTime)} – {timeStr(session.endTime)}
          </p>
        </div>
        <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${config.badge}`}>
          {config.label}
        </span>
      </div>

      <p className="text-xs text-gray-500 mb-2">
        📍 {session.roomName}
      </p>

      {isFuture && (
        <div className="w-full text-xs font-medium text-gray-400 bg-gray-100 border border-gray-200 rounded-md py-1.5 text-center cursor-not-allowed">
          🔒 Chưa đến lịch
        </div>
      )}

      {canAttend && (
        <button
          onClick={() => navigate(`/timetable/${session.id}/attendance`)}
          className="w-full text-xs font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md py-1.5 transition-colors"
        >
          ✅ Điểm danh
        </button>
      )}

      {!isFuture && session.status === SESSION_STATUS.COMPLETED && (
        <div className="space-y-1.5">
          <button
            onClick={() => navigate(`/timetable/${session.id}/attendance`)}
            className="w-full text-xs font-medium text-indigo-600 bg-white border border-indigo-200 hover:bg-indigo-50 rounded-md py-1.5 transition-colors"
          >
            📋 Xem điểm danh
          </button>
          <button
            onClick={() => navigate(`/timetable/${session.id}/feedback`)}
            className="w-full text-xs font-medium text-purple-600 bg-white border border-purple-200 hover:bg-purple-50 rounded-md py-1.5 transition-colors"
          >
            📝 Đánh giá
          </button>
        </div>
      )}
    </div>
  );
}

