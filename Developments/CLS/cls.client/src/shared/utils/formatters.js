/**
 * Các hàm định dạng dữ liệu dùng chung trong toàn ứng dụng CLS.
 */

/**
 * Định dạng chuỗi ISO date → DD/MM/YYYY
 * @param {string|null} isoString
 * @returns {string}
 */
export function formatDate(isoString) {
  if (!isoString) return '—';
  return new Date(isoString).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  });
}

/**
 * Định dạng chuỗi ISO datetime → DD/MM/YYYY HH:mm
 * @param {string|null} isoString
 * @returns {string}
 */
export function formatDateTime(isoString) {
  if (!isoString) return '—';
  return new Date(isoString).toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/**
 * Định dạng số tiền → 1.500.000 ₫ (Việt Nam locale)
 * @param {number|null} amount
 * @returns {string}
 */
export function formatCurrency(amount) {
  if (amount == null) return '—';
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount);
}

/**
 * Trả về badge text/class tương ứng với trạng thái entity.
 * @param {string} status
 * @returns {{ label: string, className: string }}
 */
export function getStatusBadge(status) {
  const map = {
    active:          { label: 'Hoạt động',       className: 'bg-green-100 text-green-700' },
    inactive:        { label: 'Ngừng hoạt động',  className: 'bg-gray-100 text-gray-600' },
    suspended:       { label: 'Tạm dừng',        className: 'bg-yellow-100 text-yellow-700' },
    expired:         { label: 'Hết hạn',         className: 'bg-red-100 text-red-600' },
    depleted:        { label: 'Hết buổi',        className: 'bg-orange-100 text-orange-600' },
    scheduled:       { label: 'Đã lên lịch',     className: 'bg-blue-100 text-blue-600' },
    in_progress:     { label: 'Đang diễn ra',    className: 'bg-indigo-100 text-indigo-700' },
    completed:       { label: 'Đã hoàn thành',   className: 'bg-green-100 text-green-700' },
    cancelled:       { label: 'Đã hủy',          className: 'bg-red-100 text-red-600' },
    // Payment statuses (CLS-003)
    pending:         { label: 'Chờ xác nhận',    className: 'bg-yellow-100 text-yellow-700' },
    confirmed:       { label: 'Đã xác nhận',     className: 'bg-green-100 text-green-700' },
    failed:          { label: 'Thất bại',        className: 'bg-red-100 text-red-600' },
    refunded:        { label: 'Đã hoàn tiền',    className: 'bg-purple-100 text-purple-600' },
    // StudentPackage statuses (CLS-003)
    pending_payment: { label: 'Chờ thanh toán',  className: 'bg-yellow-100 text-yellow-700' },
    archived:        { label: 'Đã lưu trữ',     className: 'bg-gray-100 text-gray-500' },
  };
  return map[status] || { label: status, className: 'bg-gray-100 text-gray-500' };
}
