import { useEffect, useRef } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useAuthStore } from '@/app/provider/authStore';
import { useNotificationStore } from '@/shared/stores/notificationStore';
import { useToastStore } from '@/shared/stores/toastStore';

/**
 * Hook: Kết nối SignalR NotificationHub — nhận real-time alerts.
 * Chỉ kết nối khi user đã login (có accessToken).
 * Tự động disconnect khi unmount hoặc logout.
 */
export function useNotificationHub() {
  const connectionRef = useRef(null);
  const accessToken = useAuthStore((s) => s.accessToken);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const increment = useNotificationStore((s) => s.increment);
  const addToast = useToastStore((s) => s.addToast);

  useEffect(() => {
    if (!isAuthenticated || !accessToken) return;

    const baseUrl = import.meta.env.VITE_API_BASE_URL || '';
    // Strip /api/v1 suffix to get the base server URL
    const serverUrl = baseUrl.replace(/\/api\/v1\/?$/, '');
    const hubUrl = `${serverUrl}/hubs/notifications`;

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    // ── Listen for NewRenewalAlerts event ─────────────────────────────────
    connection.on('NewRenewalAlerts', (data) => {
      const count = data?.count ?? 0;
      if (count > 0) {
        increment(count);
        addToast({
          type: 'warning',
          message: `🔔 Có ${count} thông báo gia hạn mới cần xử lý!`,
        });
      }
    });

    // ── Start connection ─────────────────────────────────────────────────
    connection
      .start()
      .then(() => console.log('[SignalR] Connected to NotificationHub'))
      .catch((err) => console.warn('[SignalR] Connection failed:', err));

    connectionRef.current = connection;

    // ── Cleanup ──────────────────────────────────────────────────────────
    return () => {
      connection.stop().catch(() => {});
      connectionRef.current = null;
    };
  }, [isAuthenticated, accessToken, increment, addToast]);

  return connectionRef;
}
