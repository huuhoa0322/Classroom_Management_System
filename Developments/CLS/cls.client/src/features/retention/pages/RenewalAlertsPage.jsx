import { useEffect } from 'react';
import { RenewalAlertTable } from '../components/RenewalAlertTable';
import { useNotificationStore } from '@/shared/stores/notificationStore';

/**
 * RenewalAlertsPage — Trang quản lý thông báo gia hạn gói học (CLS-006).
 * Reset notification badge khi user navigate vào trang này.
 */
export default function RenewalAlertsPage() {
  const reset = useNotificationStore((s) => s.reset);

  // Reset unread count khi vào trang
  useEffect(() => {
    reset();
  }, [reset]);

  return (
    <div className="space-y-6">
      {/* ── Header ──────────────────────────────────────────────────── */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">🔔 Thông báo gia hạn</h1>
        <p className="text-sm text-gray-500 mt-1">
          Danh sách học sinh có gói học sắp hết buổi hoặc sắp hết hạn. 
          Hãy liên hệ phụ huynh để tư vấn gia hạn.
        </p>
      </div>

      {/* ── Info Banner ─────────────────────────────────────────────── */}
      <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 flex items-start gap-3">
        <span className="text-xl mt-0.5">⚡</span>
        <div className="text-sm text-amber-800">
          <p className="font-medium">Hệ thống tự động quét hàng ngày</p>
          <p className="mt-0.5 text-amber-600">
            Gói học có ≤ 4 buổi còn lại hoặc ≤ 14 ngày trước khi hết hạn sẽ được cảnh báo tự động.
          </p>
        </div>
      </div>

      {/* ── Table ───────────────────────────────────────────────────── */}
      <RenewalAlertTable />
    </div>
  );
}
