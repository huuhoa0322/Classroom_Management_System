import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { recordPaymentSchema } from '../schemas/payment.schema';
import { useAvailablePackages } from '../hooks/usePayment';
import { formatCurrency } from '@/shared/utils/formatters';

/**
 * PaymentForm — Form ghi thanh toán mới.
 * Presentational component (nhận onSubmit qua props).
 *
 * @param {{ studentId: number, onSubmit: function, onCancel: function, isSubmitting: boolean }} props
 */
export function PaymentForm({ studentId, onSubmit, onCancel, isSubmitting }) {
  const { data: packages, isLoading: loadingPackages } = useAvailablePackages();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(recordPaymentSchema),
    defaultValues: {
      studentId,
      tuitionPackageId: '',
      amountPaid: '',
      note: '',
    },
  });

  const selectedPkgId = watch('tuitionPackageId');
  const selectedPackage = packages?.find((p) => p.id === Number(selectedPkgId));

  const processSubmit = (data) => {
    onSubmit({
      ...data,
      tuitionPackageId: Number(data.tuitionPackageId),
      amountPaid: Number(data.amountPaid),
    });
  };

  return (
    <form onSubmit={handleSubmit(processSubmit)} className="space-y-5">
      {/* Gói học */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Chọn gói học <span className="text-red-500">*</span>
        </label>
        {loadingPackages ? (
          <div className="h-10 bg-gray-100 rounded animate-pulse" />
        ) : (
          <select
            {...register('tuitionPackageId', { valueAsNumber: true })}
            className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
          >
            <option value="">— Chọn gói —</option>
            {packages?.map((pkg) => (
              <option key={pkg.id} value={pkg.id}>
                {pkg.name} — {formatCurrency(pkg.price)}
              </option>
            ))}
          </select>
        )}
        {errors.tuitionPackageId && (
          <p className="text-red-500 text-xs mt-1">{errors.tuitionPackageId.message}</p>
        )}
        {selectedPackage && (
          <p className="text-xs text-gray-500 mt-1">
            {selectedPackage.totalSessions} buổi • Thời hạn {selectedPackage.durationDays} ngày
          </p>
        )}
      </div>

      {/* Số tiền */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Số tiền (VNĐ) <span className="text-red-500">*</span>
        </label>
        <input
          type="number"
          step="1000"
          {...register('amountPaid', { valueAsNumber: true })}
          placeholder="Ví dụ: 1500000"
          className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
        />
        {errors.amountPaid && (
          <p className="text-red-500 text-xs mt-1">{errors.amountPaid.message}</p>
        )}
      </div>

      {/* Phương thức (cố định) */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Phương thức</label>
        <div className="flex items-center gap-2 px-3 py-2.5 bg-gray-50 border border-gray-200 rounded-lg text-sm text-gray-600">
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
          </svg>
          Chuyển khoản ngân hàng
        </div>
      </div>

      {/* Ghi chú */}
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">Ghi chú</label>
        <textarea
          {...register('note')}
          rows={3}
          placeholder="Thông tin chuyển khoản, số tham chiếu..."
          className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none resize-none"
        />
        {errors.note && (
          <p className="text-red-500 text-xs mt-1">{errors.note.message}</p>
        )}
      </div>

      {/* Trạng thái khởi tạo */}
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3 text-sm text-yellow-800">
        ⏳ Trạng thái ban đầu: <strong>Chờ xác nhận</strong> — cần Admin xác nhận sau khi nhận CK.
      </div>

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
          {isSubmitting ? 'Đang xử lý...' : 'Ghi thanh toán'}
        </button>
      </div>
    </form>
  );
}
