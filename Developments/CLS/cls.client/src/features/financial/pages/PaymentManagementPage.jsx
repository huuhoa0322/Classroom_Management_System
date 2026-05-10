import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAllPayments, useUpdatePaymentStatus } from '../hooks/usePayment';
import { PaymentHistoryTable } from '../components/PaymentHistoryTable';
import { ConnectionErrorBanner } from '@/shared/components/ConnectionErrorBanner';
import { toast } from '@/shared/stores/toastStore';

/**
 * PaymentManagementPage — Trang quản lý toàn bộ thanh toán (CLS-003).
 * Route: /payments
 */
export default function PaymentManagementPage() {
  const [page, setPage] = useState(1);

  const { data: payments, isLoading, isError, error, refetch } = useAllPayments(page);
  const updatePaymentStatus = useUpdatePaymentStatus();

  const handleUpdateStatus = ({ paymentId, newStatus }) => {
    updatePaymentStatus.mutate(
      { id: paymentId, status: newStatus },
      {
        onSuccess: () => {
          toast.success('Cập nhật trạng thái thanh toán thành công!');
        },
        onError: (err) => {
          toast.error(err.message || 'Không thể cập nhật trạng thái. Vui lòng thử lại.');
        },
      }
    );
  };

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <Link
            to="/"
            className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 mb-2"
          >
            ← Quay lại Dashboard
          </Link>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý tài chính</h1>
          <p className="text-sm text-gray-500 mt-1">
            Toàn bộ bản ghi thanh toán trong hệ thống
          </p>
        </div>
      </div>

      {/* Error */}
      {isError && (
        <ConnectionErrorBanner error={error} onRetry={() => refetch()} />
      )}

      {/* Table */}
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
        <PaymentHistoryTable
          payments={payments}
          isLoading={isLoading}
          page={page}
          onPageChange={setPage}
          onUpdateStatus={handleUpdateStatus}
          isUpdating={updatePaymentStatus.isPending}
        />
      </div>
    </div>
  );
}
