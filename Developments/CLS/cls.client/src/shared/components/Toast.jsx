import { useToastStore } from '../stores/toastStore';

/**
 * Toast — Global notification component.
 * Hiển thị ở góc trên phải, auto-dismiss, hỗ trợ success/error/warning/info.
 * Render 1 lần duy nhất trong Layout.
 */
export function Toast() {
  const toasts = useToastStore((s) => s.toasts);
  const removeToast = useToastStore((s) => s.removeToast);

  if (!toasts.length) return null;

  return (
    <div className="fixed top-4 right-4 z-[9999] flex flex-col gap-2 pointer-events-none">
      {toasts.map((t) => (
        <div
          key={t.id}
          className={`pointer-events-auto max-w-sm w-full rounded-lg shadow-lg border px-4 py-3 flex items-start gap-3 animate-slide-in ${getStyles(t.type)}`}
        >
          <span className="text-lg flex-shrink-0 mt-0.5">{getIcon(t.type)}</span>
          <p className="text-sm flex-1 break-words">{t.message}</p>
          <button
            onClick={() => removeToast(t.id)}
            className="text-current opacity-40 hover:opacity-70 text-lg leading-none flex-shrink-0"
            aria-label="Đóng"
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
}

function getStyles(type) {
  switch (type) {
    case 'success':
      return 'bg-emerald-50 border-emerald-200 text-emerald-800';
    case 'error':
      return 'bg-red-50 border-red-200 text-red-800';
    case 'warning':
      return 'bg-amber-50 border-amber-200 text-amber-800';
    default:
      return 'bg-blue-50 border-blue-200 text-blue-800';
  }
}

function getIcon(type) {
  switch (type) {
    case 'success':
      return '✅';
    case 'error':
      return '❌';
    case 'warning':
      return '⚠️';
    default:
      return 'ℹ️';
  }
}
