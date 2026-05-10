import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { createUserSchema, updateUserSchema } from '../schemas/userSchema';

export function UserForm({ onSubmit, onCancel, isSubmitting, defaultValues }) {
  const isEdit = !!defaultValues;
  const { register, handleSubmit, formState: { errors, isValid } } = useForm({
    resolver: zodResolver(isEdit ? updateUserSchema : createUserSchema),
    defaultValues: defaultValues || { fullName: '', email: '', phone: '', password: '' },
    mode: 'onChange',
  });

  const fieldClass = (err) => `w-full rounded-xl border bg-gray-50 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:bg-white transition-colors ${err ? 'border-red-300 focus:ring-red-400' : 'border-gray-200 focus:ring-indigo-500'}`;

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Họ tên <span className="text-red-500">*</span></label>
        <input {...register('fullName')} autoFocus placeholder="VD: Nguyễn Văn A" className={fieldClass(errors.fullName)} />
        {errors.fullName && <p className="text-xs text-red-500 mt-1">{errors.fullName.message}</p>}
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Email <span className="text-red-500">*</span></label>
        <input type="email" {...register('email')} placeholder="teacher@cls.edu.vn" className={fieldClass(errors.email)} />
        {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email.message}</p>}
      </div>
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Số điện thoại</label>
        <input {...register('phone')} placeholder="0901234567" className={fieldClass(errors.phone)} />
        {errors.phone && <p className="text-xs text-red-500 mt-1">{errors.phone.message}</p>}
      </div>
      {!isEdit && (
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Mật khẩu <span className="text-red-500">*</span></label>
          <input type="password" {...register('password')} placeholder="Tối thiểu 6 ký tự" className={fieldClass(errors.password)} />
          {errors.password && <p className="text-xs text-red-500 mt-1">{errors.password.message}</p>}
        </div>
      )}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-xs text-blue-700">
        💡 {isEdit ? 'Để đặt lại mật khẩu, sử dụng nút "Đặt lại MK" trong danh sách.' : 'Tài khoản được tạo sẽ có vai trò Giáo viên (Teacher).'}
      </div>
      <div className="flex justify-end gap-2 pt-2">
        <button type="button" onClick={onCancel} className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">Hủy</button>
        <button type="submit" disabled={isSubmitting || !isValid} className="px-4 py-2 text-sm text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 transition-colors">
          {isSubmitting ? 'Đang lưu...' : isEdit ? 'Cập nhật' : 'Tạo tài khoản'}
        </button>
      </div>
    </form>
  );
}
