import { useParams, useNavigate } from 'react-router-dom';
import { useFeedbackList } from '../hooks/useFeedback';
import { SlaTimer } from '../components/SlaTimer';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { formatDateTime } from '@/shared/utils/formatters';

/**
 * FeedbackListPage — UC-09: Danh sách học sinh + trạng thái đánh giá.
 * Route: /timetable/:sessionId/feedback
 */
export default function FeedbackListPage() {
  const { sessionId } = useParams();
  const navigate = useNavigate();

  const { data: sheet, isLoading, isError, error, refetch } = useFeedbackList(sessionId);

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
        <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
      </div>
    );
  }

  if (!sheet) return null;

  const completedCount = sheet.students.filter((s) => s.hasFeedback).length;
  const totalCount = sheet.students.length;
  const progress = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

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
          <div className="p-2.5 bg-purple-100 rounded-xl">
            <span className="text-xl">📝</span>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-800">Đánh giá học tập</h1>
            <p className="text-sm text-gray-500">
              {sheet.className} • {formatDateTime(sheet.startTime)}
            </p>
          </div>
        </div>
        <div className="ml-auto">
          <SlaTimer deadline={sheet.slaDeadline} />
        </div>
      </div>

      {/* SLA Warning Banner */}
      {sheet.isSlaExpired && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-3 text-red-700 text-sm">
          ⚠️ Đã quá hạn SLA (12 giờ sau khi buổi học kết thúc). Bạn vẫn có thể đánh giá nhưng sẽ bị đánh dấu trễ.
        </div>
      )}

      {/* Progress Bar */}
      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-gray-700">
            Tiến độ đánh giá
          </span>
          <span className="text-sm text-gray-500">
            {completedCount}/{totalCount} học sinh
          </span>
        </div>
        <div className="w-full bg-gray-200 rounded-full h-2.5">
          <div
            className="bg-indigo-600 h-2.5 rounded-full transition-all duration-300"
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>

      {/* Student Table */}
      <div className="overflow-x-auto rounded-xl border border-gray-200">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-600 text-xs uppercase">
            <tr>
              <th className="px-4 py-3 text-left font-semibold w-10">#</th>
              <th className="px-4 py-3 text-left font-semibold">Họ và tên</th>
              <th className="px-4 py-3 text-center font-semibold">Điểm</th>
              <th className="px-4 py-3 text-center font-semibold">Trạng thái</th>
              <th className="px-4 py-3 text-center font-semibold">Hành động</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {sheet.students.map((student, idx) => (
              <tr key={student.studentId} className="hover:bg-gray-50/50">
                <td className="px-4 py-3 text-gray-400">{idx + 1}</td>
                <td className="px-4 py-3 font-medium text-gray-800">
                  {student.studentName}
                </td>
                <td className="px-4 py-3 text-center">
                  {student.hasFeedback ? (
                    <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-indigo-100 text-indigo-700 font-bold text-sm">
                      {student.score}
                    </span>
                  ) : (
                    <span className="text-gray-300">—</span>
                  )}
                </td>
                <td className="px-4 py-3 text-center">
                  {student.hasFeedback ? (
                    <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full bg-green-50 text-green-700 text-xs font-medium">
                      ✅ Đã đánh giá
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full bg-gray-100 text-gray-500 text-xs font-medium">
                      ⏳ Chưa đánh giá
                    </span>
                  )}
                </td>
                <td className="px-4 py-3 text-center">
                  <button
                    onClick={() =>
                      navigate(
                        `/timetable/${sessionId}/feedback/${student.studentId}`
                      )
                    }
                    className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
                      student.hasFeedback
                        ? 'bg-amber-50 text-amber-700 hover:bg-amber-100 border border-amber-200'
                        : 'bg-indigo-600 text-white hover:bg-indigo-700'
                    }`}
                  >
                    {student.hasFeedback ? '✏️ Xem & Sửa' : '📝 Đánh giá'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
