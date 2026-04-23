import { useState } from 'react';
import { formatCurrency, formatDateTime, getStatusBadge } from '@/shared/utils/formatters';
import { PAYMENT_STATUS } from '@/shared/utils/constants';

/**
 * PaymentHistoryTable — Bảng lịch sử thanh toán với actions inline.
 *
 * @param {{
 *   payments: Object,
 *   isLoading: boolean,
 *   page: number,
 *   onPageChange: function,
 *   onUpdateStatus: function,
 *   isUpdating: boolean
 * }} props
 */
export function PaymentHistoryTable({
  payments,
  isLoading,
  page,
  onPageChange,
  onUpdateStatus,
  isUpdating,
}) {
  const [confirmAction, setConfirmAction] = useState(null);

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-14 bg-gray-100 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  const items = payments?.items || [];
  const totalPages = payments?.totalPages || 1;

  if (!items.length) {
    return (
      <div className="text-center py-10 text-gray-400">
        <p>Chưa có lịch sử thanh toán.</p>
      </div>
    );
  }

  const handleAction = (paymentId, newStatus) => {
    setConfirmAction({ paymentId, newStatus });
  };

  const executeAction = () => {
    if (confirmAction) {
      onUpdateStatus(confirmAction);
      setConfirmAction(null);
    }
  };

  return (
    <>
      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200">
              <th className="text-left py-3 px-4 font-medium text-gray-500">Ngày</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Gói học</th>
              <th className="text-right py-3 px-4 font-medium text-gray-500">Số tiền</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Trạng thái</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Người ghi</th>
              <th className="text-center py-3 px-4 font-medium text-gray-500">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {items.map((payment) => {
              const badge = getStatusBadge(payment.status);
              return (
                <tr key={payment.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 whitespace-nowrap">
                    {formatDateTime(payment.paymentDate)}
                  </td>
                  <td className="py-3 px-4">{payment.packageName}</td>
                  <td className="py-3 px-4 text-right font-medium">
                    {formatCurrency(payment.amount)}
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className={`text-xs font-medium px-2.5 py-0.5 rounded-full ${badge.className}`}>
                      {badge.label}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-gray-600">{payment.recordedByName}</td>
                  <td className="py-3 px-4 text-center">
                    <div className="flex justify-center gap-1">
                      {payment.status === PAYMENT_STATUS.PENDING && (
                        <>
                          <button
                            onClick={() => handleAction(payment.id, PAYMENT_STATUS.CONFIRMED)}
                            disabled={isUpdating}
                            className="px-2 py-1 text-xs bg-green-50 text-green-700 rounded hover:bg-green-100 transition-colors disabled:opacity-50"
                          >
                            ✓ Xác nhận
                          </button>
                          <button
                            onClick={() => handleAction(payment.id, PAYMENT_STATUS.FAILED)}
                            disabled={isUpdating}
                            className="px-2 py-1 text-xs bg-red-50 text-red-700 rounded hover:bg-red-100 transition-colors disabled:opacity-50"
                          >
                            ✗ Thất bại
                          </button>
                        </>
                      )}
                      {payment.status === PAYMENT_STATUS.CONFIRMED && (
                        <button
                          onClick={() => handleAction(payment.id, PAYMENT_STATUS.REFUNDED)}
                          disabled={isUpdating}
                          className="px-2 py-1 text-xs bg-purple-50 text-purple-700 rounded hover:bg-purple-100 transition-colors disabled:opacity-50"
                        >
                          ↩ Hoàn tiền
                        </button>
                      )}
                      {(payment.status === PAYMENT_STATUS.FAILED ||
                        payment.status === PAYMENT_STATUS.REFUNDED) && (
                        <span className="text-xs text-gray-400">—</span>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-2 mt-4">
          <button
            onClick={() => onPageChange(page - 1)}
            disabled={page <= 1}
            className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            ← Trước
          </button>
          <span className="text-sm text-gray-500">
            Trang {page} / {totalPages}
          </span>
          <button
            onClick={() => onPageChange(page + 1)}
            disabled={page >= totalPages}
            className="px-3 py-1.5 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            Tiếp →
          </button>
        </div>
      )}

      {/* Confirm Dialog */}
      {confirmAction && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-xl shadow-2xl p-6 w-full max-w-sm mx-4">
            <h3 className="font-semibold text-gray-800 mb-2">Xác nhận thao tác</h3>
            <p className="text-sm text-gray-600 mb-5">
              Bạn có chắc chắn muốn chuyển trạng thái thanh toán sang{' '}
              <strong>{getStatusBadge(confirmAction.newStatus).label}</strong>?
            </p>
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setConfirmAction(null)}
                className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200"
              >
                Hủy
              </button>
              <button
                onClick={executeAction}
                className="px-4 py-2 text-sm text-white bg-blue-600 rounded-lg hover:bg-blue-700"
              >
                Xác nhận
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
