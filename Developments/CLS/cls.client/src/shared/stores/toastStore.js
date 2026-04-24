import { create } from 'zustand';

let toastId = 0;

/**
 * Zustand store quản lý toast notifications.
 * Dùng chung cho toàn bộ ứng dụng CLS.
 */
export const useToastStore = create((set) => ({
  toasts: [],

  addToast: ({ type = 'info', message, duration = 5000 }) => {
    const id = ++toastId;
    set((state) => ({
      toasts: [...state.toasts, { id, type, message }],
    }));

    // Auto-dismiss
    if (duration > 0) {
      setTimeout(() => {
        set((state) => ({
          toasts: state.toasts.filter((t) => t.id !== id),
        }));
      }, duration);
    }

    return id;
  },

  removeToast: (id) =>
    set((state) => ({
      toasts: state.toasts.filter((t) => t.id !== id),
    })),
}));

/**
 * Helper — gọi tắt toast từ bất kỳ đâu (không cần hook).
 *
 * Cách dùng:
 *   import { toast } from '@/shared/stores/toastStore';
 *   toast.success('Tạo thành công!');
 *   toast.error('Có lỗi xảy ra.');
 */
export const toast = {
  success: (message) =>
    useToastStore.getState().addToast({ type: 'success', message }),
  error: (message) =>
    useToastStore.getState().addToast({ type: 'error', message, duration: 7000 }),
  warning: (message) =>
    useToastStore.getState().addToast({ type: 'warning', message }),
  info: (message) =>
    useToastStore.getState().addToast({ type: 'info', message }),
};
