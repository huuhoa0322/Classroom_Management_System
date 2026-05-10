import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { sessionSchema } from '../schemas/session.schema';
import { useClasses, useRooms, useTeachers } from '../hooks/useSession';
import { formatDateTime } from '@/shared/utils/formatters';

/**
 * SessionForm — Form tạo/sửa buổi học (CLS-004).
 *
 * @param {{
 *   onSubmit: function,
 *   onCancel: function,
 *   isSubmitting: boolean,
 *   defaultValues?: object,
 *   apiError?: string,
 *   apiErrorData?: object
 * }} props
 */
export function SessionForm({ onSubmit, onCancel, isSubmitting, defaultValues, apiError, apiErrorData }) {
  const { data: classes, isLoading: loadingClasses } = useClasses();
  const { data: rooms, isLoading: loadingRooms } = useRooms();
  const { data: teachers, isLoading: loadingTeachers } = useTeachers();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(sessionSchema),
    defaultValues: defaultValues || {
      classId: '',
      teacherId: '',
      roomId: '',
      startTime: '',
      endTime: '',
    },
  });

  const processSubmit = (data) => {
    onSubmit({
      classId: Number(data.classId),
      teacherId: Number(data.teacherId),
      roomId: Number(data.roomId),
      startTime: new Date(data.startTime).toISOString(),
      endTime: new Date(data.endTime).toISOString(),
    });
  };

  const isDropdownLoading = loadingClasses || loadingRooms || loadingTeachers;

  return (
    <form onSubmit={handleSubmit(processSubmit)} className="space-y-5">
      {/* Lớp học */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Lớp học <span className="text-red-500">*</span>
        </label>
        {isDropdownLoading ? (
          <div className="h-10 bg-gray-100 rounded animate-pulse" />
        ) : (
          <select
            {...register('classId')}
            className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
          >
            <option value="">— Chọn lớp —</option>
            {Array.isArray(classes) && classes.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        )}
        {errors.classId && (
          <p className="text-red-500 text-xs mt-1">{errors.classId.message}</p>
        )}
      </div>

      {/* Giáo viên */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Giáo viên <span className="text-red-500">*</span>
        </label>
        {isDropdownLoading ? (
          <div className="h-10 bg-gray-100 rounded animate-pulse" />
        ) : (
          <select
            {...register('teacherId')}
            className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
          >
            <option value="">— Chọn giáo viên —</option>
            {Array.isArray(teachers) && teachers.map((t) => (
              <option key={t.id} value={t.id}>
                {t.fullName}
              </option>
            ))}
          </select>
        )}
        {errors.teacherId && (
          <p className="text-red-500 text-xs mt-1">{errors.teacherId.message}</p>
        )}
      </div>

      {/* Phòng học */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Phòng học <span className="text-red-500">*</span>
        </label>
        {isDropdownLoading ? (
          <div className="h-10 bg-gray-100 rounded animate-pulse" />
        ) : (
          <select
            {...register('roomId')}
            className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
          >
            <option value="">— Chọn phòng —</option>
            {Array.isArray(rooms) && rooms.map((r) => (
              <option key={r.id} value={r.id}>
                {r.name} (Sức chứa: {r.capacity})
              </option>
            ))}
          </select>
        )}
        {errors.roomId && (
          <p className="text-red-500 text-xs mt-1">{errors.roomId.message}</p>
        )}
      </div>

      {/* Thời gian bắt đầu */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Thời gian bắt đầu <span className="text-red-500">*</span>
        </label>
        <input
          type="datetime-local"
          {...register('startTime')}
          className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
        />
        {errors.startTime && (
          <p className="text-red-500 text-xs mt-1">{errors.startTime.message}</p>
        )}
      </div>

      {/* Thời gian kết thúc */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Thời gian kết thúc <span className="text-red-500">*</span>
        </label>
        <input
          type="datetime-local"
          {...register('endTime')}
          className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
        />
        {errors.endTime && (
          <p className="text-red-500 text-xs mt-1">{errors.endTime.message}</p>
        )}
      </div>

      {/* API Error (409 Conflict) */}
      {apiError && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
          <div className="flex items-start gap-2">
            <svg className="w-5 h-5 flex-shrink-0 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
            <div className="min-w-0">
              <p>{apiError}</p>
              {apiErrorData?.conflictingSession && (
                <div className="mt-2 grid grid-cols-1 gap-1 text-xs text-red-800">
                  <p>Lớp: {apiErrorData.conflictingSession.className || `#${apiErrorData.conflictingSession.classId}`}</p>
                  <p>Giáo viên: {apiErrorData.conflictingSession.teacherName || `#${apiErrorData.conflictingSession.teacherId}`}</p>
                  <p>Phòng: {apiErrorData.conflictingSession.roomName || `#${apiErrorData.conflictingSession.roomId}`}</p>
                  <p>
                    Thời gian: {formatDateTime(apiErrorData.conflictingSession.startTime)}
                    {' - '}
                    {formatDateTime(apiErrorData.conflictingSession.endTime)}
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex justify-end gap-3 pt-2">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 text-sm text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {isSubmitting ? 'Đang xử lý...' : defaultValues ? 'Cập nhật' : 'Tạo buổi học'}
        </button>
      </div>
    </form>
  );
}
