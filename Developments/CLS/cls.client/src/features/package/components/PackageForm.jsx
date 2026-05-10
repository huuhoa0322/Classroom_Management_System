import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { packageSchema } from '../schemas/packageSchema';

export function PackageForm({ onSubmit, onCancel, isSubmitting, defaultValues }) {
  const isEdit = !!defaultValues;
  const { register, handleSubmit, formState: { errors, isValid } } = useForm({
    resolver: zodResolver(packageSchema),
    defaultValues: defaultValues || { name: '', totalSessions: '', durationDays: '', price: '' },
    mode: 'onChange',
  });

  const fieldClass = (err) => `w-full rounded-xl border bg-gray-50 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:bg-white transition-colors ${err ? 'border-red-300 focus:ring-red-400' : 'border-gray-200 focus:ring-indigo-500'}`;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Tên gói <span className="text-red-500">*</span></label>
        <input {...register('name')} autoFocus placeholder="VD: Gói 30 buổi" className={fieldClass(errors.name)} />
        {errors.name && <p className="text-xs text-red-500 mt-1">{errors.name.message}</p>}
      </div>
      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Số buổi <span className="text-red-500">*</span></label>
          <input type="number" {...register('totalSessions')} placeholder="30" className={fieldClass(errors.totalSessions)} />
          {errors.totalSessions && <p className="text-xs text-red-500 mt-1">{errors.totalSessions.message}</p>}
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Thời hạn (ngày) <span className="text-red-500">*</span></label>
          <input type="number" {...register('durationDays')} placeholder="90" className={fieldClass(errors.durationDays)} />
          {errors.durationDays && <p className="text-xs text-red-500 mt-1">{errors.durationDays.message}</p>}
        </div>
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Giá gói (VNĐ) <span className="text-red-500">*</span></label>
        <input type="number" {...register('price')} placeholder="1500000" className={fieldClass(errors.price)} />
        {errors.price && <p className="text-xs text-red-500 mt-1">{errors.price.message}</p>}
      </div>
      <div className="flex justify-end gap-2 pt-2">
        <button type="button" onClick={onCancel} className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">Hủy</button>
        <button type="submit" disabled={isSubmitting || !isValid} className="px-4 py-2 text-sm text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors">
          {isSubmitting ? 'Đang lưu...' : isEdit ? 'Cập nhật' : 'Tạo gói'}
        </button>
      </div>
    </form>
  );
}
