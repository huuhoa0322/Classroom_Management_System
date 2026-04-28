import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createStudentSchema, updateStudentSchema } from '../schemas/student.schema';

/**
 * Form tạo mới / cập nhật học sinh — dùng chung bằng prop `student`.
 *
 * @param {{ student?: object, onSubmit: (data) => void, isLoading: boolean, onCancel: () => void }} props
 */
export const StudentForm = ({ student, onSubmit, isLoading, onCancel }) => {
  const isEdit = !!student;
  const schema = isEdit ? updateStudentSchema : createStudentSchema;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues: isEdit
      ? {
          fullName: student.fullName,
          dateOfBirth: student.dateOfBirth ?? '',
          parentFullName: student.parentFullName ?? '',
          parentEmail: student.parentEmail ?? '',
          parentPhone: student.parentPhone ?? '',
          parentRelationship: student.parentRelationship ?? '',
        }
      : {},
  });

  useEffect(() => {
    if (student) {
      reset({
        fullName: student.fullName,
        dateOfBirth: student.dateOfBirth ?? '',
        parentFullName: student.parentFullName ?? '',
        parentEmail: student.parentEmail ?? '',
        parentPhone: student.parentPhone ?? '',
        parentRelationship: student.parentRelationship ?? '',
      });
    }
  }, [student, reset]);

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      {/* ── Thông tin học sinh ─────────────────────────────────────────── */}
      <div>
        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-3">
          Thông tin học sinh
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Họ và tên <span className="text-red-500">*</span>
            </label>
            <input
              {...register('fullName')}
              className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${
                errors.fullName ? 'border-red-400' : 'border-gray-300'
              }`}
              placeholder="Nguyễn Văn A"
            />
            {errors.fullName && (
              <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Ngày sinh</label>
            <input
              type="date"
              {...register('dateOfBirth')}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            {errors.dateOfBirth && (
              <p className="mt-1 text-xs text-red-600">{errors.dateOfBirth.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* ── Thông tin phụ huynh ─────────────────────────────────────────── */}
      <div>
        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-3">
          Thông tin phụ huynh
        </h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Họ và tên phụ huynh <span className="text-red-500">*</span>
            </label>
            <input
              {...register('parentFullName')}
              className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${
                errors.parentFullName ? 'border-red-400' : 'border-gray-300'
              }`}
              placeholder="Nguyễn Văn B"
            />
            {errors.parentFullName && (
              <p className="mt-1 text-xs text-red-600">{errors.parentFullName.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Email phụ huynh <span className="text-red-500">*</span>
            </label>
            <input
              {...register('parentEmail')}
              type="email"
              className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${
                errors.parentEmail ? 'border-red-400' : 'border-gray-300'
              }`}
              placeholder="phu.huynh@email.com"
            />
            {errors.parentEmail && (
              <p className="mt-1 text-xs text-red-600">{errors.parentEmail.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Số điện thoại</label>
            <input
              {...register('parentPhone')}
              className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${
                errors.parentPhone ? 'border-red-400' : 'border-gray-300'
              }`}
              placeholder="0901234567"
            />
            {errors.parentPhone && (
              <p className="mt-1 text-xs text-red-600">{errors.parentPhone.message}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Phụ huynh là gì của học sinh? <span className="text-red-500">*</span>
            </label>
            <select
              {...register('parentRelationship')}
              className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 ${
                errors.parentRelationship ? 'border-red-400' : 'border-gray-300'
              }`}
            >
              <option value="">-- Chọn --</option>
              <option value="Bố">Bố</option>
              <option value="Mẹ">Mẹ</option>
              <option value="Người giám hộ">Người giám hộ</option>
            </select>
            {errors.parentRelationship && (
              <p className="mt-1 text-xs text-red-600">{errors.parentRelationship.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* ── Actions ───────────────────────────────────────────────────────── */}
      <div className="flex justify-end gap-3 pt-2 border-t border-gray-100">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm font-medium text-gray-600 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {isLoading ? 'Đang lưu...' : isEdit ? 'Cập nhật' : 'Tạo học sinh'}
        </button>
      </div>
    </form>
  );
};
