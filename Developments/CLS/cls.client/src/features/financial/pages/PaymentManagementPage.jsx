import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAllPayments, useUpdatePaymentStatus } from '../hooks/usePayment';
import { PaymentHistoryTable } from '../components/PaymentHistoryTable';

/**
 * PaymentManagementPage — Trang quản lý toàn bộ thanh toán (CLS-003).
 * Route: /payments
 */
export default function PaymentManagementPage() {
  const [page, setPage] = useState(1);

  const { data: payments, isLoading, isError } = useAllPayments(page);
  const updatePaymentStatus = useUpdatePaymentStatus();

  const handleUpdateStatus = ({ paymentId, newStatus }) => {
    updatePaymentStatus.mutate({ id: paymentId, status: newStatus });
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
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700 mb-6 flex items-center gap-2">
          <svg className="w-5 h-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          Không thể tải dữ liệu thanh toán. Vui lòng thử lại sau.
        </div>
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
