/**
 * ConnectionErrorBanner — Banner thông báo lỗi kết nối / tải dữ liệu.
 * Dùng chung cho tất cả các trang khi query thất bại (mất mạng, server lỗi, ...).
 *
 * @param {{ message?: string, error?: Error, onRetry?: function }} props
 */
export function ConnectionErrorBanner({ message, error, onRetry }) {
  const displayMessage = message
    || error?.message
    || 'Không thể tải dữ liệu. Vui lòng kiểm tra kết nối mạng và thử lại.';

  return (
    <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-center gap-3 mb-6">
      <span className="text-2xl shrink-0">📡</span>
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-amber-800">
          Không thể tải dữ liệu từ máy chủ
        </p>
        <p className="text-xs text-amber-600 mt-0.5 truncate" title={displayMessage}>
          {displayMessage}
        </p>
      </div>
      {onRetry && (
        <button
          onClick={onRetry}
          className="px-3 py-1.5 bg-amber-600 text-white text-xs font-medium rounded-lg hover:bg-amber-700 transition-colors shrink-0"
        >
          🔄 Thử lại
        </button>
      )}
    </div>
  );
}
