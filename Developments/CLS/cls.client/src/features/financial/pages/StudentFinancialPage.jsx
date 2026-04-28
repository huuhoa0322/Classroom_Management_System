import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  useStudentPackages,
  useStudentPayments,
  useRecordPayment,
  useUpdatePaymentStatus,
} from '../hooks/usePayment';
import { useStudentDetail } from '@/features/student/hooks/useStudents';
import { StudentPackageList } from '../components/StudentPackageList';
import { PaymentHistoryTable } from '../components/PaymentHistoryTable';
import { PaymentForm } from '../components/PaymentForm';
import { toast } from '@/shared/stores/toastStore';

/**
 * StudentFinancialPage — Trang tài chính của học sinh (CLS-003).
 * Route: /students/:id/financials
 */
export default function StudentFinancialPage() {
  const { id } = useParams();
  const studentId = Number(id);

  const [paymentPage, setPaymentPage] = useState(1);
  const [showPaymentForm, setShowPaymentForm] = useState(false);

  // Data hooks
  const { data: student, isLoading: loadingStudent } = useStudentDetail(studentId);
  const { data: packages, isLoading: loadingPackages, isError: errorPackages } = useStudentPackages(studentId);
  const { data: payments, isLoading: loadingPayments, isError: errorPayments } = useStudentPayments(studentId, paymentPage);

  // Mutation hooks
  const recordPayment = useRecordPayment();
  const updatePaymentStatus = useUpdatePaymentStatus();

  const handleRecordPayment = (data) => {
    recordPayment.mutate(data, {
      onSuccess: () => {
        setShowPaymentForm(false);
        toast.success('Ghi thanh toán thành công!');
      },
      onError: (err) => {
        toast.error(err.message || 'Không thể ghi thanh toán. Vui lòng thử lại.');
      },
    });
  };

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
    <div className="max-w-6xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <Link
            to="/students"
            className="text-sm text-blue-600 hover:text-blue-800 flex items-center gap-1 mb-2"
          >
            ← Quay lại danh sách học sinh
          </Link>
          <h1 className="text-2xl font-bold text-gray-800">
            Tài chính học sinh {loadingStudent ? '...' : (student?.fullName || `#${studentId}`)}
          </h1>
          <p className="text-gray-500 text-sm mt-1">
            Quản lý gói học và lịch sử thanh toán
          </p>
        </div>
        <button
          onClick={() => setShowPaymentForm(true)}
          className="px-4 py-2.5 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 shadow-sm"
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
          Ghi thanh toán mới
        </button>
      </div>

      {/* Error Banner */}
      {(errorPackages || errorPayments) && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-sm text-red-700 flex items-center gap-2">
          <svg className="w-5 h-5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
          </svg>
          Không thể tải dữ liệu tài chính. Vui lòng thử lại sau.
        </div>
      )}

      {/* Gói học */}
      <section>
        <h2 className="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
          <svg className="w-5 h-5 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
          </svg>
          Gói học
        </h2>
        <StudentPackageList packages={packages} isLoading={loadingPackages} />
      </section>

      {/* Lịch sử thanh toán */}
      <section>
        <h2 className="text-lg font-semibold text-gray-800 mb-4 flex items-center gap-2">
          <svg className="w-5 h-5 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
          Lịch sử thanh toán
        </h2>
        <div className="bg-white border border-gray-200 rounded-xl shadow-sm p-4">
          <PaymentHistoryTable
            payments={payments}
            isLoading={loadingPayments}
            page={paymentPage}
            onPageChange={setPaymentPage}
            onUpdateStatus={handleUpdateStatus}
            isUpdating={updatePaymentStatus.isPending}
            showStudentColumn={false}
          />
        </div>
      </section>

      {/* Modal — Ghi thanh toán mới */}
      {showPaymentForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
          <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Ghi thanh toán mới</h3>
            <PaymentForm
              studentId={studentId}
              onSubmit={handleRecordPayment}
              onCancel={() => setShowPaymentForm(false)}
              isSubmitting={recordPayment.isPending}
            />
            {recordPayment.isError && (
              <p className="text-red-500 text-sm mt-3">
                Lỗi: {recordPayment.error?.message || 'Không thể ghi thanh toán.'}
              </p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
