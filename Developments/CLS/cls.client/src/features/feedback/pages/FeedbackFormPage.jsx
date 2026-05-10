import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useStudentFeedback, useSubmitFeedback } from '../hooks/useFeedback';
import { SlaTimer } from '../components/SlaTimer';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { useToastStore } from '@/shared/stores/toastStore';
import { formatDateTime } from '@/shared/utils/formatters';

/**
 * FeedbackFormPage — UC-09: Form đánh giá cho 1 student.
 * Route: /timetable/:sessionId/feedback/:studentId
 */
export default function FeedbackFormPage() {
  const { sessionId, studentId } = useParams();
  const navigate = useNavigate();
  const addToast = useToastStore((s) => s.addToast);

  const { data: detail, isLoading, isError, error, refetch } = useStudentFeedback(sessionId, studentId);
  const submitMutation = useSubmitFeedback();

  const [score, setScore] = useState('');
  const [content, setContent] = useState('');

  // Pre-fill if existing feedback
  useEffect(() => {
    if (detail?.existingFeedback) {
      setScore(String(detail.existingFeedback.score ?? ''));
      setContent(detail.existingFeedback.content ?? '');
    }
  }, [detail]);

  const isExisting = !!detail?.existingFeedback;

  // Same-day edit check
  const isToday = detail?.existingFeedback
    ? new Date(detail.existingFeedback.submittedAt).toDateString() === new Date().toDateString()
    : true;
  const readOnly = isExisting && !isToday;

  const handleSubmit = () => {
    const scoreNum = parseInt(score, 10);
    if (isNaN(scoreNum) || scoreNum < 1 || scoreNum > 10) {
      addToast({ type: 'error', message: 'Điểm đánh giá phải từ 1 đến 10.' });
      return;
    }
    if (!content.trim()) {
      addToast({ type: 'error', message: 'Nội dung nhận xét không được để trống.' });
      return;
    }
    if (content.length > 1000) {
      addToast({ type: 'error', message: 'Nội dung nhận xét không quá 1000 ký tự.' });
      return;
    }

    submitMutation.mutate(
      {
        sessionId: Number(sessionId),
        data: {
          studentId: Number(studentId),
          score: scoreNum,
          content: content.trim(),
        },
      },
      {
        onSuccess: () => {
          addToast({
            type: 'success',
            message: isExisting
              ? 'Cập nhật đánh giá thành công!'
              : 'Gửi đánh giá thành công!',
          });
          navigate(`/timetable/${sessionId}/feedback`);
        },
        onError: (err) => {
          addToast({
            type: 'error',
            message: err.message || 'Có lỗi xảy ra khi gửi đánh giá.',
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
        <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
      </div>
    );
  }

  if (!detail) return null;

  return (
    <div className="p-6 space-y-6 max-w-2xl mx-auto">
      {/* Back + Header */}
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate(`/timetable/${sessionId}/feedback`)}
          className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2} stroke="currentColor" className="w-5 h-5 text-gray-600">
            <path strokeLinecap="round" strokeLinejoin="round" d="M10.5 19.5 3 12m0 0 7.5-7.5M3 12h18" />
          </svg>
        </button>
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-purple-100 rounded-xl">
            <span className="text-xl">✍️</span>
          </div>
          <div>
            <h1 className="text-2xl font-bold text-gray-800">
              {readOnly ? 'Xem đánh giá' : 'Đánh giá học sinh'}
            </h1>
            <p className="text-sm text-gray-500">
              {detail.studentName} • {detail.className}
            </p>
          </div>
        </div>
        <div className="ml-auto">
          <SlaTimer deadline={detail.slaDeadline} />
        </div>
      </div>

      {/* Session Info */}
      <div className="bg-white rounded-xl border border-gray-200 p-4">
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-gray-400 text-xs mb-1">Học sinh</p>
            <p className="font-medium text-gray-800">{detail.studentName}</p>
          </div>
          <div>
            <p className="text-gray-400 text-xs mb-1">Lớp</p>
            <p className="font-medium text-gray-800">{detail.className}</p>
          </div>
          <div>
            <p className="text-gray-400 text-xs mb-1">Thời gian buổi học</p>
            <p className="font-medium text-gray-800">
              {formatDateTime(detail.startTime)}
            </p>
          </div>
          <div>
            <p className="text-gray-400 text-xs mb-1">SLA Deadline</p>
            <p className="font-medium text-gray-800">
              {formatDateTime(detail.slaDeadline)}
            </p>
          </div>
        </div>
      </div>

      {/* SLA Warning */}
      {detail.isSlaExpired && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-3 text-red-700 text-sm">
          ⚠️ Đã quá hạn SLA. Đánh giá sẽ bị đánh dấu trễ (MSG-02).
        </div>
      )}

      {/* Read-only notice */}
      {readOnly && (
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-3 text-blue-700 text-sm">
          ℹ️ Đánh giá đã gửi trước đó. Hiển thị ở chế độ chỉ đọc.
        </div>
      )}
      {isExisting && isToday && !readOnly && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-3 text-amber-700 text-sm">
          ✏️ Bạn có thể chỉnh sửa đánh giá trong ngày hôm nay.
        </div>
      )}

      {/* Form */}
      <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        {/* Score */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Điểm đánh giá <span className="text-red-500">*</span>
            <span className="text-gray-400 font-normal ml-1">(1–10)</span>
          </label>
          <input
            type="number"
            min="1"
            max="10"
            value={score}
            onChange={(e) => setScore(e.target.value)}
            disabled={readOnly}
            placeholder="1"
            className="w-24 text-lg font-bold border border-gray-300 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 disabled:bg-gray-50 disabled:text-gray-500"
          />
        </div>

        {/* Content */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Nhận xét chi tiết <span className="text-red-500">*</span>
            <span className="text-gray-400 font-normal ml-1">
              ({content.length}/1000)
            </span>
          </label>
          <textarea
            rows={5}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            disabled={readOnly}
            maxLength={1000}
            placeholder="Nhập nhận xét về quá trình học tập, thái độ, và khuyến nghị cải thiện..."
            className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-300 disabled:bg-gray-50 disabled:text-gray-500 resize-none"
          />
        </div>
      </div>

      {/* Submit */}
      {!readOnly && (
        <div className="flex justify-end">
          <button
            onClick={handleSubmit}
            disabled={submitMutation.isPending}
            className="px-6 py-2.5 bg-indigo-600 text-white rounded-lg font-medium text-sm hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            {submitMutation.isPending
              ? 'Đang gửi...'
              : isExisting
                ? '💾 Cập nhật đánh giá'
                : '📝 Gửi đánh giá'}
          </button>
        </div>
      )}
    </div>
  );
}
