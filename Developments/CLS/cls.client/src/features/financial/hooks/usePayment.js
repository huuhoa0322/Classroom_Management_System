import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  recordPayment,
  updatePaymentStatus,
  getStudentPayments,
  getAllPayments,
  getStudentPackages,
  getAvailablePackages,
} from '../services/paymentService';

/**
 * Hook: Lấy toàn bộ payments (phân trang, không lọc student).
 */
export function useAllPayments(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: ['allPayments', page, pageSize],
    queryFn: () => getAllPayments({ page, pageSize }),
  });
}

/**
 * Hook: Lấy danh sách gói học của học sinh.
 * @param {number} studentId
 */
export function useStudentPackages(studentId) {
  return useQuery({
    queryKey: ['studentPackages', studentId],
    queryFn: () => getStudentPackages(studentId),
    enabled: !!studentId,
  });
}

/**
 * Hook: Lấy lịch sử thanh toán của học sinh (phân trang).
 * @param {number} studentId
 * @param {number} page
 * @param {number} pageSize
 */
export function useStudentPayments(studentId, page = 1, pageSize = 10) {
  return useQuery({
    queryKey: ['studentPayments', studentId, page, pageSize],
    queryFn: () => getStudentPayments(studentId, { page, pageSize }),
    enabled: !!studentId,
  });
}

/**
 * Hook: Lấy catalog gói học (active only).
 */
export function useAvailablePackages() {
  return useQuery({
    queryKey: ['tuitionPackages'],
    queryFn: getAvailablePackages,
  });
}

/**
 * Hook: Ghi nhận thanh toán mới.
 * Sau khi thành công → invalidate cache.
 */
export function useRecordPayment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: recordPayment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['studentPayments'] });
      queryClient.invalidateQueries({ queryKey: ['studentPackages'] });
      queryClient.invalidateQueries({ queryKey: ['allPayments'] });
    },
  });
}

/**
 * Hook: Cập nhật trạng thái thanh toán (confirm / fail / refund).
 */
export function useUpdatePaymentStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }) => updatePaymentStatus(id, { status }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['studentPayments'] });
      queryClient.invalidateQueries({ queryKey: ['studentPackages'] });
      queryClient.invalidateQueries({ queryKey: ['allPayments'] });
    },
  });
}
