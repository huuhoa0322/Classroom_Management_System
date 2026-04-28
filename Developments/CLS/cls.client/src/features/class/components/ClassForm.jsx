import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { classSchema } from '../schemas/classSchema';

/**
 * ClassForm — Form tạo/sửa lớp học (modal content).
 *
 * @param {{
 *   onSubmit: (data: { name: string }) => void,
 *   onCancel: () => void,
 *   isSubmitting: boolean,
 *   defaultValues?: { name: string }
 * }} props
 */
export function ClassForm({ onSubmit, onCancel, isSubmitting, defaultValues }) {
  const isEdit = !!defaultValues;

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm({
    resolver: zodResolver(classSchema),
    defaultValues: defaultValues || { name: '' },
    mode: 'onChange',
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div>
        <label htmlFor="class-name" className="block text-sm font-medium text-gray-700 mb-1.5">
          Tên lớp <span className="text-red-500">*</span>
        </label>
        <input
          id="class-name"
          type="text"
          {...register('name')}
          autoFocus
          placeholder="VD: Toán Nâng Cao - K12"
          className={`w-full rounded-xl border bg-gray-50 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:bg-white transition-colors ${
            errors.name
              ? 'border-red-300 focus:ring-red-400'
              : 'border-gray-200 focus:ring-indigo-500'
          }`}
        />
        {errors.name && (
          <p className="text-xs text-red-500 mt-1">{errors.name.message}</p>
        )}
      </div>

      <div className="flex justify-end gap-2 pt-2">
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
        >
          Hủy
        </button>
        <button
          type="submit"
          disabled={isSubmitting || !isValid}
          className="px-4 py-2 text-sm text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors"
        >
          {isSubmitting ? 'Đang lưu...' : isEdit ? 'Cập nhật' : 'Tạo lớp'}
        </button>
      </div>
    </form>
  );
}
