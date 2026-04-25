import { create } from 'zustand';

/**
 * Zustand store cho notification badge — theo dõi số thông báo chưa đọc.
 * Được cập nhật bởi useNotificationHub khi nhận SignalR event.
 */
export const useNotificationStore = create((set) => ({
  /** Số alert mới chưa đọc */
  unreadCount: 0,

  /** Tăng unread count (gọi khi nhận SignalR event) */
  increment: (count = 1) => set((state) => ({ unreadCount: state.unreadCount + count })),

  /** Reset về 0 (gọi khi user navigate tới trang Renewal Alerts) */
  reset: () => set({ unreadCount: 0 }),
}));
